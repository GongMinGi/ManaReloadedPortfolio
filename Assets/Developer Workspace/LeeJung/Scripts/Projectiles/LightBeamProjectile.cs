using Game.Combat.Stats;
using UnityEngine;

/// <summary>
/// 개발자: 이정
/// - 빛이여 마법이 생성하는 빔 투사체 파라미터
/// </summary>
public class LightBeamProjectileParams : ProjectileParams
{
    public Vector3 startPoint;            // 시작 위치
    public Vector3 endPoint;             // 끝 위치
    public float duration;             // 빔 지속 시간
    public float spawnTime;          // 빔 생성 시간
}

/// <summary>
/// 개발자: 이정
/// - 빛이여 마법이 생성하는 빔 투사체
/// </summary>
public class LightBeamProjectile : AbstractProjectile
{
    private LightBeamProjectileParams lightBeamParams;
    private float durationTimer = 0f;
    private float spawnTimer = 0f;
    private bool isActivated = false;
    private LineRenderer lineRenderer;
    [SerializeField] private GameObject magicCircle;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// - durationTimer: duration이 되면 빔 제거
    /// - spawnTimer: spawnTime 동안 마법진 활성화 후 빔 생성
    /// </summary>
    private void Update()
    {
        if (isActivated == false && spawnTimer <= lightBeamParams.spawnTime)
        {
            spawnTimer += Time.deltaTime;
        }

        if (isActivated == true && durationTimer <= lightBeamParams.duration)
        {
            durationTimer += Time.deltaTime;
            float newRadius = Mathf.Clamp01(durationTimer / lightBeamParams.duration) * lightBeamParams.radius;
            lineRenderer.startWidth = newRadius;
            lineRenderer.endWidth = newRadius;
        }

        if(isActivated==false && spawnTimer >= lightBeamParams.spawnTime)
        {
            isActivated = true;

            if (magicCircle != null)
            {
                magicCircle.SetActive(false);
            }

            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.SetPosition(0, lightBeamParams.startPoint);
                lineRenderer.SetPosition(1, lightBeamParams.endPoint);
            }

            DoDamage();
        }

        if (durationTimer >= lightBeamParams.duration)
        {
            Release();
        }
    }

    /// <summary>
    /// - 빛의 광선 소환에 필요한 매개변수가 담긴 구조체를 받아온다.
    /// </summary>
    public override void Setup(ProjectileParams param)
    {
        lightBeamParams = param as LightBeamProjectileParams;
        transform.position = lightBeamParams.startPoint;
        spawnTimer = 0f;
        durationTimer = 0f;
        isActivated = false;

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0f;
            lineRenderer.endWidth = 0f;
            lineRenderer.SetPosition(0, lightBeamParams.startPoint);
            lineRenderer.SetPosition(1, lightBeamParams.endPoint);
            lineRenderer.enabled = false;
        }

        if (magicCircle != null)
        {
            magicCircle.SetActive(true);
        }
    }

    /// <summary>
    /// - 범위 내의 감지된 적에게 데미지를 준다.
    /// </summary>
    private void DoDamage()
    {
        Collider[] hits = Physics.OverlapCapsule(lightBeamParams.startPoint, lightBeamParams.endPoint, lightBeamParams.radius, lightBeamParams.enemyL);
        
        foreach (var hit in hits)
        {
            UnitStats enemy = hit.GetComponent<UnitStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(lightBeamParams.damage);
            }
        }
    }
}
