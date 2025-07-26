using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 보스 적의 공격 상태(스킬)를 정의하는 추상 클래스
/// BossAttackStateMachine의 각 상태는 이 클래스를 상속하여 구체적인 스킬 동작을 구현함
///
/// 각 상태는 BossEnemyController를 소유 객체로 받아 공격 실행에 필요한 정보를 참조할 수 있음
/// 
/// 공통적으로 사용되는 SkillAttackLoop 코루틴을 추상화하여, 개별 상태에서 구현하도록 강제함
/// </summary>
public abstract class BossAttackBaseState : BaseState<BossAttackType>
{
    protected BossEnemyController owner;

    public BossAttackBaseState(BossEnemyController owner)
    {
        this.owner = owner;
    }

    /// <summary>
    /// 스킬 공격 루틴을 정의하는 추상 메서드
    /// 각 스킬 상태는 이 메서드를 오버라이드하여 실제 코루틴 로직을 구현해야 함
    /// </summary>
    protected abstract IEnumerator SkillAttackLoop();
}