using Game.Combat.Stats;
using System.Collections;
using UnityEngine;

public class FireTornadoParam : ProjectileParams
{
    public float duration;
    public float damageRadius;
    public float damageInterval;

    public int slowEffectId;
    public float slowDuration;
    public float slowAmount;
}

/// <summary>
/// * 화염 폭풍 투사체 (고정형 영역)
///  - 0.5초 틱마다 범위 내 적에게 데미지 부여 및 SlowEffect 갱신
/// </summary>
public class FireTornadoProjectile : AbstractProjectile
{
    private float elapsed = 0f;
    private float damageTimer = 0f;

    private float duration;
    private float damageRadius;
    private float damageInterval;
    private float damagePerTick;
    private LayerMask enemyLayer;

    private FireTornadoParam tornadoParam;
    private SlowEffect fireSlowEffect;      // 부여할 디버프 객체 캐싱
    private Collider[] hitsBuffer;

    [SerializeField] private int maxHits = 64;
    [SerializeField] private ParticleSystem tornadoEffect;

    public override void Setup(ProjectileParams param)
    {
        elapsed = 0f;
        damageTimer = 0f;

        tornadoParam = param as FireTornadoParam;

        if (hitsBuffer == null)
        {
            hitsBuffer = new Collider[maxHits];
        }

        // Setup 시점에 SlowEffect 객체 미리 생성 및 캐싱
        fireSlowEffect = new SlowEffect(tornadoParam.slowDuration, false, tornadoParam.slowEffectId, tornadoParam.slowAmount, ModifierMode.Multiply);

        if (tornadoEffect != null) tornadoEffect.Play();

        StartCoroutine(FireTornadoRoutine());
    }

    private IEnumerator FireTornadoRoutine()
    {
        duration = tornadoParam.duration;
        damageRadius = tornadoParam.damageRadius;
        damageInterval = tornadoParam.damageInterval;
        damagePerTick = tornadoParam.damage;
        enemyLayer = tornadoParam.enemyL;

        // 스킬 시작 즉시 1회 적용 (선딜레이 없는 타격을 원할 경우)
        ApplyDamageAndSlow();

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            elapsed += deltaTime;
            damageTimer += deltaTime;

            // 설정한 주기(0.5초)마다 데미지와 슬로우 디버프 갱신
            if (damageTimer >= damageInterval)
            {
                ApplyDamageAndSlow();
                damageTimer = 0f;
            }

            yield return null;
        }

        Release();
    }

    /// <summary>
    /// OverlapSphere를 통해 범위 내 대상 식별 후 데미지 및 상태이상 부여
    /// </summary>
    private void ApplyDamageAndSlow()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, damageRadius, hitsBuffer, enemyLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider col = hitsBuffer[i];
            if (col == null) continue;

            if (col.TryGetComponent(out UnitStats enemy))
            {
                // 데미지 처리
                enemy.TakeDamage(damagePerTick);

                // 시스템에 구현된 EffectHandler를 통해 상태이상 부여
                enemy.EffectHandler.AddStatusEffect(fireSlowEffect);
            }
        }
    }
}
