using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 개발자: 이정
/// - 벽 마법이 생성하는 대지의 벽 투사체 파라미터
/// </summary>
public class EarthWallProjectileParams : ProjectileParams
{
    public Vector3 spawnPoint;              // 소환 위치
    public float duration;              // 벽 유지 시간
    public float spawnTime;          // 벽 생성 시간
    public float xSize;              // 벽 x 크기
    public float ySize;              // 벽 y 크기
    public float zSize;              // 벽 z 크기
    public Quaternion rotation;       // 벽 회전
}

/// <summary>
/// 개발자 : 이정
/// - 벽 마법이 생성하는 대지의 벽 투사체
/// </summary>
public class EarthWallProjectile : AbstractProjectile
{
    private EarthWallProjectileParams earthWallParam;
    private float durationTimer = 0f;
    private float spawnTimer = 0f;

    /// <summary>
    /// - durationTimer: duration이 되면 벽 제거
    /// - spawnTimer: spawnTime 동안 y 크기를 선형 보간하여 벽 생성
    /// </summary>
    private void Update()
    {
        if (durationTimer >= earthWallParam.duration)
        {
            Release();
        }

        if(spawnTimer <= earthWallParam.spawnTime)
        {
            float newY = Mathf.Clamp01(spawnTimer / earthWallParam.spawnTime) * earthWallParam.ySize;
            transform.localScale = new Vector3(earthWallParam.xSize, newY, earthWallParam.zSize);
        }

        durationTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;
    }

    /// <summary>
    /// - 벽 소환에 필요한 매개변수가 담긴 구조체를 받아온다.
    /// </summary>
    public override void Setup(ProjectileParams param)
    {
        earthWallParam = param as EarthWallProjectileParams;
        transform.position = earthWallParam.spawnPoint;
        durationTimer = 0f;
        spawnTimer = 0f;
        transform.rotation = earthWallParam.rotation;
        transform.localScale = new Vector3(earthWallParam.xSize, 0, earthWallParam.zSize);
    }
}
