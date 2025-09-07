using Game.combat.EnemyAttack;
using Game.Combat.Stats;
using UnityEngine;

namespace Game.Combat.EnemyAttack
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 적의 근거리 공격 구현 클래스
    /// BaseAttack을 상속하여 Melee용 공격 처리
    /// </summary>
    public class MeleeAttack : BaseAttack
    {
        [SerializeField] private Collider hitCollider;
        [SerializeField] private LayerMask attackLayer;

        [Header("Sound Setting")]
        [SerializeField] int atkSfxId;

        /// <summary>
        /// 실제 공격 실행
        /// 애니메이션 트리거 실행 후 콜라이더 활성화
        /// </summary>
        /// <param name="target">공격 타켓의 UnitStats</param>
        protected override void PerformAttack(UnitStats target = null)
        {
            enemy.Animator.SetTrigger(attackTrigger);
            //enemy.PlaySFX(atkSfxId);   // 공격 사운드 출력
            hitCollider.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (attackLayer.Contain(other.gameObject.layer))
            {
                GameModeManager.Player.Stats.TakeDamage(attackDamage);
                hitCollider.enabled = false;
            }
        }
    }
}