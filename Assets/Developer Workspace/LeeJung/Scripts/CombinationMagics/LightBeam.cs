using UnityEngine;

/// <summary>
/// 개발자: 이정
/// - 플레이어 전방에 빛의 광선 소환
/// </summary>
public class LightBeam : BaseCombinationMagic
{
    [Header("Projectile")]
    [SerializeField] private LightBeamProjectile projectilePrefab;     // 소환할 빛의 광선 프리팹
    private LightBeamProjectile projectileInstance;                  // 풀에서 가져온 오브젝트를 다운 캐스팅 하기 위한 인스턴스 변수

    [Header("Light Beam Settings")]
    [SerializeField] private float duration = 1f;                           // 빔 지속시간
    [SerializeField] private float spawnTime = 0.2f;                         // 빔 생성 시간
    [SerializeField] private float radius = 1f;                             // 빔의 반지름
    [SerializeField] private float damage = 1000f;                           // 빔 데미지
    [SerializeField] private float spawnDistance = 1f;                         // 캐릭터 전방 소환 거리
    [SerializeField] private float lightBeamLength = 10f;                      // 빔 길이                           
    [SerializeField] private LayerMask enemyLayer;                // 적 탐지 레이어

    private Transform caster;                                     // 스킬 시전자 (플레이어)

    /// <summary>
    /// - 풀 생성 및 초기화
    /// </summary>
    private void Init()
    {
        if (GameModeManager.PoolManager.HasPool(projectilePrefab))
        {
            return;
        }

        GameModeManager.PoolManager.CreatePool(projectilePrefab, 5, 10);   // 풀매니저에 투사체 풀 생성 (초기5개, 최대 10개)
    }

    /// <summary>
    /// - 플레이어 전방에 빛의 광선 소환
    /// </summary>
    public override void ExecuteSkill()
    {
        if (canUseSkill == false)
        {
            return;
        }

        Init();
        base.ExecuteSkill();
        caster = GameModeManager.Player.transform;

        if (caster == null)
        {
            return;
        }

        Vector3 spawnPoint = caster.position + caster.forward * spawnDistance + Vector3.up * 1f; // 플레이어 높이 보정
        Vector3 endPoint = spawnPoint + caster.forward * lightBeamLength;
        Quaternion rotation = caster.rotation;

        LightBeamProjectileParams projectileParams = new LightBeamProjectileParams()
        {
            // <공통 파라미터>
            damage = damage,
            enemyL = enemyLayer,

            // <빛의 광선 파라미터>
            startPoint = spawnPoint,
            endPoint = endPoint,
            duration = duration,
            spawnTime = spawnTime,
            radius = radius,
        };

        PooledObject go = GameModeManager.PoolManager.GetPool(projectilePrefab, spawnPoint, rotation);
        projectileInstance = go as LightBeamProjectile;
        projectileInstance.Setup(projectileParams);
    }
}
