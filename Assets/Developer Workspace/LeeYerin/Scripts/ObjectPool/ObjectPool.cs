using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 지정된 PooledObject를 미리 생성해 보관하고, 필요 시 재사용하거나 새로 생성하는 풀링 시스템이 구현된 클래스
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Header("Object Pool Settings")]
    [SerializeField] PooledObject poolObj;  // 풀링할 오브젝트 프리팹
    [Tooltip("초기 생성할 오브젝트 개수")]
    [SerializeField] int size;
    [Tooltip("보관 가능한 오브젝트의 최대 수")]
    [SerializeField] int capacity;

    private Stack<PooledObject> objectPool;

    #region Object Pool
    /// <summary>
    /// ObjectPool을 초기화하고 지정된 개수만큼 오브젝트 인스턴스를 미리 생성하여 비활성 상태로 보관하는 메서드
    /// </summary>
    /// <param name="poolObj">풀링 대상이 되는 오브젝트 프리팹</param>
    /// <param name="size">초기 생성할 오브젝트 개수</param>
    /// <param name="capacity">풀에서 보관 가능한 최대 오브젝트 개수</param>
    public void CreatePool(PooledObject poolObj, int size, int capacity)
    {
        this.poolObj = poolObj;
        this.size = size;
        this.capacity = capacity;

        objectPool = new Stack<PooledObject>(capacity);

        // size만큼 오브젝트 생성
        for (int i = 0; i < size; i++)
        {
            PooledObject instance = Instantiate(poolObj);
            instance.gameObject.SetActive(false);
            instance.Pool = this;
            instance.transform.SetParent(transform);    // 오브젝트를 Pool의 자식으로 생성
            objectPool.Push(instance);
        }
    }

    /// <summary>
    /// Pool에서 재사용 가능한 오브젝트를 반환받아 지정 위치와 회전으로 활성화하는 메서드
    /// 만약 Pool이 비어있다면 새로운 인스턴스를 생성한다.
    /// </summary>
    /// <param name="position">오브젝트를 배치할 월드 위치</param>
    /// <param name="rotation">오브젝트의 회전값</param>
    /// <returns>활성화된 PooledObject 인스턴스</returns>
    public PooledObject GetPool(Vector3 position, Quaternion rotation)
    {
        if (objectPool == null) return null;

        if (objectPool.Count > 0)
        {
            PooledObject instance = objectPool.Pop();
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.gameObject.SetActive(true);
            return instance;
        }
        else
        {
            PooledObject instance = Instantiate(poolObj);
            instance.Pool = this;
            instance.transform.SetParent(transform);    // 오브젝트를 Pool의 자식으로 생성
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.gameObject.SetActive(true);
            return instance;
        }
    }

    /// <summary>
    /// 사용이 끝난 ObjectPool로 반환하는 메서드
    /// Pool의 용량을 초과하면 오브젝트를 파괴하여 메모리를 해제한다.
    /// </summary>
    /// <param name="instance"></param>
    public void ReturnPool(PooledObject instance)
    {
        if (objectPool == null) return;

        if (objectPool.Count < capacity)
        {
            instance.gameObject.SetActive(false);

            if (!instance.IsUI)
                instance.transform.parent = transform;

            objectPool.Push(instance);
        }
        else
        {
            Destroy(instance.gameObject);
        }
    }
    #endregion
}