using Game.Combat.Stats;
using System.Collections.Generic;
using UnityEngine;

public class WizabooAuraParam : ProjectileParams
{
    public Vector3 center;
    public float duration;
    public float tickInterval;

    public int slowEffectId;
    public float slowDuration;
    public float slowAmount;
}

public class WizabooAuraProjectile : AbstractProjectile
{

    private WizabooAuraParam auraParam;
    private SlowEffect auraSlowEffect;

    [SerializeField] private ParticleSystem wizabooAuraVFX;
    private SphereCollider auracollider;

    private Dictionary<UnitStats, float> targetInZone = new Dictionary<UnitStats, float>();
    private float lifeTimer = 0f;


    private void Awake()
    {
        auracollider = GetComponent<SphereCollider>();
        if(auracollider == null)
        {
            auracollider = gameObject.AddComponent<SphereCollider>();
        }
        auracollider.isTrigger = true; 
    }

    public override void Setup(ProjectileParams param)
    {
        auraParam = param as WizabooAuraParam;
        auracollider.radius = auraParam.radius;
        auraSlowEffect = new SlowEffect(auraParam.slowDuration, false, auraParam.slowEffectId, auraParam.slowAmount, ModifierMode.Multiply);

        targetInZone.Clear();
        lifeTimer = auraParam.duration;
        if(wizabooAuraVFX != null)
        {
            wizabooAuraVFX.Play();
        }
    }

    private void Update()
    {
        lifeTimer -= Time.deltaTime;
        if(lifeTimer <= 0f)
        {
            ReleaseAura();
            return;
        }

        List<UnitStats> currentTargets = new List<UnitStats>(targetInZone.Keys);
        foreach (UnitStats target in currentTargets)
        {
            if(target == null || target.HP<=0)
            {
                targetInZone.Remove(target);
                continue;
            }

            targetInZone[target] += Time.deltaTime;

            if (targetInZone[target] >= auraParam.tickInterval)
            {
                target.TakeDamage(auraParam.damage);
                target.EffectHandler.AddStatusEffect(auraSlowEffect);
                targetInZone[target] -= auraParam.tickInterval;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out UnitStats enemy))
        {
            if(targetInZone.ContainsKey(enemy) == false)
            {
                targetInZone.Add(enemy, auraParam.tickInterval);
            }    
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out UnitStats enemy))
        {
            if(targetInZone.ContainsKey(enemy))
            {
                targetInZone.Remove(enemy);
            }
        }
    }


    private void ReleaseAura()
    {
        transform.SetParent(null);
        targetInZone.Clear();
        Release();
    }    
}
