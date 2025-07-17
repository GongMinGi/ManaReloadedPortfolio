using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 개발자: 이예린
/// 
/// 적 캐릭터의 동작 및 네이게이션 이동 등을 관리하는 컨트롤러
/// TODO... 이후 상태 머신이랑 연결해야 함.
/// </summary>
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Movement Setting")]
    [Tooltip("NavMeshAgent component used for enemy movement")]
    [SerializeField] NavMeshAgent agent;    // 적 이동에 사용하는 NavMeshAgent 컴포넌트

    [Header("Pool Object Setting")]
    [SerializeField] PooledObject enemyPooeledObj;

    #region Unity Event
    private void Update()
    {
        if (enemyPooeledObj == null)
            Debug.LogError("PooledObject is null");

        if (EnemyManager.Instance.Player == null)
            return;

        TryTracking();
    }
    #endregion

    #region Tracking Player
    /// <summary>
    /// 적의 플레이어 추적을 시도하는 메서드
    /// NavMesh 위에 있을 경우 플레이어 위치를 목적지로 설정하여 추적,
    /// NavMesh가 아닐 경우 비활성화 후 오브젝트 풀에 반환한다
    /// </summary>
    private void TryTracking()
    {
        if (agent.isOnNavMesh)
            agent.SetDestination(EnemyManager.Instance.Player.transform.position);
        else
            enemyPooeledObj.Release();
    }
    #endregion
}