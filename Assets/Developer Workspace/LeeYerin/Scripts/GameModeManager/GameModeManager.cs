using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 현재 게임 모드에서 사용되는 주요 매니저들 및 플레이어 인스턴스를
/// 정적으로 중앙에서 관리하기 위한 퍼사드 클래스
/// 
/// 이 클래스를 통해 PoolManager, MapTileManager, EnemyManager, SkillCastingManager 등의
/// 기능적 매니저에 대한 접근을 단일 진입점으로 통합함
/// </summary>
public static class GameModeManager
{
    #region Player
    /// 현재 활성화된 플레이어 컨트롤러 인스턴스
    /// 플레이어를 알아야 하는 매니저들은 설정 시 자동으로 주입
    [SerializeField] static PlayerController player;
    public static PlayerController Player { get { return player; }set { player = value; } }
    #endregion

    #region UI Manager
    private static UIManager uiManager;
    public static UIManager UIManager { get { return uiManager; } set { uiManager = value; } }
    #endregion

    #region Game Scene's Manager
    private static GameLogicManager gameLogicManager;
    private static PoolManager poolManager;
    private static MapTileManager mapTileManager;
    private static EnemyManager enemyManager;
    private static SkillCastingManager skillCastingManager;

    public static GameLogicManager GameLogicManager { get { return gameLogicManager; } set { gameLogicManager = value; } }
    public static PoolManager PoolManager { get { return poolManager; } set { poolManager = value;} }
    public static MapTileManager MapTileManager { get { return mapTileManager; } set { mapTileManager = value; mapTileManager.Player = player; } }
    public static EnemyManager EnemyManager { get { return enemyManager; } set { enemyManager = value; enemyManager.Player = player; } }
    public static SkillCastingManager SkillCastingManager { get { return skillCastingManager; } set { skillCastingManager = value; } }

    /// <summary>
    /// 모든 필수 매니저가 준비된 상태인지 반환
    /// </summary>
    public static bool IsReady => MapTileManager.IsReady && EnemyManager.IsReady;
}