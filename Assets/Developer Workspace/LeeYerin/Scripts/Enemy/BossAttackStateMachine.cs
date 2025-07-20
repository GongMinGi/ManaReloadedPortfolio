using Game.Enemy.Boss.State;
using System.Diagnostics;

/// <summary>
/// 개발자: 이예린
/// 
/// 보스 적의 공격(스킬) 상태를 관리하는 상태 머신
/// 각 스킬 타입(돌진, 원거리, 광역 근접)에 해당하는 상태 클래스를 초기화하고 전환함
/// </summary>
public class BossAttackStateMachine : BaseStateMachine<BossAttackType>
{
    /// <summary>
    /// 이 상태 머신을 사용하는 보스 적 컨트롤러 참조
    /// </summary>
    protected BossEnemyController owner;

    /// <summary>
    /// 보스 공격 상태 머신 생성자
    /// 보스 컨트롤러를 전달받아 상태 초기화에 사용
    /// </summary>
    /// <param name="owner"></param>
    public BossAttackStateMachine(BossEnemyController owner)
    {
        this.owner = owner;
        InitializeStates();
    }

    /// <summary>
    /// 보스의 각 공격 타입에 대응하는 상태를 등록
    /// ChargeState, RangedState, AreaMeleeState를 초기화하여 딕셔너리에 매핑
    /// </summary>
    private void InitializeStates()
    {
        stateDict.Add(BossAttackType.Charge, new ChargeState(owner));
        stateDict.Add(BossAttackType.Ranged, new RangedState(owner));
        stateDict.Add(BossAttackType.AreaMelee, new AreaMeleeState(owner));
    }
}

/// <summary>
/// 개발자: 이예린
/// 
/// 보스 적의 공격 스킬 유형을 정의한 열거형
/// 보스 적의 상태 머신에서 상태를 구분하는 데 사용됨
/// </summary>
public enum BossAttackType
{
    Charge,     // 돌진
    Ranged,     // 원거리
    AreaMelee   // 광역 근접
}