using UnityEngine;

public class Wizaboo : BaseCombinationMagic
{
    [Header("장판 설정")]
    [SerializeField] private WizabooAuraProjectile auraProjectilePrefab;
    [SerializeField] private float duration = 7f;
    [SerializeField] private float tickInterval = 0.01f;
    [SerializeField] private int tickDamage = 1;
    [SerializeField] private float radius = 2.5f;

    [Header("디버프 설정")]
    [SerializeField] private int slowEffectId;
    [SerializeField] private float slowDuration = 0.1f;
    [SerializeField] private float slowAmount = 0.6f;

    private Transform caster;

    public void Init()
    {
        if (GameModeManager.PoolManager.HasPool(auraProjectilePrefab))
        {
            return;
        }
        GameModeManager.PoolManager.CreatePool(auraProjectilePrefab, 3, 5);
    }

    public override void ExecuteSkill()
    {
        Init();

        if(canUseSkill == false )
        {
            return;
        }
        base.ExecuteSkill();
        caster = GameModeManager.Player.transform;

        PooledObject go = GameModeManager.PoolManager.GetPool(auraProjectilePrefab, caster.position, caster.rotation);
        WizabooAuraProjectile wizabooAuraProjectile = go as WizabooAuraProjectile;

        var parameters = new WizabooAuraParam 
        {
            center = caster.position,
            radius = this.radius,
            duration = this.duration,
            tickInterval = this.tickInterval,
            damage = this.tickDamage,
            slowEffectId = this.slowEffectId,
            slowDuration = this.slowDuration,
            slowAmount = this.slowAmount
        };

        // 장판을 플레이어의 자식으로 설정하여 자동으로 따라다니게 함
        wizabooAuraProjectile.transform.SetParent(caster);

        wizabooAuraProjectile.Setup(parameters);
    }

}
