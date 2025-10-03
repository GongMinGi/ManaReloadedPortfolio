using Game.Combat.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// * 작성자 : 공민기
///   - 빛 / 어둠 속성 전용
///   - 캐스팅한 원소에 빛이나 어둠속성이 포함되어 있을 때 실행되는 원거리 공격
///   - 마우스 좌클릭을 누르고 있는 동안 빔 형태의 공격이 계속 나가는 홀드형 공격
///   - 계속 홀드하고 있더라도 마법 시전시간이 끝나면 공격이 끝난다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RangedBeamAttack : MonoBehaviour, IRangedAttack, IRequireAttackContext, IAttackSignals
{
    #region Field and Property

    private RangedAttackContext _ctx;        // 변수 이름 수정 필요

    [Header("VFX Setting")]     // 구현: 이예린
    //[SerializeField] VFXObject beamVFX;
    //[SerializeField] ParticleSystem vfxObjects;
    [SerializeField] private ParticleSystem beamloopParticle;   // 
    [SerializeField] private ParticleSystem impactParticle;     // 충돌지점 이펙트
    [SerializeField] private float surfaceOffset = 0.02f;       // z-fighting 방지 

    public event Action Started;
    public event Action<float> Progress;
    public event Action Ended;
    public event Action Interrupted;


    [Header("Beam Settings")]
    [SerializeField] private float maxDistance = 15f;       // 빔 최대 사거리
    [SerializeField] private float maxDuration = 4f;        // 한 번에 지속 가능한 최대 시간
    [SerializeField] private float tickInterval = 0.25f;    // 피해 주기
    [SerializeField] private int damagePerTick = 6;         // 틱당 피해량
    [SerializeField] private LayerMask enemyLayer;          // 적 레이어
    [SerializeField] private LayerMask obstacleLayer;       // 빔을 막는 지형 레이어

    [Header("SoundSetting")]
    [SerializeField] int sfxId = 110016;                                                        // 재생할 사운드 리소스 아이디


    [Header("Visual")]
    [SerializeField] private Transform muzzle;              // 빔 시작 지점

    [Tooltip("For prototype visualization of beam")]
    [SerializeField] private LineRenderer lr;               // 빔을 시각적으로 표현하기 위한 라인 렌더러 

    private Coroutine beamRoutine;                          // 현재 실행중인 빔 코루틴 핸들
    private bool isFiring;                                  // 빔 공격 실행 중 여부

    #endregion

    #region Unity Event

    private void Awake()
    {
        lr.enabled = false;                                 // 초기화 시에 일단 라인랜더러를 꺼 놓는다.
        if (muzzle == null) muzzle = transform;             // 디폴트 : 플레이어 transform
    }

    #endregion


    public void BindContext(RangedAttackContext ctx) => _ctx = ctx;


    #region IRangedAttack Implementation

    public void ExecuteAttack(E_CastingType type)
    {       
        if (isFiring) return;                               // 이미 발사 중이면 무시
        //GameModeManager.SoundManager.PlaySFX(110016);       // 빔 발사 사운드 , 하드코딩으로 빌드용 버그 없이 수정
        beamRoutine = StartCoroutine(FireBeam());           // 빔 코루틴 시작
      
    }

    /// <summary>
    /// 외부에서 코루틴을 정지시키고 싶을 때 사용한다
    /// </summary>
    public void Stop()
    {
        if (!isFiring) return;                              // 발사 중이 아니면 무시
        StopCoroutine(beamRoutine);                         // 코루틴 종료
        lr.enabled = false;                                 // 라인 숨김
        isFiring = false;                                   // 상태 리셋
        Interrupted?.Invoke();          // 애니메이션 강제취소 신호
    }
    #endregion



    #region Beam Implementation

    IEnumerator FireBeam()
    {

        isFiring = true;                                    // 발사 상태 ON
        //lr.enabled = true;                                // 라인 표시 ON
        Started?.Invoke();                                  // 시작 신호

        if(beamloopParticle) beamloopParticle.Play(true);

        float startTime = Time.time;                        // 현재 시간을 시작 시간으로 설정 
        float nextTickTime = 0f;                            // 다음 데미지 틱 시간 (쿨다운 타이머)

        // 좌클릭이 눌려 있고, 최대 지속 시간을 넘지 않을 때까지 루프
        while (Mouse.current.leftButton.isPressed && Time.time -startTime < maxDuration)
        {
            Vector3 origin = muzzle.position;               // 레이 시작점
            Vector3 dir = muzzle.forward;                   // 발사 방향( transform.forward)
            
            float beamLength = maxDistance;                 // 최종 빔 길이 초기값.
            RaycastHit hit;                                 // 단일 Raycast 결과 저장용

            // 장애물 / 적 레이어에만 충돌 검사 (트리거는 무시)
            if (Physics.Raycast(origin, dir, out hit, maxDistance,
                enemyLayer | obstacleLayer , QueryTriggerInteraction.Ignore))
            {
                beamLength = hit.distance;                                      // 빔 길이를 충돌지점까지로 줄임

                // 빔이 적/ 장애물을 맞췄을 때 위치/회전 갱신 
                if(impactParticle)
                {
                    impactParticle.transform.SetPositionAndRotation(
                        hit.point + hit.normal * surfaceOffset,                 // 충돌 지점
                        Quaternion.LookRotation(hit.normal)                     // 표면 법선 방향으로 회전
                    );
                    if (!impactParticle.isPlaying) impactParticle.Play(true);   // 꺼져 있으면 킨다
                }

                // 데미지는 틱 간격 으로만 적용 ( 프레임마다가 아님)
                bool hitEnemy = enemyLayer.Contain(hit.collider.gameObject.layer);
                if( hitEnemy && Time.time >= nextTickTime )
                {
                    if (hit.collider.TryGetComponent(out UnitStats target))
                        target.TakeDamage(damagePerTick);                       // 데미지 1틱 적용

                    nextTickTime = Time.time + tickInterval;                    // 다음 틱 시간 갱신
                }

            }
            else
            {
                // 히트가 없으면 임팩트 파티클 끄기 ( 잔상 없이 자연스럽게)
                if( impactParticle && impactParticle.isPlaying)
                    impactParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting );
            }


            //// 라인 렌더러 길이 갱신
            //lr.SetPosition(0, origin);                      // 시작점
            //lr.SetPosition(1, origin + dir * beamLength);   // 충돌지점( 혹은 최대 사거리 지점)



            // 전체 지속시간 대비 진행률 이벤트 ( UI 게이지 등에서 활용 가능)
            float t = Mathf.InverseLerp(0f, maxDuration, Time.time - startTime);
            Progress?.Invoke(t);


            yield return null;
        }

        //종료 처리
        //lr.enabled = false;                                 //빔 라인 숨김 
        if (beamloopParticle) beamloopParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (impactParticle && impactParticle.isPlaying)
            impactParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        //GameModeManager.SoundManager.StopSFX();
        isFiring = false;                                   // 상태 리셋 ( 빔 발사 중 false 변경)            
        Ended?.Invoke();                                    // 애니메이션 정상 종료
    }

    #endregion

#if UNITY_EDITOR // SCENE 뷰 디버그
    void OnDrawGizmoSelected()
    {
        if (muzzle == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(muzzle.position, muzzle.position + muzzle.forward * maxDistance);
    }


#endif

}
