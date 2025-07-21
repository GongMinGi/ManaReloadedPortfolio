using System.Collections;
using UnityEngine;

public class EnemyPooledObject : PooledObject
{
    private EnemySpawnTracker tracker;
    private EnemyPool enemyPool;

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
    }

    protected override IEnumerator OnActivated()
    {
        yield return new WaitUntil(() => enemyPool != null && enemyPool.Tracker != null);

        if (tracker.CanSpawn)
            tracker.NotifySpawned();
        else
            Release();
    }

    protected override void OnDeactivated()
    {
        tracker.NotifyDied();
    }
}
