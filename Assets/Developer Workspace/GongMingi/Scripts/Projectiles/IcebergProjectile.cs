using Game.Combat.Stats;
using System.Collections;
using UnityEngine;

public class IcebergParams : ProjectileParams
{
    public Vector3 start;
    public Vector3 target;
    public float travelTime;

    public FreezingGroundProjectile groundPrefab;
    public float groundRadius;
    public float groundDuration;
    public float groundTickInterval;
    public float groundTickDamage;

    public int slowEffectId;
    public float slowDuration;
    public float slowAmount;
}

public class IcebergProjectile : AbstractProjectile
{
    private IcebergParams icebergParams;
    [SerializeField] private ParticleSystem icebergVFX;

    public override void Setup(ProjectileParams p)
    {
        icebergParams = p as IcebergParams;
        transform.position = icebergParams.start;

        if( icebergVFX != null)
        {
            icebergVFX.Play();
        }
        StartCoroutine(IcebergRoutine());
    }

    private IEnumerator IcebergRoutine()
    {
        Vector3 start = transform.position;
        Vector3 target = icebergParams.target;
        float time = 0f;
        float duration = icebergParams.travelTime;

        // 2초동안 낙하 처리
        while( time < 1f)
        {
            time += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, target, time);
            yield return null;  
        }

        // 1. 착탄 폭발 데미지 (반경 5)
        if (icebergParams.radius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere(target, icebergParams.radius, icebergParams.enemyL, QueryTriggerInteraction.Ignore);
            foreach (var col in hits)
            {
                if (col.TryGetComponent(out UnitStats enemy))
                {
                    enemy.TakeDamage(icebergParams.damage);
                }
            }
            if (icebergVFX != null) icebergVFX.Stop();
        }

        // 2. 후속 얼음 장판 소환
        PooledObject pooledGo = GameModeManager.PoolManager.GetPool(icebergParams.groundPrefab, target, Quaternion.identity);
        var groundInstance = pooledGo as FreezingGroundProjectile;

        var groundParams = new FreezingGroundParams
        {
            radius = icebergParams.groundRadius, // 장판 반경 (3)
            damage = icebergParams.groundTickDamage,
            enemyL = icebergParams.enemyL,

            center = target,
            duration = icebergParams.groundDuration,
            tickInterval = icebergParams.groundTickInterval,

            slowEffectId = icebergParams.slowEffectId,
            slowDuration = icebergParams.slowDuration,
            slowAmount = icebergParams.slowAmount
        };

        groundInstance.Setup(groundParams);

        Release(); // 빙산 투사체 풀 반환
    }
}
