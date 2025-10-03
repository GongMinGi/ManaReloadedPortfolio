using System;
using System.Collections;
using UnityEngine;

public class BaseCombinationMagic : MonoBehaviour
{
    [Header("Skill Setting")]
    [SerializeField] int skill_ID;
    [SerializeField] protected float coolTime = 10f;
    protected Coroutine skillCoolTime;
    [NonSerialized]protected bool canUseSkill = true;

    public struct Context
    {
        public Transform caster;        // 시전자(플레이어) 트랜스폼
        public MonoBehaviour coroutineRunner;   // 코루틴 실행 주체 ( 대개 playercontroller 자신)
    }

    public virtual void ExecuteSkill()
    {
        if (GameModeManager.QARoomManager != null && GameModeManager.QARoomManager.IsNoCooldown == true)
        {
            Debug.Log("쿨타임 무제한 모드로 스킬 사용");
            return;
        }

        canUseSkill = false;
        // 스킬 쿨타임 실행
        skillCoolTime = GameModeManager.Player.StartCoroutine(SkillCoolTimer());
    }

    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 지정된 스킬의 쿨타임 UI를 관리하는 코루틴
    /// 스킬 아이콘을 활성화하고 쿨타임 텍스트를 1초 단위로 감소시키며,
    /// 쿨타임 종료 시 스킬 사용 가능 상태를 true로 변경함
    /// </summary>
    /// <returns></returns>
    protected IEnumerator SkillCoolTimer()
    {
        SkillUIInfo info = GameModeManager.SkillCastingManager.GetSkillIcon(skill_ID);

        info.CoolTimeUI.gameObject.SetActive(true);
        float time = coolTime;
        info.CoolTimeText.text = $"{time}";

        while (time > 0f)
        {
            yield return new WaitForSeconds(1f);
            info.CoolTimeText.text = $"{--time}";
        }

        canUseSkill = true;
        info.CoolTimeUI.gameObject.SetActive(false);
    }
}
