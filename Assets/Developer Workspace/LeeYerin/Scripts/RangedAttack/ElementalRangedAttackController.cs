using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어가 보유한 원소 속성을 바탕으로,
/// 우선순위에 따라 적절한 원거리 공격을 자동 선택하고 실행하는 컨트롤러
/// 
/// 원소의 우선순위는 ScriptableObject(ElementPriorityData)에 정의되며,
/// 사용자는 사전에 정의된 공격 유형(Charge, Beam, Cone) 중 하나를 실행하게 됨
/// </summary>
public class ElementalRangedAttackController : MonoBehaviour
{
    [SerializeField] ElementPriorityData elementPriorityData;
    [SerializeField] Animator playerAnim;
    public Animator PlayerAnim 
    {
        get => playerAnim;
        set => playerAnim = value; 
    }

    private Dictionary<E_CastingType, int> castedElementCount = new();

    /// <summary>
    /// enum으로 정의된 casting type을  리스트에 플레그 형태로 저장
    /// </summary>
    public Dictionary<E_CastingType, int> CastedElementCount => castedElementCount;

    [Header("RangedAttack Components")]
    [SerializeField] RangedChargeConeAttack chargeConeAttack;
    [SerializeField] RangedChargeProjectileAttack chargeProjectileAttack;
    [SerializeField] RangedBeamAttack beamAttack;
    [SerializeField] RangedConeAttack coneAttack;

    private IAnimationDriver _anim;                                         // Animator를 감싼 추상 드라이버
    private RangedAttackContext _ctx;                                       // 공격 공용 컨텍스트..(Ow
    private readonly List<AttackBinding> attackBindings = new();            // 이벤트 구독 핸들 보관

    private void Awake()
    {
        playerAnim = GetComponent<Animator>();

        //Debug.Log($"playerAnim (_anim 할당전) : {playerAnim}");            // awake보다 먼저 실행돼는 버그..,????

        //if(playerAnim == null)
        //    Debug.Log($"playerAnim 연산 1회진행 후 : {playerAnim}");

        //playerAnim.SetTrigger("RangedConeAttack");
        //Debug.Log($"playerAnim 트리거 연산 진행후 : {playerAnim}");


        _anim = new AnimatorDriver(playerAnim);                             // Animator를 드라이버로 감싸기
        Debug.Log($"playerAnim (_anim 할당후) : {playerAnim}");

        if (_anim != null) Debug.Log("할당됨");
        else Debug.Log("할당 안됌");

        _ctx = new RangedAttackContext(transform, transform, _anim);        // 공격 공용 컨텍스트 ( Owner/ DefaultMuzzle/ Anim) 구성

        // 공격 바인딩 ( 시그널 -> Animator 파라미터 매핑) 
        //  - 지속형은 boolHash로, 차지 / 채널링이면 useProgress = true 로 진행도 연결
        attackBindings.Add(WireAttack(beamAttack, boolHash: AnimParams.Beam, useProgress: false));
        attackBindings.Add(WireAttack(chargeProjectileAttack, boolHash: AnimParams.ChargeProjectile, useProgress: false));
        attackBindings.Add(WireAttack(chargeConeAttack, boolHash: AnimParams.ChargeCone, useProgress: false));
        attackBindings.Add(WireAttack(coneAttack, boolHash: AnimParams.HoldConeAttack, useProgress: false));

        //Animator ani;
        //ani.GetBehaviour
    }

    private void OnDestroy()
    {
        // 생성한 모든 바인딩 Dispose => 이벤트 구독 안전 해제
        foreach (var binding in attackBindings) binding.Dispose();
        attackBindings.Clear();
    }

    /// <summary>
    /// * AttackBinding 클래스에서 이벤트에 연결한 액션을 구체적으로 구현하는 메서드
    ///  - 각 액션을 animator의 파라미터메서드와 결합시킨다.
    ///  - 어떤 스크립트인지 몰라도 Monobehavior라면 받아서 인터페이스 구현 여부만 보고 바인딩을 한다
    /// </summary>
    /// <param name="rangedAttack"></param>
    /// <param name="boolHash"></param>
    /// <param name="useProgress"></param>
    /// <param name="triggerHash"></param>
    /// <returns></returns>
    private AttackBinding WireAttack(
        MonoBehaviour rangedAttack,                                                     // 모든 공격 클래스가 Monobehavior상속 => 특정 클래스 이름을 알 필요없이 어떤 공격이든 전달가능
        int? boolHash = null,                                                           // 지속형 공격이면 Animator Bool 해시
        bool useProgress = false,                                                       // 차지, 채널링 진행도를 사용할 지 여부
        int? triggerHash = null)                                                        // 즉발형 공격일 때 trigger파리미터 사용
    {
        if (rangedAttack == null) return AttackBinding.Empty;                           // 인스펙터에 원거리공격 컴포넌트가 할당되지 않았다면 더미 바인딩 반환

        if (rangedAttack is IRequireAttackContext needCtx)                              // 공격이 컨텍스트를 요구하면(인터페이스를 구현했으면) 주입
            needCtx.BindContext(_ctx);

        if ( rangedAttack is not IAttackSignals signal)                                 // 시그널 없는 공격(ex: 즉발)이면 바인딩 불필요
            return AttackBinding.Empty;

        AttackBinding binding = new AttackBinding(signal);                              // 실제 구독/해제를 관리할 AttackBinding 인스턴스 생성

        // 지속형(bool 파라미터) 매핑
        if ( boolHash.HasValue)
        {
            int parameterHashValue = boolHash.Value;                                    // 해시 캐싱
            binding.Started     = () => _anim.SetBool(parameterHashValue, true);        // 공격 시작 => bool ON
            binding.Ended       = () => _anim.SetBool(parameterHashValue, false);       // 정상 종료 => Bool OFF
            binding.Interrupted = () => _anim.SetBool(parameterHashValue, false);       // 강제 취소 => Bool OFF

            if (useProgress)                                                            // 차지, 채널링 진행도 매핑
                binding.Progress = attackProgressedRate => _anim.SetFloat(parameterHashValue, attackProgressedRate);
        }

        binding.Subscribe();                                                            // AttackBinding에서 실제 event와 실행할 Action을 연결
        return binding;                                                                 // 컨트롤러에서 Dispose 할 수 있도록 반환
    }


    private sealed class AttackBinding : IDisposable                                    // IDisposable을 구현한 이벤트 - 바인딩 한 덩어리 를 나타내는 내부 전용(sealed) 클래스
    {
        public static readonly AttackBinding Empty = new(null);                         // 시그널이 아예 없는 경우에 쓰는 싱글턴 더미 객체 

        private readonly IAttackSignals rangedAttackSignal;                             // 실제로 구독할 공격 시그널(started, end 등 )을 보관하는 읽기 전용 참조
        public Action Started;                                                          // 공격이 시작될 때 호출될 델리게이트(Action)
        public Action Ended;                                                            // 공격이 정상 종료될 때 호출
        public Action Interrupted;                                                      // 외부 요인(Stop 등)으로 끊겼을때 호출
        public Action<float> Progress;                                                  // 차지, 채널링 진행도(0~1)를 전달 ( 필요시 사용) 

        public AttackBinding(IAttackSignals signal) => rangedAttackSignal = signal;     // 생성자. 컨트롤러가 전달한 signal을 받아온다.

        public void Subscribe()                                                         // signal(이벤트)과 그 이벤트에서 호출할 함수(Acition)을 묶는다.
        {
            if (rangedAttackSignal == null) return;                                     // signal(이벤트)이 없으면 아무것도 하지 않음
            if (Started != null) rangedAttackSignal.Started         += Started;         // 시작 이벤트 연결
            if (Ended != null) rangedAttackSignal.Ended             += Ended;           // 종료 이벤트 연결
            if (Interrupted != null) rangedAttackSignal.Interrupted += Interrupted;     // 취소 이벤트 연결    
            if (Progress != null) rangedAttackSignal.Progress       += Progress;        // 진행도 이벤트 연결
        }

        public void Dispose()                                                           // IDisposeable을 이용하여 안전하게 구독 해제 
        {
            if (rangedAttackSignal == null) return;                                     // 이미 해제된 경우 바로 종료 
            if (Started != null) rangedAttackSignal.Started         -= Started;         // 시작 이벤트 해제
            if (Ended != null) rangedAttackSignal.Ended             -= Ended;           // 종료 이벤트 해제
            if (Interrupted != null) rangedAttackSignal.Interrupted -= Interrupted;     // 취소 이벤트 해제
            if (Progress != null) rangedAttackSignal.Progress       -= Progress;        // 진행도 이벤트 해제
            Started = Ended = Interrupted = null;                                       // 델리게이트 참조를 제거해 GC 대상화
            Progress = null;
        }
    }


    #region Casted Element Count Clear
    /// <summary>
    /// 보유한 원소 속성별 카운트를 초기화하는 메서드
    /// 모든 원소의 개수를 0으로 설정하여 원소 보유 정보를 리셋함
    /// </summary>
    public void ClearCastedElementCount()
    {
        foreach (var element in castedElementCount.Keys.ToList())
            castedElementCount[element] = 0;
    }
    #endregion

    #region Elemental Attack Dispatcher
    /// <summary>
    /// 현재 보유한 원소를 기반으로 적절한 원거리 공격을 자동 선택하여 실행하는 메서드
    /// 우선순위와 보유 개수를 고려하여 실행되는 공격이 결정됨
    /// </summary>
    public void TryElementalRangedAttack()
    {
        if (castedElementCount == null)
            return;

        // 우선순위와 보유 개수를 기반으로 하나의 속성을 선택
        E_CastingType? castingType = SelectPrimaryCastingElement();

        if (!castingType.HasValue)
        {
            Debug.LogWarning("사용 가능한 속성이 없습니다.");
        }
        else
        {
            Debug.Log($"[ {castingType} ] 속성이 선택됐습니다.");

            // 선택된 속성에 해당하는 공격 방식 호출
            switch (castingType.Value)
            {
                case E_CastingType.Earth:
                case E_CastingType.Thunder:
                    ChargeAttack(castingType.Value);
                    break;

                case E_CastingType.Light:
                case E_CastingType.Darkness:
                    BeamAttack(castingType.Value);
                    break;

                case E_CastingType.Fire:
                case E_CastingType.Cold:
                    ConeAttack(castingType.Value);
                    break;
            }
        }

        // 보유 중인 원소 개수에 대한 데이터 정리
        ClearCastedElementCount();
    }

    /// <summary>
    /// 현재 보유한 원소들 중 가장 높은 우선순위의 원소 속성을 선택하는 메서드
    /// 단, 동일 우선순위 내에서 여러 원소가 있을 경우 개수가 많은 쪽이 우선되어 선택됨
    /// </summary>
    /// <returns>선택된 원소 속성 (없다면 null 반환)</returns>
    E_CastingType? SelectPrimaryCastingElement()
    {
        foreach (var group in elementPriorityData.PriorityGroups)
        {
            E_CastingType? castingType = null;

            foreach (var element in group.elements)
            {
                if (!castedElementCount.ContainsKey(element))
                    continue;

                // 해당 원소를 1개 이상 보유 중인지 확인
                if (castedElementCount[element] == 0)
                    continue;

                // 동일 우선순위 그룹 내에서는 개수가 가장 많은 속성을 우선 선택
                if (castingType == null || castedElementCount[castingType.Value] < castedElementCount[element])
                    castingType = element;
            }

            // 현재 우선순위 그룹에서 보유 중인 원소가 있다면,
            // 그 원소 중 가장 많이 보유한 것을 선택해 즉시 반환
            if (castingType != null)
                return castingType;
        }

        // 사용 가능한 원소가 전혀 없는 경우
        return null;
    }
    #endregion

    #region Ranged Attack Handlers
    /// <summary>
    /// Hold & Release 방식의 차지 공격을 수행하는 메서드
    /// 땅: 폭발성 투사체 / 전기: 전방 원뿔 범위 즉시 피해
    /// </summary>
    /// <param name="castingType">실행할 속성</param>
    private void ChargeAttack(E_CastingType castingType)
    {
        switch (castingType)
        {
            case E_CastingType.Earth:
                chargeProjectileAttack.ExecuteAttack(castingType);
                break;
            case E_CastingType.Thunder:
                chargeConeAttack.ExecuteAttack(castingType);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Hold 방식으로 지속되는 빔 공격을 수행하는 메서드
    /// 빛/어둠: 전방 직선 방향으로 지속 피해
    /// </summary>
    /// <param name="castingType">실행할 속성</param>
    private void BeamAttack(E_CastingType castingType)
    {
        switch (castingType)
        {
            case E_CastingType.Light:
                beamAttack.ExecuteAttack(castingType);
                break;
            case E_CastingType.Darkness:
                beamAttack.ExecuteAttack(castingType);
                break;
            default:
                Debug.LogWarning("BeamAttack 할 수 없는 원소 속성입니다.");
                break;
        }
    }

    /// <summary>
    /// Hold 방식의 원뿔 모양 범위 공격을 수행합니다.
    /// 불/냉기: 지속적 범위 피해
    /// </summary>
    /// <param name="castingType">실행할 속성</param>
    private void ConeAttack(E_CastingType castingType)
    {
        switch (castingType)
        {
            case E_CastingType.Fire:
                coneAttack.ExecuteAttack(castingType);
                break;
            case E_CastingType.Cold:
                coneAttack.ExecuteAttack(castingType);
                break;
            default:
                Debug.LogWarning("ConeAttack 할 수 없는 원소 속성입니다.");
                break;
        }
    }

    #endregion
}