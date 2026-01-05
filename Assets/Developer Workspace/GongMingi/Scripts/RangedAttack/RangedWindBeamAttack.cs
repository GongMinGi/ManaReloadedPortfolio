using Game.Combat.Stats;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// * 개발자 : 공민기
/// 원거리 바람 빔 공격을 관리하는 클래스.
/// 마우스 왼쪽 버튼을 누르는 동안 전방으로 빔을 발사하며 틱 데미지와 슬로우 효과를 준다.
/// </summary>
public class RangedWindBeamAttack : MonoBehaviour, IRangedAttack 
{
    public int? AnimationBoolHash => throw new NotImplementedException();
    public int? AnimationTriggerHash => throw new NotImplementedException();
    public bool UseProgress => throw new NotImplementedException();

    // 공격 상태 알림을 위한 이벤트들
    public event Action OnAttackStarted;            // 공격 시작 시 발생
    public event Action<float> OnProgressUpdated;    // (현재미사용) 진행률 알림음
    public event Action OnAttackEnded;              // 공격 정상 종료 시 발생
    public event Action OnAttackInterrupted;        // 공격 중단 시 발생

    [Header("VFX Setting")]
    [SerializeField] private ParticleSystem beamLoopParticle;

    [Header("Wind Beam Setting")]
    [SerializeField] private float maxDistance = 30f;
    [SerializeField] private float maxDuration = 6f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private int damagePerTick = 150;
    [SerializeField] private Vector2 beamSize = new Vector2(2f, 30f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("VFX")]
    [SerializeField] private Transform muzzle;

    [Header("Slow Effect Settings")]
    [SerializeField] private int slowEffectId;                 // 슬로우 효과 ID (EffectHandler에서 구분용)
    [SerializeField] private float slowDuration = 0.5f;        // 슬로우 효과 지속 시간
    [SerializeField] private float slowAmount = 0.8f;          // 슬로우 강도 (곱연산 비율, ex. 0.8f → 20% 슬로우)

    private SlowEffect windSlowEffect;                         // 슬로우 상태 효과 객체
    private Coroutine beamRoutine;                       // 현재 실행 중인 빔 코루틴 참조
    private bool isFiring = false;                       // 발사 중인지 여부

    private void Awake()
    {
        if(muzzle == null)
        {
            muzzle = transform;
        }

        windSlowEffect = new SlowEffect(slowDuration, false, slowEffectId, slowAmount, ModifierMode.Multiply);
    }

    /// <summary>
    /// 공격을 실행합니다. 이미 발사 중이면 중복 실행을 방지합니다.
    /// </summary>
    public void ExecuteAttack(E_CastingType type)
    {
        if (isFiring)
        {
            return;
        }

        beamRoutine = StartCoroutine(FireBeam());
    }

    /// <summary>
    /// 공격을 강제로 정지시킴.
    /// </summary>
    public void Stop()
    {
        if (isFiring == false)
        {
            return;
        }

        StopCoroutine(beamRoutine);
        isFiring = false;
        OnAttackInterrupted?.Invoke();

        beamLoopParticle.Stop(true);
        beamLoopParticle.Clear(true);
    }

    /// <summary>
    /// 빔 발사 로직을 처리하는 코루틴.
    /// </summary>
    IEnumerator FireBeam()
    {
        isFiring = true;
        OnAttackStarted?.Invoke();

        float startTime = Time.time;
        float nextTicktime = 0f;

        beamLoopParticle.Play();

        while (Mouse.current.leftButton.isPressed && Time.time - startTime < maxDuration)
        {
            Vector3 origin = muzzle.position;
            Vector3 dir = muzzle.forward;
            float finalDistance = maxDistance;

            // 1. 장애물 체크: 레이캐스트를 쏘아 벽이 있으면 빔 거리를 조절
            if (Physics.Raycast(origin, dir, out RaycastHit obsHit, maxDistance, obstacleLayer))
            {
                finalDistance = obsHit.distance;
            }

            // 2. 적 충돌 판정: BoxCast를 사용하여 직육면체 범위 내의 모든 적을 검출
            Vector3 halfExtents = new Vector3(beamSize.x * 0.5f, beamSize.y * 0.5f, 0.5f);

            RaycastHit[] enemyHits = Physics.BoxCastAll(
                origin,
                halfExtents,
                dir,
                Quaternion.LookRotation(dir),
                maxDistance,
                enemyLayer,
                QueryTriggerInteraction.Ignore
            );

            // 3. 틱 처리: 설정한 간격(tickInterval)마다 데미지와 디버프 적용
            if (Time.time >= nextTicktime)
            {
                foreach (var hit in enemyHits)
                {
                    // 적의 스탯 컴포넌트를 가져와 데미지 및 슬로우 적용
                    if (hit.collider.TryGetComponent(out UnitStats target))
                    {
                        target.TakeDamage(damagePerTick);
                        target.EffectHandler.AddStatusEffect(windSlowEffect);   // StatusEffectHandler를 통해 슬로우 효과 추가
                    }
                }

                nextTicktime = Time.time + tickInterval;
            }
            yield return null;  
        }

        // 루프 종료 후 파티클 정지 및 청소
        beamLoopParticle.Stop(true);
        beamLoopParticle.Clear(true);
        isFiring = false;
    }
}

#region legacy
//private RangedAttackContext _ctx;


//public void BindContext(RangedAttackContext ctx)
//{
//    _ctx = ctx;
//}

#endregion