using DG.Tweening;
using Game.combat.EnemyAttack;
using Game.Combat.Stats;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 개발자: 이예린
/// 
/// 적 캐릭터의 동작 및 네이게이션 이동 등을 관리하는 컨트롤러
/// 
/// 주의:
/// - 이 클래스는 일반 적(기본 적)을 위한 기본 동작을 제공함
/// - 엘리트나 보스와 같은 상위 적은 이 클래스를 상속받아 스킬이나 추가 행동을 구현해야 함
/// </summary>
public class EnemyController : MonoBehaviour
{
    #region Stats
    [Header("Stats Setting")]
    [SerializeField] UnitStats stats;   // 적의 현재 스탯을 관리하는 컴포넌트
    public UnitStats Stats => stats;
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

    public Animator Animator => animator;
    #endregion

    #region Sound
    [Header("Sound Setting")]
    [SerializeField] AudioSource source;
    [SerializeField] int deathSfxId = 110009;
    /// <summary>
    /// 지정된 사운드 ID에 해당하는 효과음을 재생하는 메서드
    /// </summary>
    /// <param name="id">재생할 사운드의 고유 ID</param>
    public void PlaySFX(int id) => GameModeManager.SoundManager.PlaySFX(id, source);
    #endregion

    #region Movement
    [Header("Enemy Movement Setting")]
    [Tooltip("NavMeshAgent component used for enemy movement")]
    [SerializeField] NavMeshAgent agent;    // 적의 경로 탐색 및 이동을 제어하는 NavMeshAgent 컴포넌트
    private Transform player;                 // 플레이어 위치 추적용 Transform 참조
    [SerializeField] private bool isMove;     // 적의 이동 상태 여부 플래그

    public NavMeshAgent Agent => agent;
    #endregion

    #region Damage
    [Header("Damage Setting")]
    [SerializeField] Collider hitSphere; // 적이 피해를 받는 판정을 위한 구체 콜라이더
    [SerializeField] DmgFloatPooledObject activeDmgText;  // 데미지 텍스트 오브젝트
    [SerializeField] EnemyHealthBarPooledObj enemyHealthBar;    // 적 체력바 오브젝트
    [SerializeField] Transform enemyHealthBarPos;               // 적 체력바 트랜스폼
    #endregion

    #region Attack
    [Header("Enemy Attack Setting")]
    [SerializeField] private BaseAttack defaultAttack;
    private bool isAttacking;
    #endregion

    #region Unity Event
    protected virtual void OnEnable()
    {
        RegisterWithManager();  // 적을 EnemyManager의 활성 적 리스트에 등록
        SetupHealthBar();       // 적 체력바(UI)를 요청하고 초기 위치/스탯 설정
        EnableAgent();          // NavMeshAgent를 활성화하고 이동을 재개
    }

    protected virtual IEnumerator Start()
    {
        // 풀 오브젝트 유효성 검사, 유효하지 않으면 초기화 중단
        if (!ValidatePoolObject()) yield break;

        // EnemyManager 내 플레이어가 초기화될 때까지 대기
        yield return new WaitUntil(() => GameModeManager.EnemyManager.Player != null);
        CachePlayer();  // 플레이어 Transform 캐싱

        SubscribeToEvents();    // 피해 및 사망 이벤트 구독
        SetupStatHandlers();    // 스탯 변경 핸들러 등록 및 이동 속도 초기화

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

        PreAttackUpdate();  // 스킬이나 추가 행동을 여기서 처리

        if (!isAttacking && !isDie)     // update를 도는 도중 죽엇을때를 대비해 !isDie 조건 추가
        {
            // 기본 공격
            if (defaultAttack.IsPlayerInRange(player))
                StartAttack(defaultAttack);
            else
                TryTracking();
        }
    }

    protected virtual void PreAttackUpdate() { }
    #endregion

    #region Enemy Initialization
    /// <summary>
    /// EnemyManager의 활성 적 리스트에 현재 적 객체를 등록하는 메서드
    /// </summary>
    private void RegisterWithManager() => GameModeManager.EnemyManager.Enemies.Add(this);

    /// <summary>
    /// UI 매니저를 통해 적의 체력바를 요청하고 초기 위치 및 스탯을 설정하는 메서드
    /// </summary>
    private void SetupHealthBar()
    {
        enemyHealthBar = GameModeManager.UIManager.RequestEnemyHealthBar(transform);
        enemyHealthBar.Setup(Stats, enemyHealthBarPos);
    }

    /// <summary>
    /// NavMeshAgent를 활성화하고 이동을 재개할 수 있도록 초기화하는 메서드
    /// </summary>
    private void EnableAgent()
    {
        if (!agent.enabled) agent.enabled = true;
        agent.isStopped = false;
    }
    #endregion

    #region Enemy Start Setup
    /// <summary>
    /// EnemyPooledObject가 유효한지 확인하는 메서드
    /// enemyPooledObj가 null이고 보스가 아닌 경우 오류를 출력하고 초기화를 중단함
    /// </summary>
    private bool ValidatePoolObject()
    {
        if (enemyPooledObj == null && !isBoss)
        {
            Debug.LogError("PooledObject is null");
            return false;
        }
        return true;
    }

    /// <summary>
    /// EnemyManager에서 플레이어 Transform을 가져와 캐싱하는 메서드
    /// </summary>
    private void CachePlayer() => player = GameModeManager.EnemyManager.Player.transform;

    /// <summary>
    /// 스탯 이벤트를 구독하여 피해 및 사망 시 처리 메서드를 연결하는 메서드
    /// </summary>
    private void SubscribeToEvents()
    {
        stats.OnDamaged += OnDamaged;
        stats.OnDie += OnDie;
    }

    /// <summary>
    /// 스탯 변경에 따른 이동 속도 적용 핸들러를 등록하고 초기 이동 속도를 설정하는 메서드
    /// </summary>
    private void SetupStatHandlers()
    {
        stats.StatApplyHandlers.Add(StatType.MoveSpeed, () => agent.speed = stats.GetMoveSpeed());
        stats.StatRevertHandlers.Add(StatType.MoveSpeed, () => agent.speed = stats.GetMoveSpeed(false));

        agent.speed = stats.MoveSpeed;
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
        if (!agent.isOnNavMesh)
        {
            isMove = false;
            enemyPooledObj.Release();
            return;
        }

        // 공격 중이라면 이동하지 않음
        if (isAttacking)
        {
            agent.isStopped = true;
            isMove = false;

            // 회전은 공격할 때도 플레이어를 향하게 유지
            FacePlayer();
            return;
        }

        // 플레이어를 추적
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // 이동 플래그 갱신
        if (!isMove) isMove = true;

        // 도착 시 회전 보정
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            FacePlayer();
    }

    /// <summary>
    /// 플레이어를 바라보도록 회전 처리하는 메서드
    /// Y축 회전만 적용하여 수직 회전 없이 수평 방향만 회전
    /// </summary>
    private void FacePlayer()
    {
        if (Agent.enabled == false)
            Agent.enabled = true;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        dirToPlayer.y = 0f; // 수직 방향 제거

        if (dirToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    /// <summary>
    /// 적의 이동을 정지시키는 메서드
    /// NavMeshAgent가 활성화되어 있을 경우 즉시 경로를 제거하고 이동 플래그를 갱신
    /// 공격 상태(isAttacking)를 true로 설정하여 이동 중 공격 제한
    /// </summary>
    public void StopMovement()
    {
        if (agent != null && agent.enabled)
        {
            // NavMesh에 올라와 있을 때만 실제 정지 처리
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        isMove = false;
        isAttacking = true;
    }

    /// <summary>
    /// 적의 이동을 재개하는 메서드
    /// NavMeshAgent가 활성화되어 있고, 적이 사망 상태가 아닐 경우 이동 허용
    /// 이동 플래그(isMove)를 true로 갱신하고 공격 상태(isAttacking)를 false로 설정
    /// </summary>
    public void ResumeMovement()
    {
        if (agent == null || isDie) return;

        if (!agent.enabled) agent.enabled = true;

        agent.isStopped = false;
        isMove = true;
        isAttacking = false;
    }
    #endregion

    #region Damage & Death Handling
    public void OnDamaged(float damage)
    {
        hitSphere.enabled = false;
        animator.SetTrigger("IsDamaged");    // 피격 애니메이션 실행

        DmgTextLogic(damage);

        Sequence damagedSequence = DOTween.Sequence();

        damagedSequence.AppendInterval(1.0f);    // 플레이어 피격 애니메이션만큼 시간차를 둔 후

        damagedSequence.AppendCallback(() =>
        {
            hitSphere.enabled = true;
        });
    }

    /// <summary>
    /// 적이 사망했을 경우 호출되는 메서드
    /// </summary>
    public virtual void OnDie(float damage)
    {
        if (Agent.enabled == false)
            Agent.enabled = true;

        isDie = true;
        defaultAttack.StopAttack();          // 사망 시 공격 코루틴 전부 중지
        DmgTextLogic(damage);
        enemyHealthBar.Release();            // 사망 시 hpbar 풀에 반납

        GameModeManager.EnemyManager.Enemies.Remove(this);      // 현재 생성된 적 객체(this)를 EnemyManager의 활성 적 리스트에서 삭제

        if (isAttacking)    // 기본 공격 중이었다면
            defaultAttack.StopAttack();

        isMove = false;
        isAttacking = false;

        animator.SetTrigger("IsDie");   // Die 애니메이션 실행
        PlaySFX(deathSfxId);   // Die 사운드 출력

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

    /// <summary>
    /// 현재 객체에 대한 피해량 텍스트를 생성 또는 활성화하고, 
    /// 전달된 피해량 값을 화면에 표시하는 메서드
    /// </summary>
    /// <param name="damage">표시할 피해량 값</param>
    private void DmgTextLogic(float damage)
    {
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
    }
    #endregion

    #region Attack Handling
    /// <summary>
    /// 적의 공격 시작
    /// </summary>
    protected void StartAttack(BaseAttack attack)
    {
        isAttacking = true;
        attack?.StartAttack(); // 연결된 공격 실행
    }
    #endregion
}