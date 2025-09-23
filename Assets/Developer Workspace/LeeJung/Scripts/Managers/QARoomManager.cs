using Game.Combat.Stats;
using UnityEngine;

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
    [SerializeField] private EnemyPooledObject enemyPrefab;

    public bool IsNoCooldown { get { return isNoCooldown; } }
    public bool CanEnemyMove { get { return canEnemyMove; } }

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

    /// <summary>
    /// - 스킬 쿨타임 무제한 모드 토글
    /// </summary>
    public void ToggleNoCooldown()
    {
        isNoCooldown = !isNoCooldown;
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
    public void ToggleAllEnemyMovement()
    {
        GameModeManager.EnemyManager.ToggleAllEnemyMovement();
        canEnemyMove = !canEnemyMove;
    }

    /// <summary>
    /// - 적 소환
    /// </summary>
    public void SpawnEnemy()
    {
        GameModeManager.EnemyManager.SpawnEnemyForQA();
    }
}
