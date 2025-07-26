using System;
using System.Collections;
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
public class RangedBeamAttack : MonoBehaviour, IRangedAttack
{
    #region Field and Property

    [Header("Beam Settings")]
    [SerializeField] private float maxDistance = 15f;       // 빔 최대 사거리
    [SerializeField] private float maxDuration = 4f;        // 한 번에 지속 가능한 최대 시간
    [SerializeField] private float tickInterval = 0.25f;    // 피해 주기
    [SerializeField] private int damagePerTick = 6;         // 틱당 피해량
    [SerializeField] private LayerMask enemyLayer;          // 적 레이어
    [SerializeField] private LayerMask obstacleLayer;       // 빔을 막는 지형 레이어


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

    #region IRangedAttack Implementation

    public void ExecuteAttack(E_CastingType type)
    {       
        if (isFiring) return;                               // 이미 발사 중이면 무시
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
    }

    #endregion

    #region Beam Implementation

    IEnumerator FireBeam()
    {

        isFiring = true;                                    // 발사 상태 ON
        lr.enabled = true;                                  // 라인 표시 ON

        float startTime = Time.time;                        // 현재 시간을 시작 시간으로 설정

        RaycastHit[] hits = new RaycastHit[8];              // RaycastNonAlloc 용 버퍼 (GC 방지) => 변수 위치 조정 필요

        // 좌클릭이 눌려 있고, 최대 지속 시간을 넘지 않을 때까지 루프
        while (Mouse.current.leftButton.isPressed && Time.time -startTime < maxDuration)
        {
            Vector3 origin = muzzle.position;               // 레이 시작점
            Vector3 dir = muzzle.forward;                   // 발사 방향( transform.forward)

            // 장애물, 적 탐지 레이케스트 ( 할당 없는 NonAlloc 버전) => deprecated  됨 다른거로 바꿔야 할듯
            int hitCount = Physics.RaycastNonAlloc(
                origin,                                     // 시작점
                dir,                                        // 방향
                hits,                                       // 결과를 담을 배열         
                maxDistance,                                // 최대 거리
                enemyLayer | obstacleLayer,                 // 탐지할 레이어 마스크
                QueryTriggerInteraction.Collide);            // 트리거 Collider 무시 ?? 

            float beamLength = maxDistance;                 // 최종 빔 길이 초기값.

            for (int i = 0;  i < hitCount; ++i)             // 빔에 맞은 개수만큼 순회
            {
                Debug.Log("충돌확인");

                RaycastHit hit = hits[i];

                if (hit.collider.isTrigger) continue;

                if (hit.distance < beamLength)              // 더 가까운 무언가에 맞으면
                    beamLength = hit.distance;              // 빔 길이 단축

                // 적 레이어에 맞았는지 판단
                if(enemyLayer.Contain(hit.collider.gameObject.layer))       // 확장메서드를 이용하여 레이어 마스크 판단.
                {
                    Debug.Log("데미지 적용");
                    if (hit.collider.TryGetComponent(out IDamageable target))
                        target.TakeDamage(damagePerTick);
                }
            }

            // 라인 렌더러 길이 갱신
            lr.SetPosition(0, origin);                      // 시작점
            lr.SetPosition(1, origin + dir * beamLength);   // 충돌지점( 혹은 최대 사거리 지점)


            yield return new WaitForSeconds(tickInterval);  // 다음 틱(피해주기)까지 대기

        }

        //종료 처리
        lr.enabled = false;                                 //빔 숨김
        isFiring = false;                                   // 상태 리셋 ( 빔 발사 중 false 변경)            

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
