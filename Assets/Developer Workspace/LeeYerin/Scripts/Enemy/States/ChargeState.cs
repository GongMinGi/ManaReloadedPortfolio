using System.Collections;
using UnityEngine;

namespace Game.Enemy.Boss.State
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 보스 적의 돌진 공격 상태를 정의한 클래스
    /// 상태 머신에서 Charge 상태일 때 실행되는 로직을 처리함
    /// </summary>
    public class ChargeState : BossAttackBaseState
    {
        public ChargeState(BossEnemyController owner) : base(owner) { }

        public override void OnStateEnter()
        {
            Debug.Log("돌진 공격 상태 진입");
            owner.StartCoroutine(SkillAttackLoop());
        }

        public override void OnStateExit()
        {
            Debug.Log("돌진 공격 상태 퇴장");
            owner.StartSkillCoolTimeLoop();
        }

        public override void OnStateUpdate() { }

        protected override IEnumerator SkillAttackLoop()
        {
            yield return new WaitForSeconds(1f);

            OnStateExit();
        }
    }
}
