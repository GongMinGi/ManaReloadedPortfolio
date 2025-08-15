using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 오브젝트 풀링 시스템에서 개별 오브젝트를 제어하는 클래스
/// 
/// - 풀에서 활성화되면 자동 반환(autoRelease) 여부에 따라 일정 시간 후 자동으로 반환될 수 있음
/// - 풀 정보(ObjectPool)가 없으면 일정 시간 후 자가 삭제됨
/// 
/// - Unity 이벤트 훅(OnActivated, OnDeactivated)을 통해 서브클래스에서 커스텀 초기화/정리 가능
/// </summary>
public class PooledObject : MonoBehaviour
{
    [SerializeField] bool autoRelease;  // 활성화 시 ReleaseRoutine을 자동 실행할지 여부
    [SerializeField] float releaseTime; // 반환까지 대기할 시간 (초 단위)

    [SerializeField] ObjectPool pool;
    public ObjectPool Pool { get { return pool; } set { pool = value; } }

    /// <summary>
    /// 해당 PooledObject가 UI인지 여부를 관리하는 bool 변수
    /// </summary>
    public bool IsUI { get; set; } = false;

    #region Unity Event
    private void OnEnable()
    {
        StartCoroutine(OnActivated());

        if (autoRelease)    // autoRelease가 true일 경우
            // 오브젝트를 일정 시간 후 자동 반환하는 코루틴 실행
            StartCoroutine(ReleaseRoutine());
    }
    #endregion

    #region Hook
    /// <summary>
    /// 오브젝트가 풀에서 활성화될 때 실행되는 커스텀 코루틴 훅
    /// 서브클래스에서 재정의하여 초기화 동작 정의 가능
    /// </summary>
    protected virtual IEnumerator OnActivated() { yield return null; }

    /// <summary>
    /// 오브젝트가 반환 또는 제거될 때 호출되는 훅
    /// 서브클래스에서 재정의하여 정리 로직 구현 가능
    /// </summary>
    protected virtual void OnDeactivated() { }
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
    /// 반드시 사용 후 수동 호출하거나 autoRelease로 자동 호출됨
    /// </summary>
    public void Release()
    {
        OnDeactivated();

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