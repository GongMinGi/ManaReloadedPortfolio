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

    // 오브젝트 풀을 관리하는 딕셔너리
    private Dictionary<int, ObjectPool> poolDic = new Dictionary<int, ObjectPool>();

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
    /// <param name="poolObj"></param>
    /// <param name="size"></param>
    /// <param name="capacity"></param>
    public void CreatePool(PooledObject poolObj, int size, int capacity)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = $"Pool_{poolObj.name}";

        // ObjectPool 컴포넌트 추가 및 초기화
        ObjectPool objectPool = gameObject.AddComponent<ObjectPool>();
        objectPool.CreatePool(poolObj, size, capacity);

        poolDic.Add(poolObj.GetInstanceID(), objectPool);
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
    /// <param name="objectPool"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public PooledObject GetPool(PooledObject objectPool, Vector3 position, Quaternion rotation)
    {
        // 프리팹 인스턴스 ID를 기반으로 ObjectPool 검색 후 오브젝트 활성화 및 반환
        return poolDic[objectPool.GetInstanceID()].GetPool(position, rotation);
    }
    #endregion
}