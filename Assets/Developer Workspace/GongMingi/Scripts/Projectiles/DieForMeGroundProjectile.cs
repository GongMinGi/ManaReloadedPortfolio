using Game.Combat.Stats;
using UnityEngine;


public class DieForMeParam : ProjectileParams
{
    public Vector3 center;          // 장판 생성 위치
    public float radius;            // 장판 반경
    public LayerMask enemyLayer;    // 적 레이어

    // TimeBomeEffect를 위한 변수
    public float bombDuration;      // 폭발까지 걸리는 시간
    public int bombDamage;          // 폭발 데미지
    public int effectId;            // 상태이상 ID
}

/// <summary>
/// 생성 즉시 반경 내 적에게 시한폭탄 디버프를 걸고 사라지는 장판 객체
/// </summary>
public class DieForMeGroundProjectile : AbstractProjectile
{
    private DieForMeParam dieParam;

    public override void Setup(ProjectileParams param)
    {
        dieParam = param as DieForMeParam;
        transform.position = dieParam.center;

        Collider[] hits = Physics.OverlapSphere(dieParam.center, dieParam.radius, dieParam.enemyLayer, QueryTriggerInteraction.Ignore);

        foreach (Collider col in hits)
        {
            if(col.TryGetComponent(out UnitStats enemy))
            {
                TimeBombEffect bombEffect = new TimeBombEffect(dieParam.bombDuration, dieParam.effectId, dieParam.bombDamage);
                enemy.EffectHandler.AddStatusEffect(bombEffect);
            }
        }

        Invoke(nameof(Release), 1f);
    }
}
