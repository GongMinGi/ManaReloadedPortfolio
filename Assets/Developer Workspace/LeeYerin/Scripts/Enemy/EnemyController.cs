using DG.Tweening;
using Game.Combat.Stats;
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
    #region Stats
    [Header("Stats Setting")]
    [SerializeField] UnitStats stats;   // 적의 현재 스탯을 관리하는 컴포넌트
    public UnitStats Stats => stats;
    #endregion

    #region Movement
    [Header("Enemy Movement Setting")]
    [Tooltip("NavMeshAgent component used for enemy movement")]
    [SerializeField] NavMeshAgent agent;    // 적의 경로 탐색 및 이동을 제어하는 NavMeshAgent 컴포넌트
    private Transform player;                 // 플레이어 위치 추적용 Transform 참조
    [SerializeField] private bool isMove;     // 적의 이동 상태 여부 플래그
    #endregion

    #region Attack
    [Header("Enemy Attack Setting")]
    [SerializeField] LayerMask PlayerLayer; // 공격 대상(플레이어) 레이어 지정용 LayerMask
    [SerializeField] protected BoxCollider attackRange; // 공격 범위를 나타내는 박스 콜라이더
    private float attackDis;                  // 공격 범위 크기 (BoxCollider z 축 기준)
    protected Coroutine attackLoop;           // 공격 반복 동작을 위한 코루틴 참조
    [SerializeField] bool isAttack = false; // 현재 공격 상태 여부 플래그
    [SerializeField] float attackDamage = 10; // 공격 시 적에게 입히는 피해량
    #endregion

    #region Damage
    [Header("Damage Setting")]
    [SerializeField] SphereCollider hitSphere; // 적이 피해를 받는 판정을 위한 구체 콜라이더
    [SerializeField] DmgFloatPooledObject activeDmgText;  // 데미지 텍스트 오브젝트
    #endregion

    #region Pooling
    [Header("Pool Object Setting")]
    [SerializeField] protected bool isBoss;         // 보스 적 여부 판단용 플래그
    [SerializeField] EnemyPooledObject enemyPooledObj; // 적 오브젝트 풀에서 관리하는 오브젝트 참조
    #endregion

    #region Animation
    [Header("Animation Setting")]
    [SerializeField] Animator animator;             // 적 애니메이션 컨트롤러
    [SerializeField] float dieAnimDuration = 2f;   // 사망 애니메이션 재생 시간
    [SerializeField] private bool isDie;              // 적 사망 상태 여부 플래그
    #endregion

    #region Unity Event
    private void OnEnable()
    {
        // 현재 생성된 적 객체(this)를 EnemyManager의 활성 적 리스트에 등록
        GameModeManager.EnemyManager.Enemies.Add(this);     
        agent.isStopped = false;        // NavMeshAgent 동작 재개
    }

    protected virtual IEnumerator Start()
    {
        if (enemyPooledObj == null)
            if (!isBoss)
                Debug.LogError("PooledObject is null");

        // EnemyManager 내 플레이어가 초기화될 때까지 대기
        yield return new WaitUntil(() => GameModeManager.EnemyManager.Player != null);
        player = GameModeManager.EnemyManager.Player.transform;

        // 기본 공격 거리 초기화 (BoxCollider z 축 크기 기준)
        attackDis = attackRange.size.z;

        stats.OnDamaged += OnDamaged;

        // 스탯 변경 시 이동 속도 갱신 핸들러 등록
        stats.StatApplyHandlers.Add(StatType.MoveSpeed, () => agent.speed = stats.GetMoveSpeed());
        stats.StatRevertHandlers.Add(StatType.MoveSpeed, () => agent.speed = stats.GetMoveSpeed(false));

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
            enemyPooledObj.Release();
        }
    }
    #endregion

    #region Damage & Death Handling
    public void OnDamaged(float damage)
    {
        hitSphere.enabled = false;

        animator.SetTrigger("IsDamaged");    // 피격 애니메이션 실행

        // activeDmgText가 null이거나, 이미 활성화되어 있지 않은 경우
        if (activeDmgText == null || !activeDmgText.gameObject.activeInHierarchy)
        {
            // 새로운 피해량 텍스트 객체를 요청하고, 플레이어 또는 적의 위치에 맞춰 배치
            activeDmgText = GameModeManager.UIManager.RequestDamageText(transform);

            // 생성된 피해량 텍스트의 대상(Target)을 현재 객체로 설정
            activeDmgText.Target = transform;
        }

        // 피해량 텍스트에 실제 피해량 값을 설정
        activeDmgText.SetDamageText(damage);

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
        GameModeManager.EnemyManager.Enemies.Remove(this);      // 현재 생성된 적 객체(this)를 EnemyManager의 활성 적 리스트에서 삭제

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
                enemyPooledObj.IsDie = true;  // 죽어 Release됨을 알림
                enemyPooledObj.Release();  // Pool에 반납
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
            if (other.TryGetComponent(out UnitStats target))
            {
                animator.SetTrigger("IsAttack");    // 기본 근접 공격 애니메이션 실행
                target.TakeDamage(attackDamage);
            }

            attackRange.enabled = false;    // 공격 판정용 콜라이더 비활성화
        }
    }
    #endregion
}