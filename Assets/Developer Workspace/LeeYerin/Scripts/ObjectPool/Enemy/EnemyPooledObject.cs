using System.Collections;
using UnityEngine;

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

    protected override void OnDeactivated()
    {
        if (!IsDie) return;

        tracker.NotifyDied();

        IsDie = false;
    }
}