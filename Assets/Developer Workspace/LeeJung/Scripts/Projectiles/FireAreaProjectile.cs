using Game.Combat.Stats;
using UnityEngine;

/// <summary>
/// 개발자: 이정
/// - 화염의 벽 마법이 생성하는 불 장판 투사체 파라미터
/// </summary>
public class FireAreaProjectileParams : ProjectileParams
{
    public Vector3 spawnPoint;              // 소환 위치
    public float duration;              // 장판 유지 시간
    public float tickInterval;          // 데미지 틱 간격
    public float xSize;              // 장판 x 크기
    public float zSize;              // 장판 z 크기
    public Quaternion rotation;       // 장판 회전
}

/// <summary>
/// 개발자 : 이정
/// - 화염의 벽 마법이 생성하는 불 장판 투사체
/// </summary>
public class FireAreaProjectile : AbstractProjectile
{
    private FireAreaProjectileParams fireAreaParam;
    private float tickTimer;
    private float durationTimer = 0f;

    /// <summary>
    /// - tickTimer: 틱 간격마다 DoDamage() 실행
    /// - durationTimer: duration이 되면 장판 제거
    /// </summary>
    private void Update()
    {
        if(tickTimer >= fireAreaParam.tickInterval)
        {
            tickTimer = 0f;
            DoDamage();
        }

        if(durationTimer >= fireAreaParam.duration)
        {
            Release();
        }

        tickTimer += Time.deltaTime;
        durationTimer += Time.deltaTime;
    }

    /// <summary>
    /// - 불장판 소환에 필요한 매개변수가 담긴 구조체를 받아온다.
    /// </summary>
    public override void Setup(ProjectileParams param)
    {
        fireAreaParam = param as FireAreaProjectileParams;
        transform.position = fireAreaParam.spawnPoint;
        tickTimer = fireAreaParam.tickInterval;
        durationTimer = 0f;
        transform.rotation = fireAreaParam.rotation;
        transform.localScale = new Vector3(fireAreaParam.xSize / 2, 1f, fireAreaParam.zSize / 2);
    }

    /// <summary>
    /// - 범위 내의 감지된 적에게 데미지를 준다.
    /// </summary>
    void DoDamage()
    {
        Vector3 halfExtents = new Vector3(fireAreaParam.xSize / 2, 0.5f, fireAreaParam.zSize / 2);
        Quaternion orientation = fireAreaParam.rotation;
        Collider[] hitColliders = Physics.OverlapBox(transform.position, halfExtents, orientation, fireAreaParam.enemyL);

        foreach (var hitCollider in hitColliders)
        {
            UnitStats enemy = hitCollider.GetComponent<UnitStats>();
            
            if (enemy != null)
            {
                enemy.TakeDamage(fireAreaParam.damage);
            }
        }
    }
}
