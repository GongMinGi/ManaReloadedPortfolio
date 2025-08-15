using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 오브젝트 풀을 관리하는 매니저 클래스
/// </summary>
public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;

    [Tooltip("Reference to the world space canvas where the UI pooledObject will be displayed")]
    [SerializeField] Transform worldSpaceCanvas;

    // 오브젝트 풀을 관리하는 딕셔너리
    private Dictionary<int, ObjectPool> poolDic = new Dictionary<int, ObjectPool>();
    // 적 오브젝트 풀을 관리하는 딕셔너리
    private Dictionary<int, EnemyPool> enemyPoolDic = new Dictionary<int, EnemyPool>();

    public EnemyPool FindEnemyPoolDic(EnemyPooledObject enemy) => enemyPoolDic[enemy.GetInstanceID()];

    #region Unity Event
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            GameModeManager.PoolManager = instance;
        }
        else
            Destroy(gameObject);
    }
    #endregion

    #region Object Pool
    /// <summary>
    /// 지정된 PooledObject를 기반으로 새로운 ObjectPool을 생성하고 등록
    /// </summary>
    /// <param name="poolObj">풀링할 대상이 되는 PooledObject 프리팹</param>
    /// <param name="size">초기 생성할 인스턴스 수</param>
    /// <param name="capacity">최대 보관 가능한 인스턴스 수</param>
    /// <param name="isUI">생성하는 오브젝트의 UI 여부</param>
    public void CreatePool(PooledObject poolObj, int size, int capacity, bool isUI = false)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = $"Pool_{poolObj.name}";

        if (isUI)
            gameObject.transform.SetParent(worldSpaceCanvas, worldPositionStays: false);

        // ObjectPool 컴포넌트 추가 및 초기화
        ObjectPool objectPool = gameObject.AddComponent<ObjectPool>();
        objectPool.CreatePool(poolObj, size, capacity);

        poolDic.Add(poolObj.GetInstanceID(), objectPool);
    }

    /// <summary>
    /// 지정된 EnemyPooledObject를 기반으로 새로운 EnemyPool을 생성하고 등록
    /// </summary>
    /// <param name="poolObj">풀링할 대상이 되는 EnemyPooledObject 프리팹</param>
    /// <param name="size">초기 생성할 인스턴스 수</param>
    /// <param name="capacity">최대 보관 가능한 인스턴스 수</param>
    public void CreateEnemyPool(EnemyPooledObject poolObj, int size, int capacity)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = $"Pool_{poolObj.name}";

        // EnemyPool 컴포넌트 추가 및 초기화
        EnemyPool objectPool = gameObject.AddComponent<EnemyPool>();
        EnemySpawnTracker tracker = new EnemySpawnTracker();
        objectPool.Initialize(tracker); // EnemyPool의 초기화 메서드를 통해 EnemySpawnTracker 할당
        objectPool.CreatePool(poolObj, size, capacity);

        enemyPoolDic.Add(poolObj.GetInstanceID(), objectPool);
    }

    /// <summary>
    /// 지정된 Pool Object에 대응하는 ObjectPool을 제거하고 딕셔너리에서 제거
    /// </summary>
    /// <param name="poolObj">제거할 대상 오브젝트 프리팹</param>
    public void DestroyPool(PooledObject poolObj)
    {
        // 프리팹 인스턴스 ID를 기반으로 ObjectPool 검색
        ObjectPool objectPool = poolDic[poolObj.GetInstanceID()];
        Destroy(objectPool.gameObject);

        poolDic.Remove(poolObj.GetInstanceID());
    }

    /// <summary>
    /// 현재 존재하는 모든 ObjectPool을 제거하고 초기화
    /// </summary>
    public void ClearPool()
    {
        foreach (ObjectPool objectPool in poolDic.Values)
        {
            Destroy(objectPool.gameObject);
        }

        poolDic.Clear();
    }

    /// <summary>
    /// 지정된 오브젝트 프리팹에 대응하는 Pool에서 오브젝트를 반환받아 지정 위치와 회전으로 활성화
    /// </summary>
    /// <param name="objectPool">반환받고자 하는 오브젝트의 프리팹 (기준 프리팹)</param>
    /// <param name="position">오브젝트를 배치할 월드 위치</param>
    /// <param name="rotation">오브젝트를 배치할 회전 값</param>
    /// <returns></returns>
    public PooledObject GetPool(PooledObject objectPool, Vector3 position, Quaternion rotation)
    {
        // 프리팹 인스턴스 ID를 기반으로 ObjectPool 검색 후 오브젝트 활성화 및 반환
        return poolDic[objectPool.GetInstanceID()].GetPool(position, rotation);
    }

    /// <summary>
    /// 지정된 오브젝트 프리팹에 대응하는 적 Pool에서 오브젝트를 반환받아 지정 위치와 회전으로 활성화
    /// </summary>
    /// <param name="enemyPool">반환받고자 하는 적 오브젝트의 프리팹 (기준 프리팹)</param>
    /// <param name="position">오브젝트를 배치할 월드 위치</param>
    /// <param name="rotation">오브젝트를 배치할 회전 값</param>
    /// <returns></returns>
    public EnemyPooledObject GetEnemyPool (EnemyPooledObject enemyPool, Vector3 position, Quaternion rotation)
    {
        // 프리팹 인스턴스 ID를 기반으로 EnemyPool 검색 후 오브젝트 활성화 및 반환
        return enemyPoolDic[enemyPool.GetInstanceID()].GetPool(position, rotation) as EnemyPooledObject;
    }
    #endregion
}