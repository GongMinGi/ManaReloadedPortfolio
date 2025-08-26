using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// Enemy 오브젝트 풀에서 관리되는 개별 적 객체를 나타내는 클래스
/// 
/// 역할:
/// - 풀에서 활성화/비활성화될 때 EnemySpawnTracker와 동기화
/// - 활성화 시 스폰 가능 여부 확인 후 Tracker 갱신
/// - 비활성화 시 정상 사망과 비정상 반환(예: NavMesh 이탈)을 구분하여 Tracker 갱신
/// - 오브젝트 풀 반환 시 상태 초기화 및 콜백 호출
/// </summary>
public class EnemyPooledObject : PooledObject
{
    private EnemySpawnTracker tracker;
    private EnemyPool enemyPool;
    private Vector3 defaultSize;    // 오브젝트의 기본 사이즈

    public bool IsDie { get; set; } = false;    // 죽어서 풀에 반납하는지 여부

    #region Unity Evemt
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => Pool != null);

        Initialize();
    }
    #endregion

    private void Initialize()
    {
        enemyPool = Pool as EnemyPool;
        tracker = enemyPool.Tracker;
        defaultSize = transform.localScale;
    }

    protected override IEnumerator OnActivated()
    {
        yield return new WaitUntil(() => enemyPool != null && enemyPool.Tracker != null);

        if (tracker.CanSpawn)
        {
            if (transform.localScale != defaultSize)    // 오브젝트가 디폴트 사이즈가 아니라면
                transform.localScale = defaultSize; // 디폴트 사이즈로 설정

            tracker.NotifySpawned();
        }
        else
            Release();
    }

    protected override void OnDeactivated(Action onComplete = null)
    {
        if (!IsDie)
            // NavMesh 문제 등 비정상 종료 시 생존 카운트만 감소, 재스폰 가능
            tracker.NotifyDespawned();
        else
        {
            // 정상 사망 시 생존 카운트 감소 + 페이즈 진행 체크
            tracker.NotifyDied();
            IsDie = false;  // 풀 반납 직전에 사망 상태 초기화
        }

        onComplete?.Invoke();
    }
}