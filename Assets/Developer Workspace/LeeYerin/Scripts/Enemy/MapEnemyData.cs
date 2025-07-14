using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 맵에서 등장하는 적에 대한 정보를 저장하는 ScriptableObject
/// 적 등장 확률 및 스폰 위치, 생성 시간 범위 등 데이터를 관리
/// </summary>
[CreateAssetMenu(fileName = "MapEnemyData", menuName = "Scriptable Objects/MapEnemyData")]
public class MapEnemyData : ScriptableObject
{
    #region Serialized Fields
    [Header("Map Enemy Odd Setting")]
    #region Map Enemy Odd Setting
    [Tooltip("List of enemy prefabs that appear on the map")]
    [SerializeField] List<PooledObject> enemies = new();    // 맵에 등장하는 적 프리팹 목록
    [Tooltip("List of the probability (ratio) of each enemy's appearance")]
    [SerializeField] List<float> enemyOdds = new(); // 각 적의 등장 확률(비율) 리스트
    #endregion

    [Header("Spawn Enemy Setting")]
    #region Spawn Enemy Setting
    [Tooltip("Spawn distance from player")]
    [SerializeField] float spawnDis = 5f; // 플레이어로부터의 생성 거리
    [Tooltip("Random generation range")]
    [SerializeField] float spawnRange = 2f; // 랜덤 생성 범위

    [Tooltip("Minimum time to create enemy objects (sec)")]
    [SerializeField] float minSpawnTime;    // 적 오브젝트 생성 최소 시간
    [Tooltip("Maximum time to spawn enemy objects (sec)")]
    [SerializeField] float maxSpawnTime;    // 적 오브젝트 생성 최대 시간
    #endregion
    #endregion

    #region Runtime Data
    // 적별 등장 확률을 빠르게 조회하기 위한 딕셔너리
    private Dictionary<PooledObject, float> enemyOddsMap;
    #endregion

    #region Public Properties
    /// <summary>
    /// 맵에 등장하는 적 프리팹 목록
    /// </summary>
    public List<PooledObject> Enemies => enemies;

    /// <summary>
    /// 적별 등장 확률을 빠르게 조회하기 위한 딕셔너리
    /// </summary>
    public Dictionary<PooledObject, float> EnemyOddsMap
    {
        get
        {
            if (enemyOdds == null)
                SetEnemyOdds();

            return enemyOddsMap;
        }
    }
    #endregion

    #region Variable Initialization
    /// <summary>
    /// 외부 변수에 스폰 관련 값을 복사하는 초기화 메서드
    /// </summary>
    /// <param name="spawnDis">복사할 스폰 거리</param>
    /// <param name="spawnRange">복사할 랜덤 생성 범위</param>
    /// <param name="minSpawnTime">복사할 최소 생성 시간</param>
    /// <param name="maxSpawnTime">복사할 최대 생성 시간</param>
    public void Initialization(
        ref float spawnDis, 
        ref float spawnRange, 
        ref float minSpawnTime, 
        ref float maxSpawnTime)
    {
        spawnDis = this.spawnDis;
        spawnRange = this.spawnRange;
        minSpawnTime = this.minSpawnTime;
        maxSpawnTime = this.maxSpawnTime;
    }
    #endregion

    #region Enemy Odds Initialization
    /// <summary>
    /// 적 목록과 확률 리스트를 매핑하여 딕셔너리를 초기화하는 메서드
    /// 적 등장 확률을 빠르게 조회하기 위해 사용됨
    /// </summary>
    private void SetEnemyOdds()
    {
        enemyOddsMap = new();

        for (int i = 0; i < enemies.Count; i++)
            enemyOddsMap.Add(enemies[i], enemyOdds[i]);
    }
    #endregion
}