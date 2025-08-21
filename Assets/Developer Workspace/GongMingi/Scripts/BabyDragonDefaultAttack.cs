using DG.Tweening;
using Game.combat.EnemyAttack;
using Game.Combat.Stats;
using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


namespace Game.combat.EnemyAttack
{

    /// <summary>
    /// *작성자 : 공민기
    /// Baby Dragon의 기본 원거리 공격 컨트롤러.
    /// - 공격 트리거(애니메이션)만 걸고, 실제 발사는 애니메이션 이벤트에서 수행.
    /// - 공격 중에는 플레이어를 '부드럽게' 따라보도록 회전( yaw-only )을 유지.
    /// - 발사 순간엔 forward로 쏘는 모드가 가장 시각적으로 자연스러움.
    /// </summary>
    public class BabyDragonDefaultAttack : BaseAttack
    {
        [Header("Projectile")]
        [SerializeField] private Transform proejctileMuzzle;
        [SerializeField] private FireballProjectile_BabyDragon projectilePrefab;
        private FireballProjectile_BabyDragon projectileInstance;

        [Header("Projectile Params")]
        [SerializeField] private float projectileSpeed;                         // 투사체 속도
        [SerializeField] private float projectileMaxRange;                      // 투사체 최대 사거리
        [SerializeField] private float projectileRadius;                        // (스피어캐스트용) 충돌 판정 반경
        [SerializeField] private LayerMask enemyLayer;                          // 타겟(플레이어) 레이어 마스크
        [SerializeField] private LayerMask obstacleLayer;                       // 장애물 레이어 마스크

        [Header("Sound Setting")]
        [SerializeField] int atkSfxId;

        [Header("Aiming")]
        [SerializeField] private float followYawDegPerSec = 540f;               // 1초에 회전하는 최대 각도
        [SerializeField] private bool fireUsingForward = true;                  // 발사 순간에 적의 정면으로 바라봄
        [SerializeField] private float stopFollowDelayAfterFire = 0.05f;        // 마지막 발사 직후 살짝 더 따라보다 끊김


        private bool aimFollowActive;                                           // 조준 (타깃 추적 회전) 활성화 여부
        private Coroutine stopAimCo;                                            // 조준 해제 지연 코루틴 핸들
        private UnitStats cachedTarget;                                         // 현재 타깃 캐시 (없으면 player 사용)
        static private bool poolIsCreated = false;                              // 풀 생성 여부 (동일 프리팹을 여러 적이 써도 1회만 생성)


        /// <summary>
        /// 투사체 풀 초기화.
        /// - 초기/최대 수량은 게임 상황에 맞게 튜닝.
        /// - 한 번만 생성하도록 static 플래그를 둠.
        /// </summary>
        private void Init()
        {
            GameModeManager.PoolManager.CreatePool(projectilePrefab, 20, 30);   // 풀매니저에 투사체 풀 생성 (초기5개, 최대 10개) 
            poolIsCreated = true;
        }


        /// <summary>
        /// BaseAttack의 공격 틱마다 호출되는 "실행부".
        /// - 여기서는 '애니메이션 트리거'만 건다 (실제 발사는 AE에서).
        /// - 동시에 '조준 추적'을 켠다 → LateUpdate에서 계속 플레이어를 따라보게 됨.
        /// </summary>
        protected override void PerformAttack(UnitStats target = null)
        {
            if (!poolIsCreated) Init();  // 첫 호출 시 풀 초기화

            // 타깃 캐시 (매 프레임 trasnform을 찾는 비용 절감) 
            cachedTarget = target != null ? target : GameModeManager.Player?.Stats;

            aimFollowActive = true;     // 플레이어 방향으로 회전 온

            // 애니메이션 트리거 -> 애니메이션 이벤트로 AE_Fire / AE_ComboEnd 등을 심어둔다
            if (enemy && enemy.Animator && !string.IsNullOrEmpty(attackTrigger))
                enemy.Animator.SetTrigger(attackTrigger);
        }

        /// <summary>
        /// 조준 추적 로직.
        /// - 공격 동안(aimFollowActive=true) 매 프레임 타깃을 향해 '제한된 각속도'로 회전.
        /// - Yaw-only(수평) 회전: 위/아래로 까딱거리지 않게 dir.y=0 처리.
        /// - RotateTowards: 초당 허용 각도(followYawDegPerSec)만큼만 회전 → 급격한 스냅 방지.
        /// </summary>
        private void LateUpdate()
        {
            if (!aimFollowActive) return;

            Vector3 dir = cachedTarget.transform.position - enemy.transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.0001f) return;      // 너무 가까우면 회전 불필요

            Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            float maxStep = followYawDegPerSec * Time.deltaTime;                            //프레임 별 최대 회전량          
            enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, targetRot, maxStep);
        }


        /// <summary>
        /// (애니메이션 이벤트) 콤보 종료.
        /// - 마지막 발사 직후 '조금 더' 따라보다 끊으면 시각적으로 안정감이 좋음.
        /// - stopFollowDelayAfterFire가 0이면 즉시 끊기.
        /// </summary>
        public void AE_ComboEnd()
        {
            // 발사를 모두 끝낸 후 좀더 플레이어방향을 따라가다 끊기
            if (stopAimCo != null) StopCoroutine(stopAimCo);
            stopAimCo = StartCoroutine(StopAimFollowAfter(stopFollowDelayAfterFire));
        }

        /// <summary>
        /// 조준 추적을 지정 시간 뒤에 끊는 코루틴.
        /// </summary>
        private IEnumerator StopAimFollowAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            aimFollowActive = false;                        // 조준 off ( 필요 시 여기서 NavMeshAgent 회전 권한 복귀) 
        }


        /// <summary>
        /// (애니메이션 이벤트) 발사 지점.
        /// - fireUsingForward=true이면 '현재 forward'로 발사 → 애니메이션 포즈/총구 방향과 100% 일치.
        /// - false면 그 순간의 muzzle→타깃 방향을 다시ㄴ 계산해 발사(움직이는 타깃을 보다 직접적으로 조준).
        /// </summary>
        public void BabyDragonFire()
        {
            if (projectilePrefab == null || proejctileMuzzle == null) return;

            enemy.PlaySFX(atkSfxId);   // 공격 사운드 출력

            Transform target = cachedTarget != null ? cachedTarget.transform : GameModeManager.Player.transform;

            // 기본값: forward( 애니메이션 일치)
            Vector3 forward = enemy.transform.forward;
            Vector3 dir = forward;

            // 필요 시 그 순간의 '정확한' 타깃 방향으로 발사
            if( !fireUsingForward )
            {
                dir = (target.position - proejctileMuzzle.position);
                dir.y = 0f;
                if (dir.sqrMagnitude < 0.0001f) dir = forward;  // 너무 가까우면 그냥 정면으로 발사
                dir.Normalize();
            }

            // 탄두 회전 ( 정면이 dir을 향하도록)
            Quaternion projectileRot = Quaternion.LookRotation(dir, Vector3.up);

            // 풀에서 인스턴스 꺼내기 ( 위치 / 회전 세팅 포함)
            PooledObject go = GameModeManager.PoolManager.GetPool(
                projectilePrefab, proejctileMuzzle.position, projectileRot );
            projectileInstance = go as FireballProjectile_BabyDragon;


            // 투사체 파라미터 구성( Setup에서 이동/충돌 등 초기화)
            var projectileParam = new FireBallParams
            {
                speed = projectileSpeed,
                maxRange = projectileMaxRange,
                radius = projectileRadius,
                damage = attackDamage,
                enemyL = enemyLayer,
                obstacleL = obstacleLayer,
                dir = dir,
            };

            projectileInstance.Setup(projectileParam);
        }




    }
}

