using DG.Tweening;
using Game.Combat.Stats;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
//using DG.Tweening;

/// <summary>
/// 개발자: 이예린
/// 
/// 적 캐릭터의 동작 및 네이게이션 이동 등을 관리하는 컨트롤러
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Stats Setting")]
    [SerializeField] UnitStats stats;   // 현재 스탯을 관리하는 컴포넌트

    [Header("Enemy Movement Setting")]
    [Tooltip("NavMeshAgent component used for enemy movement")]
    [SerializeField] NavMeshAgent agent;    // 적 이동에 사용하는 NavMeshAgent 컴포넌트
    private Transform player;   // 플레이어의 Transform

    [Header("Enemy Attack Setting")]
    [SerializeField] LayerMask PlayerLayer;
    [SerializeField] protected BoxCollider attackRange;
    private float attackDis;
    protected Coroutine attackLoop;
    [SerializeField] bool isAttack = false;
    [SerializeField] float attackDamage = 10;

    [Header("Damage Setting")]
    [SerializeField] SphereCollider hitSphere;

    [Header("Pool Object Setting")]
    [SerializeField] protected bool isBoss;
    [SerializeField] EnemyPooledObject enemyPooeledObj;

    [Header("Animation Setting")]
    [SerializeField] Animator animator;
    [SerializeField] float dieAnimDuration = 2f;
    [SerializeField] private bool isMove;
    [SerializeField] private bool isDie;

    #region Unity Event
    private void OnEnable()
    {
        agent.isStopped = false;
    }

    protected virtual IEnumerator Start()
    {
        if (enemyPooeledObj == null)
            if (!isBoss)
                Debug.LogError("PooledObject is null");

        yield return new WaitUntil(() => GameModeManager.EnemyManager.Player != null);
        player = GameModeManager.EnemyManager.Player.transform;

        attackDis = attackRange.size.z;
        agent.speed = stats.MoveSpeed;      // stats의 MoveSpeed 데이터 기반으로 agent의 speed 세팅
    }
    private void Update()
    {
        if (player == null) return;     // 플레이어의 Transform이 null이면 리턴

        if (isDie)  // 죽은 상태일 때
        {
            if (!agent.isStopped)   // agent가 작동 중이라면
            {
                agent.isStopped = true;     // 즉시 정지
                agent.ResetPath();          // 경로도 완전히 제거
            }
            
            return;
        }

        TryTracking();
    }
    #endregion

    #region Tracking Player
    /// <summary>
    /// 적의 플레이어 추적을 시도하는 메서드
    /// NavMesh 위에 있을 경우 플레이어 위치를 목적지로 설정하여 추적,
    /// NavMesh가 아닐 경우 비활성화 후 오브젝트 풀에 반환한다
    /// </summary>
    private void TryTracking()
    {
        if (agent.isOnNavMesh)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            // isAttack이 true인데 플레이어와 적의 거리가 공격 가능한 거리보다 멀 경우
            if (isAttack && distance > attackDis)
                isAttack = false;

            agent.SetDestination(player.position);

            // 이동 상태가 아니면 이동 상태로 전환
            if (!isMove)
                isMove = true;

            // 목적지에 도착했는지 확인
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // 플레이어를 향한 방향 벡터 계산 (수평 방향만)
                Vector3 dirToPlayer = (player.position - transform.position).normalized;
                dirToPlayer.y = 0f; // 수직 요소 제거하여 수평 회전만 수행

                // 방향 벡터가 유효한 경우에만 회전 수행
                if (dirToPlayer != Vector3.zero)
                {
                    // 목표 방향을 쿼터니언으로 변환
                    Quaternion lookRotation = Quaternion.LookRotation(dirToPlayer);

                    // 현재 회전에서 목표 회전으로 부드럽게 보간하여 회전
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
                }
            }
        }
        else
        {
            isMove = false;     // 이동 상태를 false로 전환
            enemyPooeledObj.Release();
        }
    }
    #endregion

    #region Damage & Death Handling
    public void OnDamaged()
    {
        hitSphere.enabled = false;

        animator.SetTrigger("IsDamaged");    // 피격 애니메이션 실행

        Sequence damagedSequence = DOTween.Sequence();

        damagedSequence.AppendInterval(1.0f);    // 플레이어 피격 애니메이션만큼 시간차를 둔 후

        damagedSequence.AppendCallback(() =>
        {
            hitSphere.enabled = true;
        });
    }

    [ContextMenu("OnDie")]
    /// <summary>
    /// 적이 사망했을 경우 호출되는 메서드
    /// </summary>
    public void OnDie()
    {
        isDie = true;

        if (attackLoop != null)
            StopCoroutine(attackLoop);

        isAttack = false;
        isMove = false;
        attackRange.enabled = true;

        animator.SetTrigger("IsDie");   // Die 애니메이션 실행

        Sequence seq = DOTween.Sequence();

        seq.AppendInterval(dieAnimDuration);

        //서서히 축소 → 풀로 반납
        seq.Append(transform.DOScale(Vector3.zero, 0.7f)
            .SetEase(Ease.InBack)) // 부드러운 축소 이펙트
            .OnComplete(() =>
            {
                isDie = false;
                enemyPooeledObj.IsDie = true;  // 죽어 Release됨을 알림
                enemyPooeledObj.Release();  // Pool에 반납
            });
    }
    #endregion

    #region Attack Handling
    /// <summary>
    /// 기본 근접 공격 루틴 코루틴(MeleeAttackLoop)을 실행하는 메서드
    /// </summary>
    protected virtual void StartAttack()
    {
        isMove = false;     // 이동 상태를 false로 전환
        attackLoop = StartCoroutine(MeleeAttackLoop());
    }

    /// <summary>
    /// 기본 근접 공격 루틴을 구현한 코루틴
    /// 
    /// isAttack가 true인 동안 1.5초 간격으로 공격 범위 판정용 콜라이더를 활성화함
    /// isAttack이 false가 되면 attackLoop를 null로 초기화하고,
    /// 콜라이더를 활성 상태로 유지하며 코루틴을 종료
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator MeleeAttackLoop()
    {
        isAttack = true;

        while (isAttack)
        {
            attackRange.enabled = true;

            yield return new WaitForSeconds(1f);
        }

        attackLoop = null;
        attackRange.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerLayer.Contain(other.gameObject.layer))    // 만약 충돌한 객체가 플레이어라면
        {
            if (isDie) return;

            if (!isAttack)  // isAttack이 현재 false일 때만 실행
                StartAttack();

            Debug.Log("단순 근접 공격 범위 내에 플레이어 들어옴");
            if (other.TryGetComponent(out IDamageable target))
            {
                animator.SetTrigger("IsAttack");    // 기본 근접 공격 애니메이션 실행
                target.TakeDamage(attackDamage);
            }

            attackRange.enabled = false;    // 공격 판정용 콜라이더 비활성화
        }
    }
    #endregion
}