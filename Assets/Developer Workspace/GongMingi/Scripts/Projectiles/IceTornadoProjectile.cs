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
        // 스킬이 실행될 때 내부 타이머 초기화
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
        // 파라미터값 복사
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

            // 부드럽게 끌어당김 ( pullTick 마다 처리)
            if (pullTimer >= pullTick)
            {
                float passedTimePercent = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, duration));
                float ease = pullEase.Evaluate(passedTimePercent);
                DoPull(pullTimer, ease);
                pullTimer = 0f;
            }

            // damageInterval마다 데미지 적용 ( 끌림과 주기 독립 )
            if (damageTimer >= damageInterval)
            {
                DoDamage(damageArea);
                damageTimer = 0f;
            }

            yield return null;  
        }

        // navmeshAgent 제어 해제 ( 스킬 종료 시점 )
        foreach (var agent in controlledAgents)
        {
            // 스킬 중간에 죽어서 navmesh를 벗어나거나 예기치 못하게 null이 된 경우 예외처리
            if(agent.enabled == false || agent.isOnNavMesh == false || agent == null)
            {
                continue;
            }
            
            agent.isStopped = false;                        // 정지 해제
        }

        foreach (var agentEntry in agentToController)
        {
            agentEntry.Value.isBeingControlled = false;     // EnemyController 쪽 네브매쉬 제어 플래그 해제
        }

        controlledAgents.Clear();
        agentToController.Clear();

        Release();
    }

    /// <summary>
    /// Warp 기반 끌어당김
    ///  - OverlapSphereNonAlloc 으로 대상 수집
    ///  - PushSpell과 유사하게 isstopped / resetpath 적용 -> warp -> 스킬 종료 시 복구
    ///  - navemesh.Raycast , SamplePosition으로 충돌/오프메시 보정
    /// </summary>
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

            // EnemyContorller에서 Agent 가져오기, BeingControlled 플래그 제어
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

                // 첫 진입 시 제어 세팅 ( 중복 정지 방지 )
                if(controlledAgents.Contains(agent) == false)
                {
                    enemyController.isBeingControlled = true;
                    agent.isStopped = true;
                    agent.ResetPath();

                    controlledAgents.Add(agent);
                    agentToController[agent] = enemyController; // 제어가 끝난 후 isBeingContorlled 일괄 false를 위해 담아둠

                }
                
                // 이번 틱 이동량 계산
                Vector3 deltaPosForThisStep = enemyToCenter.normalized * (pullSpeed * ease * deltaTimeForStep);

                // 중심 지나침 방지
                if (deltaPosForThisStep.sqrMagnitude > enemyToCenter.magnitude)
                {
                    deltaPosForThisStep = enemyToCenter;
                }

                Vector3 desiredPos = curEnemyPos + deltaPosForThisStep;

                // 직선 경로가 막히면 막히기 전 지점으로 이동
                if (NavMesh.Raycast(curEnemyPos, desiredPos, out var hit, agent.areaMask))
                {
                    desiredPos = hit.position;
                }

                // 네브매쉬에서 벗어난 경우 근처 네브매시로 이동시킴
                if (NavMesh.SamplePosition(desiredPos, out var sampleHit, 2.0f, NavMesh.AllAreas))
                {
                    desiredPos = sampleHit.position;
                }

                agent.Warp(desiredPos);
                continue;
            }

        }

    }

    /// <summary>
    /// 데미지 반경 내 대상에게만 틱 데미지 적용
    /// </summary>
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
