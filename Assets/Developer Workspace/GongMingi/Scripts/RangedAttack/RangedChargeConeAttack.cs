using Game.Combat.Stats;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.UI;

/// <summary>
/// * 작성자 : 공민기
///   - 전기 속성 전용 '차지 후 부채꼴' 공격
///   - 캐스팅한 원소에 전기 속성이 포함되어 잇는 경우 시전되는 원거리 공격
///   - 마우스 좌클릭을 누르는 동안 충전하고 때는 순간에 마법을 발사한다
///   
/// </summary>
public class RangedChargeConeAttack : MonoBehaviour, IRangedAttack, IRequireAttackContext, IAttackSignals
{
    #region Field and Property

    private RangedAttackContext _ctx;

    [Header("VFX Setting")]     // 구현: 이예린
    [SerializeField] VFXObject chargeVFX;
    [SerializeField] VFXObject attackVFX;

    public event Action Started;
    public event Action<float> Progress;
    public event Action Ended;
    public event Action Interrupted;

    [Header("Cone Parameters")]
    [SerializeField] private float radius = 6f;                 // 적 탐지 반경
    [SerializeField] private float angle = 60f;                 // 전체 부채꼴 각도
    [SerializeField] private bool flatcone = true;              // y축 높이 무시

    [Header("Charge Parameter")]
    [SerializeField] private int baseDamage = 12;               // 최소 데미지
    [SerializeField] private int damageStep = 8;                // 1스택당 증가량
    [SerializeField] private int maxDamage = 60;                // 상한선
    [SerializeField] private float chargeInterval = 0.5f;       // 스택 주기 (sec) 


    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;              // 감지할 적 레이어 

    float cosThreshold;                                         // 공격범위 부채꼴 각도
    int currentDamage;                                          // 현재 누적(스택)데미지
    bool isCharging;                                            // 차지 중 여부
    Coroutine chargeRoutine;                                    // 코루틴 핸들러

    #endregion


    #region Unity Event

    /// <summary>
    /// - 인스펙터에 들어온 각도 값으로 부채꼴(공격범위)의 각을 정한다.
    /// </summary>
    private void Awake() => cosThreshold = Mathf.Cos(angle * 0.5f *  Mathf.Deg2Rad);

    #endregion

    public void BindContext(RangedAttackContext ctx) => _ctx = ctx;


    #region Interface Implementation
    public void ExecuteAttack(E_CastingType type)
    {
        if (isCharging) return;                                  // 중복 방지
        Debug.Log("차지시작");
        chargeRoutine = StartCoroutine(ChargeProcess());         
        
    }

    public void Stop()                                           // 외부에서 강제 취소하고 싶을 때
    {
        if(!isCharging) return;                                  // 차지 취소
        StopCoroutine(chargeRoutine);
        isCharging = false;
        Interrupted?.Invoke();          // 애니메이션 강제취소   
    }
    #endregion


    #region Charge And Attack
    IEnumerator ChargeProcess()
    {
        isCharging = true;                                        // 차지 중 여부 true 로 전환
        currentDamage = baseDamage;                               // 초기 데미지 설정
        float elapsed = 0f;                                       // 차지 간격 타이머

        Started?.Invoke();              // 차지 시작
        chargeVFX.Play();               // 차징 VFX 실행

        // 차지 단계
        while (Mouse.current.leftButton.isPressed)                // 버튼 홀드 감지
        {
            elapsed += Time.deltaTime;                            // 타이머 증가

            if(elapsed >= chargeInterval)                         // 차지 시간이 일정 간격을 넘는 경우
            {
                elapsed -= chargeInterval;                        // 타이머 초기화
                currentDamage = Mathf.Min(currentDamage + damageStep, maxDamage);   // 데미지 강화
                Debug.Log("차지단계 증가");
            }

            float denom = Mathf.Max(1, maxDamage - baseDamage); 
            float t = Mathf.Clamp01((currentDamage - baseDamage) / (float)denom);
            Progress?.Invoke(t);
            
            yield return null;                                    // 다음 프레임까지 대기
        }
        
        chargeVFX.Stop();                                         // 차징 VFX 종료
        attackVFX.Play();                                         // 범위 공격 VFX 실행

        FireConeDamage();                                         // 마우스를 땠을 때 격발
        isCharging = false;                                       // 차지 중 여부 false로 전환
        Ended?.Invoke();            // 발사 후 종료
    }


    /// <summary>
    /// 부채꼴 범위 내 적에게 currentDamage 적용
    ///  - 감지된 적의 방향벡터와 플레이어의 정면 벡터를 내적.
    ///  - 각도가 일정 이상인 적에게만 대미지를 적용하는 것.
    /// </summary>
    void FireConeDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            radius, 
            enemyLayer, 
            QueryTriggerInteraction.Ignore);    // 구체 범위 내에 적 감지
        Vector3 forward = transform.forward;                                                // 플레이어의 정면 벡터 추출
        if (flatcone) forward.y = 0;                                                        // y축 값 무시
        forward.Normalize();                                                                // 벡터 정규화

        Debug.Log("차지공격 발사");

        foreach (var hit in hits)                                                           // 충돌한 적 각각마다 데미지 적용
        {
            Vector3 dir = hit.transform.position - transform.position;                      // 플레이어 => 적 방향벡터 추출
            if (flatcone) dir.y = 0;                                                        // y축 무시
            dir.Normalize();                                                                // 벡터 정규화

            if (Vector3.Dot(forward, dir) >= cosThreshold)                                  // 정규화시킨 두 벡터 내적값이 특정 각도 이상일때만
            {
                if (hit.TryGetComponent(out UnitStats target))
                    target.TakeDamage(currentDamage);
                // 데미지 적용
            }
        }

        // 발사 파티클 추가 적용 필요
    }

    #endregion 

}
