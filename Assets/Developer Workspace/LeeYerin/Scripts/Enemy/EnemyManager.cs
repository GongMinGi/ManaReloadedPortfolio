using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 적을 관리하는 싱글톤 매니저 클래스
/// 적 오브젝트 풀 생성, 적 생성 위치 및 주기 관리, 적 생성 로직을 담당
/// </summary>
public class EnemyManager : MonoBehaviour
{
    #region Fields and Properties
    #region Singleton
    private static EnemyManager instance;
    public static EnemyManager Instance => instance;
    #endregion

    #region Enemy Object Pool Settings
    [Header("Enemy Object Pool Settings")]
    [SerializeField] MapEnemyData mapEnemyData;
    [SerializeField] int size;
    [SerializeField] int capacity;
    #endregion

    #region Enemy Spawn Settings
    [Header("Enemy Spawn Settings")]
    [SerializeField] PlayerController player;
    public PlayerController Player => player;

    #region Initialized from ScriptableObject
    [Tooltip("Spawn distance from player")]
    [SerializeField] float spawnDis; // 플레이어로부터의 생성 거리
    [Tooltip("Random generation range")]
    [SerializeField] float spawnRange; // 랜덤 생성 범위

    [Tooltip("Minimum time to create enemy objects (sec)")]
    [SerializeField] float minSpawnTime;    // 적 오브젝트 생성 최소 시간
    [Tooltip("Maximum time to spawn enemy objects (sec)")]
    [SerializeField] float maxSpawnTime;    // 적 오브젝트 생성 최대 시간
    #endregion

    #endregion
    #endregion

    #region Unity Event
    private void Start()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(instance);

        Initialization();

        // 적 생성 코루틴 시작
        StartCoroutine(SpawnEnemyLoop());
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 적 Pool 생성 및 스폰 설정값 초기화
    /// </summary>
    private void Initialization()
    {
        // MapEnemyData 내 적 프리팹 목록을 순회하며 각각 Pool 생성
        foreach (var enemy in mapEnemyData.Enemies)
            PoolManager.Instance.CreatePool(enemy, size, capacity);

        // MapEnemyData에 정의된 스폰 설정값을 필드에 복사
        mapEnemyData.Initialization(
            ref spawnDis, 
            ref spawnRange, 
            ref minSpawnTime, 
            ref maxSpawnTime);
    }
    #endregion

    #region Spawn Enemys
    /// <summary>
    /// 적을 특정 방향으로 스폰하는 메서드
    /// 플레이어 위치 기준으로 spawnDis 거리만큼 이동 방향으로 이동하고, 
    /// 랜덤 범위 내 위치 보정 적용
    /// </summary>
    /// <param name="enemyPrefab">생성할 적 프리팹</param>
    /// <param name="moveDir">생성 방향(단위 벡터)</param>
    public void SpawnEnemy(PooledObject enemyPrefab, Vector3 moveDir)
    {
        // 플레이어 위치 + 이동 방향 * 생성 거리
        Vector3 spawnPos = player.transform.position + moveDir * spawnDis;
        // 랜덤 범위 내 위치 오프셋 추가(x, z 축)
        spawnPos += new Vector3(Random.Range(-spawnRange, spawnRange), 0, Random.Range(-spawnRange, spawnRange));

        // Pool에서 적 오브젝트 위치 지정 및 활성화
        PooledObject newEnemy = PoolManager.Instance.GetPool(enemyPrefab, spawnPos, Quaternion.identity);

        // 생성 위치 방향을 바라보도록 회전 설정
        newEnemy.transform.LookAt(spawnPos);
    }

    /// <summary>
    /// 일정 시간 간격으로 적을 반복 생성하는 코루틴
    /// 생성 대기 시간은 최소/최대 생성 시간 사이에서 랜덤으로 결정
    /// </summary>
    /// <returns></returns>
    IEnumerator SpawnEnemyLoop()
    {
        while (true)  // TODO... 아후 게임 종료 여부 관리하는 변수 연결할 예정
        {
            // 최소 ~ 최대 스폰 시간 사이에서 랜덤 대기
            yield return new WaitForSeconds(
                Random.Range(minSpawnTime, maxSpawnTime));
            SpawnEnemy(mapEnemyData.Enemies[0], player.moveDir);    // TODO... 페이즈에 따른 적 오브젝트 변경은 이후 적용
        }
    }
    #endregion
}