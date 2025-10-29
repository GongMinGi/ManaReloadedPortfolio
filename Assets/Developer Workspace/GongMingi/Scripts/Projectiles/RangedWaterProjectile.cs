using Game.Combat.Stats;
using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 개발자: 공민기
/// 물 속성 원거리 투사체
/// - 투사체는 직진 이동
/// - 적과 충돌 시 1회 데미지를 주고, 남은 투사체 사거리 안에서 일정 거리만큼 부드럽게 끌어당김
/// - 끌리는 동안 NavMeshAgent 제어권을 뺏어 Warp 이동
/// - 사거리 도달/풀 반환 시 제어 중인 적들의 권한을 모두 복구
/// </summary>
public class RangedWaterProjectile : AbstractProjectile
{
    [SerializeField] private Rigidbody rb;                                          // 투사체 이동 제어용 Rigidbody
    [SerializeField] private float     dragDistance = 4f;                           // 적을 끌어당길 최대 거리

    private HashSet<UnitStats>    hitUnits         = new HashSet<UnitStats>();      // 이미 피격된 유닛 (중복 데미지 방지)
    private List<NavMeshAgent>    controlledAgents = new List<NavMeshAgent>();      // 제어 중인 NavMeshAgent 리스트
    private List<EnemyController> controlledEnemy  = new List<EnemyController>();   // 제어 중인 EnemyController 리스트

    private ProjectileParams rangedWaterParam;                                      // 발사 시 주입되는 파라미터
    private float            traveledDistance = 0f;                                 // 투사체가 이동한 누적 거리
    private float            dragDuration = 0.25f;                                  // 적을 끌어당기는 데 걸리는 시간
    private Vector3          spawnPos;                                              // 발사 위치

    /// <summary>
    /// 투사체 발사 시 초기화
    /// </summary>
    public override void Setup(ProjectileParams param)
    {
        rangedWaterParam = param;
        hitUnits.Clear();
        controlledAgents.Clear();
        controlledEnemy.Clear();

        spawnPos = transform.position;
        traveledDistance = 0f;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        float deltaMovement = rangedWaterParam.speed * Time.deltaTime;                  // 투사체의 순간 이동량
        rb.MovePosition(transform.position + transform.forward * deltaMovement);
        traveledDistance   += deltaMovement;                                            // 투사체가 이동한 누적거리

        if ((transform.position - spawnPos).sqrMagnitude >= rangedWaterParam.maxRange * rangedWaterParam.maxRange)
        {
            SafeRelease();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 적 충돌시 데미지 적용 후 navemesh 이동 
        if(other.isTrigger == false && rangedWaterParam.enemyL.Contain(other.gameObject.layer))
        {
            UnitStats unitStats = other.GetComponent<UnitStats>();

            if(unitStats != null && hitUnits.Contains(unitStats) == false)
            {
                hitUnits.Add(unitStats);
                unitStats.TakeDamage(rangedWaterParam.damage);

                // Navemesh 제어권 획득
                if (other.TryGetComponent<EnemyController>(out var enemyController))
                {
                    var agent = enemyController.Agent;

                    if (agent != null && agent.enabled && agent.isOnNavMesh)
                    {
                        float remainedProjectileDistance = Mathf.Max(0f, rangedWaterParam.maxRange - traveledDistance);
                        float actualDragDistance = Mathf.Min(dragDistance, remainedProjectileDistance);

                        enemyController.isBeingControlled = true;
                        agent.isStopped = true;
                        agent.ResetPath();

                        dragDuration = actualDragDistance / rangedWaterParam.speed;

                        // 제어중인 에이전트와 애너미컨트롤러리스트 (중복검사는 위에 유닛스텟에서 진행햇으니 불필요)
                        controlledAgents.Add(agent);                
                        controlledEnemy.Add(enemyController);

                        StartCoroutine(DragTarget(enemyController, agent, transform.forward, actualDragDistance, dragDuration));
                    }
                }
            }
        }
    }

    /// <summary>
    /// 충돌한 적을 일정 거리/시간 동안 투사체 방향으로 끌어당김
    /// </summary>
    private IEnumerator DragTarget(EnemyController enemyController, NavMeshAgent agent, Vector3 moveDirection, float moveDistance, float duration)
    {
        Transform target = agent.transform;
        if(target == null)
        {
            yield break;
        }

        Vector3 start = target.position;
        Vector3 end = start + moveDirection.normalized * moveDistance;
        float elapsedTime = 0f;

        // 일정 시간 동안 Lerp를 통해 부드럽게 이동
        while( elapsedTime <= duration && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            elapsedTime += Time.deltaTime;
            float passedTimePercent = Mathf.Clamp01(elapsedTime/ duration);
            Vector3 desired = Vector3.Lerp(start, end, passedTimePercent);

            // 직선 경로에 장애물이 있으면 충돌 지점까지만 이동
            if (NavMesh.Raycast(target.position, desired, out var hit, agent.areaMask))
            {
                desired = hit.position;
            }

            // navmesh 를 벗어난 경우 navmesh위로 위치 보정
            if (NavMesh.SamplePosition(desired, out var sample, 2.0f, NavMesh.AllAreas))
            {
                desired = sample.position;
            }

            agent.Warp(desired);
            yield return null;
        }

        if (agent != null && agent.enabled)
        {
            Vector3 desired = end;
            if (NavMesh.SamplePosition(desired, out var sample, 2.0f, NavMesh.AllAreas))
            {
                desired = sample.position;
            }

            if (agent.isOnNavMesh)
            {
                agent.Warp(desired);
            }
        }

        if (enemyController != null)
        {
            enemyController.isBeingControlled = false;
        }
        
        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
        }
    }

    /// <summary>
    /// 투사체 사거리 종료/풀 반환될 대
    /// 제어 중인 모든 적의 권한을 원복하고 투사체 비활성화
    /// </summary>
    private void SafeRelease()
    {
        StopAllCoroutines();                                // 혹시 남아있을 코루틴 모두 정지

        foreach(var agent in controlledAgents)              // navmesh 제어 해제
        {
            // 스킬 중간에 죽어서 navmesh를 벗어나거나 예기치 못하게 null이 된 경우 예외처리
            if (agent.enabled == false || agent.isOnNavMesh == false || agent == null)
            {
                continue;
            }

            agent.isStopped = false;                        // 이동 정지 해제
        }

        foreach(var enemyController in controlledEnemy)     // Enemycontroller 제어 해제
        {
            enemyController.isBeingControlled = false;
        }

        controlledAgents.Clear();
        controlledEnemy.Clear();
        hitUnits.Clear();

        Release();
    }
}
