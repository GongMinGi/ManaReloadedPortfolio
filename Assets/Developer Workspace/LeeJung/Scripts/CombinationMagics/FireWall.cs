using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 개발자: 이정
/// - 플레이어 전방에 불 장판 생성
/// - 불 장판은 일정 시간동안 지속되며, 그 위에 있는 적들에게 지속적인 피해를 입힘
/// </summary>
public class FireWall : BaseCombinationMagic
{
    [Header("Projectile")]
    [SerializeField] private FireAreaProjectile projectilePrefab;     // 소환할 불장판 프리팹
    private FireAreaProjectile projectileInstance;                  // 풀에서 가져온 오브젝트를 다운 캐스팅 하기 위한 인스턴스 변수

    [Header("Fire Area Settings")]
    [SerializeField] private float areaDuration = 5f;                           // 불장판 지속시간
    [SerializeField] private float areaTickInterval = 1f;                       // 불장판 도트데미지 틱 간격
    [SerializeField] private float areaXSize = 2f;                             // 불장판 x 크기
    [SerializeField] private float areaZSize = 6f;                             // 불장판 z 크기
    [SerializeField] private float spawnDistance = 1f;                         // 캐릭터 전방 소환 거리
    [SerializeField] private float tickDamage = 30f;                           // 불장판 도트데미지
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
    /// - 플레이어 전방에 불장판 소환
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

        Vector3 spawnPoint = caster.position + caster.forward * (spawnDistance + areaZSize / 2);
        Quaternion rotation = caster.rotation;
        FireAreaProjectileParams projectileParams = new FireAreaProjectileParams()
        {
            // <공통 파라미터>
            damage = tickDamage,
            enemyL = enemyLayer,

            // <불장판 파라미터>
            spawnPoint = spawnPoint,
            duration = areaDuration,
            tickInterval = areaTickInterval,
            xSize = areaXSize,
            zSize = areaZSize,
            rotation = rotation
        };

        PooledObject go = GameModeManager.PoolManager.GetPool(projectilePrefab, spawnPoint, rotation);
        projectileInstance = go as FireAreaProjectile;
        projectileInstance.Setup(projectileParams);
    }
}
