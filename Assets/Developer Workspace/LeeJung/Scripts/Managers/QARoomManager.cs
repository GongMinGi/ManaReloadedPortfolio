using Game.Combat.Stats;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개발자: 이정
/// - QA룸에서 플레이어의 체력 회복, 데미지 입히기, 스킬 쿨타임 감소 등의 기능을 제공하는 매니저
/// </summary>
public class QARoomManager : MonoBehaviour
{
    private static QARoomManager instance;

    [SerializeField] private float testDamage = 100f;
    [SerializeField] private bool isNoCooldown = false;
    [SerializeField] private bool canEnemyMove = true;
    [SerializeField] private bool isFilterOn = false; 
    [SerializeField] private EnemyPooledObject enemyPrefab;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private Toggle noCooldownToggle;
    [SerializeField] private Toggle enemyMovementToggle;
    [SerializeField] private Toggle skillIconFilterToggle;

    public bool IsNoCooldown { get { return isNoCooldown; } }
    public bool CanEnemyMove { get { return canEnemyMove; } }
    public bool IsFilterOn { get { return isFilterOn; } }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.QARoomManager = instance;
        }

        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        noCooldownToggle.onValueChanged.AddListener(ToggleNoCooldown);
        enemyMovementToggle.onValueChanged.AddListener(ToggleAllEnemyMovement);
        skillIconFilterToggle.onValueChanged.AddListener(ToggleSkillIconFilter);
    }

    /// <summary>
    /// - 스킬 쿨타임 무제한 모드 토글
    /// </summary>
    public void ToggleNoCooldown(bool isNoCooldown)
    {
        Debug.Log($"쿨타임 무제한 모드: {isNoCooldown}");
        this.isNoCooldown = isNoCooldown;
    }

    /// <summary>
    /// - 플레이어의 체력을 최대치로 회복
    /// </summary>
    public void SetPlayerFullHP()
    {
        UnitStats playerStats = GameModeManager.Player.GetComponent<UnitStats>();

        if (playerStats != null)
        {
            playerStats.SetFullHP();
        }
    }

    /// <summary>
    /// - 플레이어에게 지정된 테스트 데미지를 입힘
    /// </summary>
    public void DealDamage()
    {
        UnitStats playerStats = GameModeManager.Player.GetComponent<UnitStats>();

        if (playerStats != null)
        {
            playerStats.TakeDamage(testDamage);
        }
    }

    /// <summary>
    /// - 모든 적 움직임 멈춤 토글
    /// </summary>
    public void ToggleAllEnemyMovement(bool canEnemyMove)
    {
        Debug.Log($"적 움직임 가능: {canEnemyMove}");
        this.canEnemyMove = canEnemyMove;
    }

    /// <summary>
    /// - 적 소환
    /// </summary>
    public void SpawnEnemy()
    {
        GameModeManager.EnemyManager.SpawnEnemyForQA(enemyPrefab,spawnDistance,canEnemyMove);
    }

    /// <summary>
    /// 스킬 Icon 필터링 토글
    /// </summary>
    /// <param name="isOn">스킬 필터 적용 여부</param>
    public void ToggleSkillIconFilter(bool isOn)
    {
        Debug.Log($"가능한 원소 스킬 필터 기능: {isOn}");
        isFilterOn = isOn;

        GameModeManager.SkillCastingManager.ApplySkillFilter(isOn);
    }
}
