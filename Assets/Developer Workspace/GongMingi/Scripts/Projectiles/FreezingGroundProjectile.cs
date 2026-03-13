using Game.Combat.Stats;
using System.Collections;
using UnityEngine;

public class FreezingGroundParams : ProjectileParams
{
    public Vector3 center;
    public float duration;
    public float tickInterval;

    public int slowEffectId;
    public float slowDuration;
    public float slowAmount;
}

/// <summary>
/// 빙산 폭발 후 깔리는 얼음 장판.
/// 반경 내의 적에게 도트 데미지와 둔화 상태이상을 지속적으로 겁니다.
/// </summary>
public class FreezingGroundProjectile : AbstractProjectile
{
    private FreezingGroundParams freezeParams;
    private SlowEffect iceSlowEffect;
    private Coroutine groundCoroutine;

    [SerializeField] private ParticleSystem blizzardVFX; // 장판 파티클

    public override void Setup(ProjectileParams param)
    {
        freezeParams = param as FreezingGroundParams;
        transform.position = freezeParams.center;

        // 전달받은 파라미터로 슬로우 이펙트 객체 생성
        iceSlowEffect = new SlowEffect(freezeParams.slowDuration, false, freezeParams.slowEffectId, freezeParams.slowAmount, ModifierMode.Multiply);

        if (blizzardVFX != null) blizzardVFX.Play();
        groundCoroutine = StartCoroutine(TickRoutine());
    }

    IEnumerator TickRoutine()
    {
        float elapsed = 0f;
        var wait = new WaitForSeconds(freezeParams.tickInterval);

        DoDamageAndSlow(); // 스폰 즉시 1틱 적용

        while (elapsed < freezeParams.duration)
        {
            yield return wait;
            elapsed += freezeParams.tickInterval;
            DoDamageAndSlow();
        }

        if (blizzardVFX != null) blizzardVFX.Stop();
        Release();
    }

    void DoDamageAndSlow()
    {
        if (freezeParams.radius <= 0f) return;

        var hits = Physics.OverlapSphere(freezeParams.center, freezeParams.radius, freezeParams.enemyL, QueryTriggerInteraction.Ignore);
        foreach (var col in hits)
        {
            if (col.TryGetComponent(out UnitStats enemy))
            {
                // 데미지 적용
                enemy.TakeDamage(freezeParams.damage);

                // 슬로우 적용
                enemy.EffectHandler.AddStatusEffect(iceSlowEffect);
            }
        }
    }
}
