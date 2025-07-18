using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 오브젝트 풀링 시스템에서 개별 오브젝트를 제어하는 클래스.
/// Pool에서 활성화될 때 자동 반환 여부(autoRelease)에 따라 일정 시간 후 자동으로 풀에 반환되며,
/// Pool 정보가 없는 경우에는 스스로 파괴됨.
/// </summary>
public class PooledObject : MonoBehaviour
{
    [SerializeField] bool autoRelease;  // 활성화 시 ReleaseRoutine을 자동 실행할지 여부
    [SerializeField] float releaseTime; // 반환까지 대기할 시간 (초 단위)

    [SerializeField] ObjectPool pool;
    public ObjectPool Pool { get { return pool; } set { pool = value; } }

    #region Unity Event
    private void OnEnable()
    {
        if (autoRelease)    // autoRelease가 true일 경우
            // 오브젝트를 일정 시간 후 자동 반환하는 코루틴 실행
            StartCoroutine(ReleaseRoutine());
    }
    #endregion

    #region Release
    /// <summary>
    /// 일정 시간(releaseTime) 후 오브젝트 반환하는 메서드를 호출하는 코루틴
    /// 오브젝트를 ObjectPool에 반환하거나, ObjectPool에 대한 정보가 없을 경우 오브젝트를 삭제
    /// </summary>
    /// <returns></returns>
    IEnumerator ReleaseRoutine()
    {
        yield return new WaitForSeconds(releaseTime);
        Release();
    }

    /// <summary>
    /// 이 오브젝트를 ObjectPool에 반환하거나, ObjectPool 정보가 없으면 오브젝트를 삭제하는 메서드
    /// </summary>
    public void Release()
    {
        if (pool != null)
        {
            pool.ReturnPool(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
}