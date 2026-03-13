using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마우스 커서 위치로 2초간 낙하하는 빙산을 소환하는 스킬.
/// 폭발 후 둔화와 도트 데미지를 유발하는 얼음 장판을 생성합니다.
/// </summary>
public class IcebergStrike : BaseCombinationMagic
{
    [Header("Projectile")]
    [SerializeField] private FreezingGroundProjectile groundProjectilePrefab; // 충돌 후 생성될 얼음 장판
    [SerializeField] private IcebergProjectile icebergPrefab;                 // 떨어지는 빙산 투사체
    private IcebergProjectile icebergInstance;

    [Header("Spawn")]
    [SerializeField] private float spawnHeight = 15f;           // 생성 높이
    [SerializeField] private float travelTime = 2f;             // 낙하 시간 (요청: 2초)

    [Header("Targeting")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxRayDistance = 1000f;
    [SerializeField] private float fallbackPlaneY = 0f;

    [Header("Impact (Iceberg)")]
    [SerializeField] private float impactRadius = 5f;           // 폭발 반경 (요청: 5)
    [SerializeField] private int burstDamage = 50;              // 폭발 데미지
    [SerializeField] private LayerMask enemyLayer;

    [Header("Freezing Ground (Area of Effect)")]
    [SerializeField] private float groundRadius = 3f;           // 장판 반경 (요청: 3)
    [SerializeField] private float groundDuration = 4f;         // 장판 지속시간 (요청: 4초)
    [SerializeField] private float groundTickInterval = 0.5f;   // 도트 데미지 간격
    [SerializeField] private int groundTickDamage = 15;         // 도트 데미지량

    [Header("Slow Effect Settings")]
    [SerializeField] private int slowEffectId;                  // 슬로우 효과 ID
    [SerializeField] private float slowDuration = 1f;           // 슬로우 지속 시간 (틱마다 갱신)
    [SerializeField] private float slowAmount = 0.6f;           // 슬로우 강도 (ex. 0.6f -> 40% 슬로우)

    private Transform caster;
    private Camera cam;

    public void Init()
    {
        if (GameModeManager.PoolManager.HasPool(icebergPrefab)) return;

        GameModeManager.PoolManager.CreatePool(icebergPrefab, 5, 10);
        GameModeManager.PoolManager.CreatePool(groundProjectilePrefab, 5, 10);
    }

    public override void ExecuteSkill()
    {
        Init();

        if (!canUseSkill) return;
        base.ExecuteSkill();

        caster = GameModeManager.Player.transform;
        cam = Camera.main;

        if (cam == null || caster == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = cam.ScreenPointToRay(mousePos);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out var hit, maxRayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0, fallbackPlaneY, 0));
            if (!plane.Raycast(ray, out float dist)) return;
            targetPoint = ray.GetPoint(dist);
        }

        Vector3 start = targetPoint + Vector3.up * spawnHeight;

        PooledObject go = GameModeManager.PoolManager.GetPool(icebergPrefab, start, caster.rotation);
        icebergInstance = go as IcebergProjectile;

        var parameters = new IcebergParams
        {
            radius = impactRadius,
            damage = burstDamage,
            enemyL = enemyLayer,

            start = start,
            target = targetPoint,
            travelTime = travelTime,

            groundPrefab = this.groundProjectilePrefab,
            groundRadius = this.groundRadius,
            groundDuration = this.groundDuration,
            groundTickInterval = this.groundTickInterval,
            groundTickDamage = this.groundTickDamage,

            slowEffectId = this.slowEffectId,
            slowDuration = this.slowDuration,
            slowAmount = this.slowAmount
        };

        icebergInstance.Setup(parameters);
    }
}