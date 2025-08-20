using Game.Combat.Stats;
using System.Collections;
using UnityEngine;

namespace Game.combat.EnemyAttack
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 적 돌진 공격(Rush Attack) 클래스
    /// 
    /// 기능:
    /// - BaseAttack 상속
    /// - 애니메이션 트리거와 콜라이더를 통해 돌진 공격 수행
    /// - 공격 대상(UnitStats)이 범위 내에 들어오면 피해 적용
    /// - 돌진하는 동안에는 적의 NavMeshAgent를 비활성함
    /// 
    /// 주의:
    /// - 현재 PerformAttack에서 Coroutine으로 공격 루프 실행
    /// - 스킬 종료 후 StopAttack 호출로 공격 루프 종료
    /// </summary>
    public class RushAttack : BaseAttack
    {
        [SerializeField] private Collider hitCollider;
        [SerializeField] private LayerMask attackLayer;


        [SerializeField] private Rigidbody rd;
        [SerializeField] private float speed;
        private bool isRush;

        [Header("Animation Triggers")]
        [SerializeField] private string prepareTrigger;      // 공격 준비 애니메이션
        [SerializeField] private string stunnedTrigger;      // 스턴 애니메이션 (사용 여부 선택)
        [SerializeField] private string skillFinishTrigger;  // 스킬 종료 애니메이션

        private Coroutine rushLoop;

        #region Unity Event
        protected override void OnDisable()
        {
            if (rushLoop != null)
            {
                StopCoroutine(rushLoop);
                rushLoop = null;

                enemy.Agent.enabled = true;
                hitCollider.enabled = false;
                isRush = false;
            }

            base.OnDisable();
        }

        private void FixedUpdate()
        {
            if (isRush)     // 돌진 중이라면
            {
                // 적이 바라보는 방향으로 돌진
                rd.MovePosition(rd.position + enemy.transform.forward * speed * Time.fixedDeltaTime);
            }
        }
        #endregion

        public override void StopAttack()
        {
            if (rushLoop != null)
            {
                StopCoroutine(rushLoop);
                rushLoop = null;

                hitCollider.enabled = false;
                isRush = false;
            }

            base.StopAttack();
        }

        /// <summary>
        /// 실제 공격 실행
        /// 실제 공격 코루틴(RushLoop) 실행
        /// </summary>
        /// <param name="target">공격 타켓의 UnitStats</param>
        protected override void PerformAttack(UnitStats target = null)
        {
            rushLoop = StartCoroutine(RushLogic());
        }

        /// <summary>
        /// 돌진 공격 로직을 담고 있는 코루틴
        /// - 준비 애니메이션 후 공격 애니메이션 실행
        /// - 콜라이더 활성화로 충돌 감지
        /// - 공격 종료 후 콜라이더 비활성화 및 StopAttack 호출
        /// </summary>
        private IEnumerator RushLogic()
        {
            // 준비 애니메이션 
            enemy.Animator.SetTrigger(prepareTrigger);
            yield return new WaitForSeconds(2f);

            // 공격 애니메이션 및 콜라이더 활성화
            enemy.Animator.SetTrigger(attackTrigger);
            enemy.Agent.enabled = false;    // NavMeshAgent 비활성화
            hitCollider.enabled = true;
            isRush = true;
            yield return new WaitForSeconds(4f);

            // 공격 종료
            hitCollider.enabled = false;
            isRush = false;
            enemy.Animator.SetTrigger(skillFinishTrigger);

            rushLoop = null;
            enemy.Agent.enabled = true;     // NavMeshAgent 활성화
            // 스킬 종료 후 공격 루프 종료
            StopAttack();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (attackLayer.Contain(other.gameObject.layer))
            {
                GameModeManager.Player.Stats.TakeDamage(attackDamage);
            }
        }
    }
}
