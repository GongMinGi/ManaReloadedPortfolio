using Game.Combat.Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// * 개발자 : 공민기
///  - 얼음 폭풍에만 사용되는 파라미터
/// </summary>
public class IceTornadoParam : ProjectileParams
{
    public float duration;
    public float pullSpeed;
    public float pullTick;
    public float damageRadius;
    public float damageInterval;
}

/// <summary>
/// * 개발자 : 공민기
///  - 얼음 폭풍 투사체 (고정형 영역)
///  - 일정주기(pullTick)마다 반경 내 적을 중심으로 끌어당김( Warp 기반)
///  - 별도주기 (damageInterval)로 데미지 적용
///  - 수명 종료 시 제어 해제 및 풀 반납
/// </summary>
public class IceTornadoProjectile : AbstractProjectile
{
    // 투사체 내부 타이머
    private float elapsed = 0f;             // 누적 경과 시간
    private float pullTimer = 0f;           // 끌림 주기 누적
    private float damageTimer = 0f;         // 데미지 주기 누적

    // 파라미터 캐시( 가독성, 파라미터접근 비용 감소)
    private float duration;
    private float pullTick;
    private float pullSpeed;
    private float pullRadius;
    private float damageRadius;
    private float damageInterval;
    private float damageArea;
    private float damagePerTick;
    private LayerMask enemyLayer;

    private IceTornadoParam tornadoParam;
    private Collider[] hitsBuffer;

    private HashSet<NavMeshAgent> controlledAgents = new();
    private Dictionary<NavMeshAgent, EnemyController> agentToController = new();

    [SerializeField] private int maxHits = 64;
    [SerializeField] private AnimationCurve pullEase = AnimationCurve.Linear(0, 1, 1, 1);
    [SerializeField] private ParticleSystem tornadoEffect;

    public override void Setup(ProjectileParams param)
    {
        elapsed     = 0f;
        pullTimer   = 0f;
        damageTimer = 0f;

        tornadoParam = param as IceTornadoParam;

        if(hitsBuffer == null)
        {
            hitsBuffer = new Collider[maxHits];
        }

        tornadoEffect.Play();
        StartCoroutine(IceTornadoRoutine());
    }

    private IEnumerator IceTornadoRoutine()
    {
        duration       = tornadoParam.duration;
        pullTick       = tornadoParam.pullTick;
        pullSpeed      = tornadoParam.pullSpeed;
        pullRadius     = tornadoParam.radius;
        damageRadius   = tornadoParam.damageRadius;
        damageInterval = tornadoParam.damageInterval;
        damagePerTick  = tornadoParam.damage;
        enemyLayer     = tornadoParam.enemyL;
        damageArea     = damageRadius * damageRadius;

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            elapsed        += deltaTime;
            pullTimer      += deltaTime;
            damageTimer    += deltaTime;

            if (pullTimer >= pullTick)
            {
                float passedTimePercent = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, duration));
                float ease = pullEase.Evaluate(passedTimePercent);
                DoPull(pullTimer, ease);
                pullTimer = 0f;
            }

            if (damageTimer >= damageInterval)
            {
                DoDamage(damageArea);
                damageTimer = 0f;
            }

            yield return null;  
        }

        foreach (var agent in controlledAgents)
        {
            if(agent.enabled == false || agent.isOnNavMesh == false || agent == null)
            {
                continue;
            }
            
            agent.isStopped = false;
        }

        foreach (var agentEntry in agentToController)
        {
            agentEntry.Value.isBeingControlled = false;
        }

        controlledAgents.Clear();
        agentToController.Clear();

        Release();
    }

    private void DoPull(float deltaTimeForStep, float ease)
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, pullRadius, hitsBuffer, enemyLayer, QueryTriggerInteraction.Ignore);

        if (count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Collider col = hitsBuffer[i];

            if (col == null)
            {
                continue;
            }

            if (col.TryGetComponent<EnemyController>(out var enemyController))
            {
                var agent                     = enemyController.Agent;
                Vector3 curEnemyPos           = agent.transform.position;
                Vector3 enemyToCenter         = transform.position - curEnemyPos;
                float enemyDistanceFromCenter = enemyToCenter.magnitude;

                if(enemyDistanceFromCenter <= 0.001f)
                {
                    continue;
                }

                if(controlledAgents.Contains(agent) == false)
                {
                    enemyController.isBeingControlled = true;
                    agent.isStopped = true;
                    agent.ResetPath();

                    controlledAgents.Add(agent);
                    agentToController[agent] = enemyController; // 제어가 끝난 후 isBeingContorlled 일괄 false를 위해 담아둠

                }
                
                Vector3 deltaPosForThisStep = enemyToCenter.normalized * (pullSpeed * ease * deltaTimeForStep);

                if (deltaPosForThisStep.sqrMagnitude > enemyToCenter.magnitude)
                {
                    deltaPosForThisStep = enemyToCenter;
                }

                Vector3 desiredPos = curEnemyPos + deltaPosForThisStep;

                if (NavMesh.Raycast(curEnemyPos, desiredPos, out var hit, agent.areaMask))
                {
                    desiredPos = hit.position;
                }

                if (NavMesh.SamplePosition(desiredPos, out var sampleHit, 2.0f, NavMesh.AllAreas))
                {
                    desiredPos = sampleHit.position;
                }

                agent.Warp(desiredPos);
                continue;
            }

        }

    }

    private void DoDamage(float damageArea)
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, damageRadius, hitsBuffer, enemyLayer, QueryTriggerInteraction.Ignore);

        if (count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Collider col = hitsBuffer[i];

            if (col == null)
            {
                continue;
            }

            if (col.TryGetComponent(out UnitStats enemy))
            {
                enemy.TakeDamage(damagePerTick); 
            }
        }
    }
        
}
