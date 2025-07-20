using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 보스 적 유닛의 전용 컨트롤러 클래스
/// 일반 EnemyController를 상속받아, 스킬 사용 및 상태 머신을 활용한 보스 전용 공격 로직을 처리
/// 
/// 현재 이후 스킬 구현을 위한 기초 틀만 구현된 상태
/// TODO... 보스 스킬 로직 및 기능 구현
/// </summary>
public class BossEnemyController : EnemyController
{
    // 보스 공격 상태 FSM
    BossAttackStateMachine fsm;
    BossAttackStateMachine FSM => fsm;

    [Header("Boss Skill Setting")]
    [SerializeField] float skillCoolTime;   // 스킬 사용 간 쿨타임
    [SerializeField] bool skillOn = false;  // 현재 스킬 공격이 가능한 상태인지 여부

    #region Unity Event
    protected override IEnumerator Start()
    {
        isBoss = true;

        // base.Start()를 수동으로 순회하여 완료까지 기다림
        IEnumerator baseStart = base.Start();
        
        while (baseStart.MoveNext())
            yield return baseStart.Current;

        // 보스 전용 상태 머신 초기화 및 스킬 쿨타임 루프 시작
        fsm = new(this);
        StartSkillCoolTimeLoop();
    }
    #endregion

    #region Attack Handling
    /// <summary>
    /// 공격 시작을 처리하는 메서드
    /// 스킬이 준비되지 않았을 경우 기본 근접 공격을 수행하고,
    /// 스킬이 준비된 상태라면 FSM을 통해 스킬 공격 상태로 전환함
    /// </summary>
    protected override void StartAttack()
    {
        if (!skillOn)
            // 기본 근접 공격 실행
            base.StartAttack();
        else
        {
            // 기본 근접 공격 루프 종료
            if (attackLoop != null)
                StopCoroutine(attackLoop);

            attackRange.enabled = false;

            // 무작위 스킬 공격 타입 선택
            BossAttackType attackType = (BossAttackType)Random.Range(0, 3);

            // 스킬 상태로 전환
            fsm.ChageState(attackType);
            skillOn = false;
        }
    }

    /// <summary>
    /// 스킬 쿨타임 루프 시작하는 메서드
    /// 쿨타임이 완료되면 skillOn을 true로 설정하여 스킬 공격이 가능하도록 함
    /// </summary>
    public void StartSkillCoolTimeLoop()
    {
        Debug.Log("스킬 루프 시작");
        StartCoroutine(SkillCoolTimeLoop());
    }

    /// <summary>
    /// 일정 시간 후 skillOn을 true로 전환하고, 스킬 공격을 트리거하는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator SkillCoolTimeLoop()
    {
        attackRange.enabled = true;

        yield return new WaitForSeconds(skillCoolTime);

        skillOn = true;
        StartAttack();
    }
    #endregion
}