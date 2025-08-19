using Game.Combat.EnemyAttack;
using Game.Combat.EnemyAttack.SO;
using Game.Combat.Stats;
using System.Collections;
using UnityEngine;

namespace Game.combat.EnemyAttack
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 적 캐릭터의 기본 공격 로직을 관리하는 클래스
    /// 
    /// - 공격 범위, 데미지, 공격 간격 등을 설정할 수 있음
    /// - 실제 공격 수행은 상속받은 클래스에서 PerformAttack()을 오버라이드하여 구현
    /// - EnemyController와 연동되어 공격 시작 시 이동을 정지, 종료 시 이동 재개
    /// </summary>
    public class BaseAttack : MonoBehaviour
    {
        [Header("Attack Range Settings")]
        [SerializeField] protected AttackRangeData rangeData;   // 공격 범위 데이터
        protected IAttackRange attackRange = null;               // 실제 범위 판정 인터페이스

        [Header("Attack Properties")]
        [SerializeField] protected float attackInterval = 1.5f; // 공격 주기
        [SerializeField] protected int attackDamage = 10;       // 공격 데미지

        protected Coroutine attackLoop;   // 반복 공격 루프
        protected bool isAttacking;      // 공격 중 상태 플래그

        [Header("References")]
        [SerializeField] protected EnemyController enemy;       // 공격 주체인 적
        [SerializeField] protected string attackTrigger;       // 애니메이션 트리거 이름

        #region Unity Event
        private void OnEnable()
        {
            attackLoop = null;
            isAttacking = false;
        }

        private void Awake()
        {
            // 범위 인스턴스 생성
            attackRange = rangeData.CreateInstance();
        }
        #endregion

        /// <summary>
        /// 플레이어가 공격 범위 안에 있는지 확인하는 메서드
        /// </summary>
        /// <param name="target">플레이어 Transform</param>
        /// <returns>범위 안이면 true, 아니면 false</returns>
        public bool IsPlayerInRange(Transform target)
        {
            if (attackRange == null) return false;

            return attackRange.IsPlayerInRange(enemy.transform, target);
        }

        /// <summary>
        /// 공격 시작 로직을 담당하는 메서드
        /// </summary>
        /// <param name="target">공격 대상(UnitStats), null이면 플레이어로 지정</param>
        public void StartAttack(UnitStats target = null)
        {
            if (attackLoop != null) return; // 중복 실행 방지

            if (attackRange == null) return;

            if (target == null)
                target = GameModeManager.Player.Stats;

            isAttacking = true;
            enemy.StopMovement();   // 공격 시작 시 이동 정지
            attackLoop = enemy.StartCoroutine(AttackLoop(target));
        }

        /// <summary>
        /// 공격 종료 로직을 담당하는 메서드
        /// </summary>
        public void StopAttack()
        {
            isAttacking = false;
            if (attackLoop != null)
            {
                enemy.StopCoroutine(attackLoop);
                attackLoop = null;
            }
            enemy.ResumeMovement();     // 공격 종료 시 이동 재개
        }

        /// <summary>
        /// 공격 중 반복적으로 호출되는 코루틴
        /// 플레이어가 공격 범위를 벗어나면 종료되고,
        /// 공격 간격마다 PerformAttack을 실행됨
        /// </summary>
        /// <param name="target">공격 대상</param>
        private IEnumerator AttackLoop(UnitStats target = null)
        {
            while (isAttacking)
            {
                if (attackRange != null && !attackRange.IsPlayerInRange(enemy.transform, target.transform))
                {
                    StopAttack();   // 범위를 벗어나면 공격 중지
                    yield break;
                }

                PerformAttack(target);
                yield return new WaitForSeconds(attackInterval);
            }

            attackLoop = null;
        }

        /// <summary>
        /// 실제 공격 수행 로직 (상속받은 클래스에서 구현)
        /// </summary>
        /// <param name="target">공격 대상</param>
        protected virtual void PerformAttack(UnitStats target = null) { }
    }
}
