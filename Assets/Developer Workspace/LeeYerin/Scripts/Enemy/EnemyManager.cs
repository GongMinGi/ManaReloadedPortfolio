using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 개발자: 이예린
/// 
/// 적을 관리하는 싱글톤 매니저 클래스
/// 적 오브젝트 풀 생성, 적 생성 위치 및 주기 관리, 적 생성 및 각 페이즈 로직을 담당
/// </summary>
public class EnemyManager : MonoBehaviour
{
    #region Fields and Properties
    #region Singleton
    private static EnemyManager instance;
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
    public PlayerController Player { get { return player; } set { player = value; } }
    private int phaseEnemyTypeCount;    // 현재 페이즈 내 등장하는 적 종류 개수

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

    #region Phase Progress Tracking
    private int clearedEnemyTypeCount = 0;  // 페이즈 내 처치가 완료된 적 종류 개수
    #endregion

    #region State Flags
    private bool isReady;

    public bool IsReady => isReady;
    #endregion
    #endregion

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameModeManager.Player != null);   // 플레이어를 알 수 있을 때까지 대기

        GameModeManager.GameLogicManager.Initialization(mapEnemyData.Phases.Count);     // 게임 전체 로직을 관리하는 매니저에 페이즈 수 전달

        if (instance == null)
        {
            instance = this;
            GameModeManager.EnemyManager = instance;
        }
        else
            Destroy(instance);

        yield return new WaitUntil(() => player != null);   // 플레이어가 할당된 후 진행

        isReady = Initialization();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// 적 Pool 생성 및 스폰 설정값 초기화
    /// </summary>
    private bool Initialization()
    {
        // MapEnemyData 내 적 프리팹 목록을 순회하며 각각 Pool 생성
        foreach (var enemy in mapEnemyData.Enemies)
            GameModeManager.PoolManager.CreateEnemyPool(enemy, size, capacity);

        // MapEnemyData에 정의된 스폰 설정값을 필드에 복사
        mapEnemyData.Initialization(
            ref spawnDis, 
            ref spawnRange, 
            ref minSpawnTime, 
            ref maxSpawnTime);

        return true;
    }
    #endregion

    #region Spawn Enemys
    /// <summary>
    /// 각 페이즈 적 생성 로직을 구현된 코루틴 실행하는 메서드
    /// </summary>
    /// <param name="phase">실행할 페이즈 단계</param>
    public void StartSpawnEnemyLoop(int phase)
    {
        // 적 생성 코루틴 시작
        StartCoroutine(SpawnEnemyLoop(phase));
    }

    /// <summary>
    /// 일정 시간 간격으로 적을 반복 생성하는 코루틴
    /// 생성 대기 시간은 최소/최대 생성 시간 사이에서 랜덤으로 결정
    /// </summary>
    /// <param name="phase">현재 페이즈 넘버</param>
    /// <returns></returns>
    IEnumerator SpawnEnemyLoop(int phase)
    {
        // 현재 진행 중인 페이즈 번호(phase)를 기준으로
        // 해당 페이즈의 적 프리팹 및 등장 수 정보를 보관한 데이터(PhaseEnemyData)를 추출
        List<EnemyPooledObject> enemyTypes = new List<EnemyPooledObject>(mapEnemyData.Phases[phase].enemies);
        phaseEnemyTypeCount = enemyTypes.Count;

        for (int i = 0; i < phaseEnemyTypeCount; i++)
        {
            // i번째 적 프리팹에 해당하는 EnemyPool을 찾아,
            // 그에 연결된 EnemySpawnTracker에 해당 적의 등장 수(phaseData.enemiesNum[i])를 설정
            GameModeManager.PoolManager.FindEnemyPoolDic((enemyTypes[i]))
                .Tracker.Set(mapEnemyData.Phases[phase].enemiesNum[i]);
        }


        while (enemyTypes.Count != 0 && !GameModeManager.GameLogicManager.IsGameOver)  // TODO... 아후 게임 종료 여부 관리하는 변수 연결할 예정
        {
            // 최소 ~ 최대 스폰 시간 사이에서 랜덤 대기
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));

            for (int i = 0; i < enemyTypes.Count; i++)
            {
                if (!GameModeManager.PoolManager.FindEnemyPoolDic(enemyTypes[i]).Tracker.CanSpawn)
                {
                    enemyTypes.Remove(enemyTypes[i]);
                    continue;
                }

                SpawnEnemy(
                    enemyTypes[i], 
                    player.MoveDir
                    );
            }
        }

        Debug.Log($"{phase}번째 페이즈의 적 스폰 로직 종료");
    }

    /// <summary>
    /// 지정한 수만큼 적을 특정 방향으로 스폰하는 메서드
    /// 플레이어 위치 기준으로 spawnDis 거리만큼 이동 방향으로 이동하고, 
    /// 랜덤 범위 내 위치 보정 적용
    /// </summary>
    /// <param name="enemyPrefab">생성할 적 프리팹</param>
    /// <param name="moveDir">생성 방향(단위 벡터)</param>
    /// <param name="enemyNum">생성할 적 수</param>
    private void SpawnEnemy(EnemyPooledObject enemyPrefab, Vector3 moveDir, int enemyNum = 1)
    {
        // 플레이어 시야에서 적 생성 방지
        if (moveDir == Vector3.zero)
            moveDir = Vector3.forward;

        // 플레이어 위치 + 이동 방향 * 생성 거리
        Vector3 spawnPos = player.transform.position + moveDir * spawnDis;

        for (int i = 0; i < enemyNum; i++)
        {
            // 랜덤 범위 내 위치 오프셋 추가(x, z 축)
            spawnPos += new Vector3(Random.Range(-spawnRange, spawnRange), 0, Random.Range(-spawnRange, spawnRange));

            // 지정된 위치(spawnPos) 근처에서 유효한 NavMesh 위치를 탐색 (최대 반경 4m)
            // 유효한 위치를 찾으면 해당 위치(hit.position)로 보정하여 에이전트 생성 오류 방지
            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPos, out hit, 4f, NavMesh.AllAreas))
            {
                spawnPos = hit.position;
            }

            // Pool에서 적 오브젝트 위치 지정 및 활성화
            EnemyPooledObject newEnemy = GameModeManager.PoolManager.GetEnemyPool(enemyPrefab, spawnPos, Quaternion.identity);

            // 생성 위치 방향을 바라보도록 회전 설정
            newEnemy.transform.LookAt(spawnPos);
        }
    }
    #endregion

    #region Try Advance Phase
    /// <summary>
    /// 페이즈 전환 조건을 평가하고 트리거하는 메서드
    /// 
    /// 현재 페이즈에서 처치 완료된 적 종류 수를 하나 증가시킴
    /// 모든 종류가 처치된 경우 다음 페이즈로 진행을 요청함
    /// </summary>
    public void TryAdvancePhase()
    {
        // 현재 페이즈에서 처치 완료된 적 종류 수를 하나 증가
        clearedEnemyTypeCount++;

        // 모든 적 종류가 처치된 경우
        if (clearedEnemyTypeCount >= phaseEnemyTypeCount)
        {
            // 카운터 초기화
            clearedEnemyTypeCount = 0;
            // 다음 페이즈 진행 요청
            GameModeManager.GameLogicManager.ProceedPhase();
        }
    }
    #endregion
}