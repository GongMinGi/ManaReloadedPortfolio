using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// * 화염 폭풍 (Fire Tornado)
///  - 마우스 커서가 찍는 지면 위치에 3x3 크기의 화염 장판 소환
///  - EffectHandler를 통한 SlowEffect 디버프 적용
/// </summary>
public class FireTornado : BaseCombinationMagic
{
    [SerializeField] private float maxRayDistance = 1000f;
    [SerializeField] private FireTornadoProjectile projectilePrefab;
    private FireTornadoProjectile projectileInstance;

    [Header("화염 폭풍 설정")]
    [SerializeField] private float duration = 6f;       // 스킬 지속 시간 (6초)
    [SerializeField] private float damageRadius = 1.5f;     // 데미지 반경 (1.5f -> 지름 3)
    [SerializeField] private float damageInterval = 0.05f;     // 데미지 틱 주기 (0.5초마다)
    [SerializeField] private float damagePerTick = 50f;      // 데미지 틱당 피해량
    [SerializeField] private LayerMask enemyLayer;              // 적 레이어
    [SerializeField] private LayerMask groundLayer;             // 지면 레이어

    [Header("디버프 설정")]
    [SerializeField] private int slowEffectId;
    [SerializeField] private float slowDuration = 0.6f;     // 틱 주기(0.5초)보다 살짝 길게 주어 끊김 방지
    [SerializeField] private float slowAmount = 0.3f;     // 이동속도 30%로 변경 (Multiply 기준)

    private Transform caster;
    private Camera cam;
    private Vector2 mouse;
    private Vector3 spawnPoint;
    private Ray ray;

    private void Init()
    {
        if (GameModeManager.PoolManager.HasPool(projectilePrefab)) return;
        GameModeManager.PoolManager.CreatePool(projectilePrefab, 5, 12);
    }

    public override void ExecuteSkill()
    {
        Init();

        if (canUseSkill == false) return;

        base.ExecuteSkill();

        caster = GameModeManager.Player.transform;
        cam = Camera.main;
        mouse = Mouse.current.position.ReadValue();
        ray = cam.ScreenPointToRay(mouse);

        if (Physics.Raycast(ray, out var hit, maxRayDistance, groundLayer, QueryTriggerInteraction.Ignore))
        {
            spawnPoint = hit.point;
            Debug.Log("[Fire Tornado] 타겟 평면 감지 성공");
        }
        else
        {
            Debug.Log("[Fire Tornado] 타겟 평면 교차 실패");
            return;
        }

        PooledObject go = GameModeManager.PoolManager.GetPool(projectilePrefab, spawnPoint, caster.rotation);
        projectileInstance = go as FireTornadoProjectile;

        var projectileParam = new FireTornadoParam
        {
            // 공통 파라미터
            speed = 0f,
            maxRange = 0f,
            radius = damageRadius,
            damage = damagePerTick,
            enemyL = enemyLayer,

            // 화염 폭풍 전용 파라미터
            duration = duration,
            damageRadius = damageRadius,
            damageInterval = damageInterval,

            // 디버프 파라미터
            slowEffectId = slowEffectId,
            slowDuration = slowDuration,
            slowAmount = slowAmount
        };

        projectileInstance.Setup(projectileParam);
    }
}
