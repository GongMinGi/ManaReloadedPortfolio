using UnityEngine;

/// <summary>
/// 개발자: 이정
/// - 플레이어 전방에 대지의 벽 생성
/// - 벽은 일정 시간동안 지속됨
/// </summary>
public class EarthWall : BaseCombinationMagic
{
    [Header("Projectile")]
    [SerializeField] private EarthWallProjectile projectilePrefab;     // 소환할 벽 프리팹
    private EarthWallProjectile projectileInstance;                  // 풀에서 가져온 오브젝트를 다운 캐스팅 하기 위한 인스턴스 변수

    [Header("Earth Wall Settings")]
    [SerializeField] private float wallDuration = 10f;                           // 벽 지속시간
    [SerializeField] private float wallSpawnTime = 0.5f;                         // 벽 생성 시간
    [SerializeField] private float wallXSize = 6f;                             // 벽 x 크기
    [SerializeField] private float wallYSize = 2f;                             // 벽 y 크기
    [SerializeField] private float wallZSize = 1f;                             // 벽 z 크기
    [SerializeField] private float spawnDistance = 4f;                         // 캐릭터 전방 소환 거리

    private Transform caster;                                     // 스킬 시전자 (플레이어)

    /// <summary>
    /// - 풀 생성 및 초기화
    /// </summary>
    private void Init()
    {
        if(GameModeManager.PoolManager.HasPool(projectilePrefab))
        {
            return;
        }

        GameModeManager.PoolManager.CreatePool(projectilePrefab, 5, 10);   // 풀매니저에 투사체 풀 생성 (초기5개, 최대 10개)
    }

    /// <summary>
    /// - 플레이어 전방에 벽 소환
    /// </summary>
    public override void ExecuteSkill()
    {
        if(canUseSkill == false)
        {
            return;
        }

        Init();
        base.ExecuteSkill();
        caster = GameModeManager.Player.transform;

        if(caster == null)
        {
            return;
        }

        Vector3 spawnPoint = caster.position + caster.forward * (spawnDistance + wallZSize / 2);
        Quaternion rotation = caster.rotation;
        EarthWallProjectileParams projectileParams = new EarthWallProjectileParams()
        {
            // <벽 파라미터>
            spawnPoint = spawnPoint,
            duration = wallDuration,
            spawnTime = wallSpawnTime,
            xSize = wallXSize,
            ySize = wallYSize,
            zSize = wallZSize,
            rotation = rotation
        };

        PooledObject go = GameModeManager.PoolManager.GetPool(projectilePrefab, spawnPoint, rotation);
        projectileInstance = go as EarthWallProjectile;
        projectileInstance.Setup(projectileParams);
    }
}
