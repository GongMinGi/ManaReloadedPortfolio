using System;
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
    #endregion

    [Header("Spawn Enemy Setting")]
    #region Spawn Enemy Setting
    [Tooltip("Spawn distance from player")]
    [SerializeField] float spawnDis = 5f; // 플레이어로부터의 생성 거리
    [Tooltip("Random generation range")]
    [SerializeField] float spawnRange = 2f; // 랜덤 생성 범위
    [SerializeField] List<PhaseEnemyData> phases = new();

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
    public List<PhaseEnemyData> Phases => phases;
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
}

/// <summary>
/// 각 페이즈 적 생성에 있어 필요한 정보를 저장하는 구조체
/// 
/// 페이즈에 나오는 적 종류 리스트(enemies)와 초 당 생성하는 적 수 리스트(enemiesPerSec)를 담고 있다
/// </summary>
[Serializable]
public struct PhaseEnemyData
{
    public List<PooledObject> enemies;
    public List<int> enemiesPerSec;
}