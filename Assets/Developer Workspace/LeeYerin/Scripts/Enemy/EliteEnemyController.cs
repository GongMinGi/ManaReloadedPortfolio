using Game.combat.EnemyAttack;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 엘리트 적(Elite Enemy) 컨트롤러 클래스
/// 
/// 기능:
/// - EnemyController 상속, 일반 공격 및 상태 관리 포함
/// - 특정 스킬(BaseAttack)을 쿨타임에 따라 반복 사용
/// - 플레이어가 스킬 범위 내에 있으면 공격 실행
/// - 사망 시 스킬 코루틴 정리 및 스킬 사용 불가 처리
/// </summary>
public class EliteEnemyController : EnemyController
{
    [Header("Skill Settings")]
    [SerializeField] BaseAttack skillAttack;    // 사용 가능한 스킬

    private bool canUseSkill;           // 스킬 사용 가능 상태
    private Coroutine skillCoolTime;    // 스킬 쿨타임 코루틴

    #region Unity Event
    protected override void OnEnable()
    {
        base.OnEnable();
        // 엘리트 적 활성화 시 스킬 쿨타임 코루틴 시작
        skillCoolTime = StartCoroutine(SkillCoolTimer());
    }
    #endregion

    /// <summary>
    /// 적 사망 처리
    /// - 스킬 코루틴 종료
    /// - 스킬 사용 불가 상태로 변경
    /// </summary>
    /// <param name="damage">받은 피해량</param>
    public override void OnDie(float damage)
    {
        if (skillCoolTime != null)
        {
            StopCoroutine(skillCoolTime);
            skillCoolTime = null;
        }

        canUseSkill = false;
        base.OnDie(damage);
    }

    /// <summary>
    /// 공격 전 업데이트 로직
    /// - 플레이어가 스킬 범위 내에 있고 스킬 사용 가능하면 실행
    /// </summary>
    protected override void PreAttackUpdate()
    {
        if (canUseSkill && skillAttack.IsPlayerInRange(GameModeManager.Player.transform))
        {
            canUseSkill = false;
            StartAttack(skillAttack);       // 스킬 실행
            skillCoolTime = StartCoroutine(SkillCoolTimer());   // 쿨타임 시작
        }
    }

    /// <summary>
    /// 스킬 쿨타임 코루틴
    /// - 쿨타임이 지나면 스킬 사용 가능 상태로 변경
    /// </summary>
    private IEnumerator SkillCoolTimer()
    {
        yield return new WaitForSeconds(skillAttack.CoolTime);

        canUseSkill = true;
        skillCoolTime = null;
    }
}