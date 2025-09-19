using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.DualShock;

public class PushSpell : BaseCombinationMagic
{
    [Header("Ranges")]
    [SerializeField] private float detectRadius  = 4f;     // 감지 반경
    [SerializeField] private float pushRadius    = 7f;     // 밀어내는 반경

    [Header("Motion")]
    [SerializeField] private float pushDuration  = 1f;  // 적을 밀어내는 데 걸리는 시간
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Filter")]
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
    [SerializeField] private LayerMask enemyLayer;

    [SerializeField] Vector3 center;
    [SerializeField] private int maxHits = 64;

    private Context ctx;
    private Collider[] hitsBuffer = null;

    public override void ExecuteSkill()
    {
        if (canUseSkill == false) 
        { 
            return; 
        }

        Debug.Log("execute skill 들어옴");
        base.ExecuteSkill();

        ctx.caster          = GameModeManager.Player.transform;
        ctx.coroutineRunner = GameModeManager.Player;
        center              = ctx.caster.position;
        hitsBuffer          = new Collider[maxHits];
        var uniqueOwner     = new HashSet<Transform>();                             // 중복 콜라이더 제거용 
        int enemyCount      = Physics.OverlapSphereNonAlloc(center, detectRadius, hitsBuffer, enemyLayer, triggerInteraction);

        for (int i = 0; i < enemyCount; i++)
        {
            Collider col = hitsBuffer[i];                                            // 감지한 적의 콜라이더를 하나 씩 뽑는다.
            if (col == null) continue;                                               // 콜라이더가 없으면 생략

            //Transform enemyTransform = col.transform;                                     // 콜라이더의 최상위 개체를 가져온다
            col.TryGetComponent<EnemyController>(out var enemyController);
            //if (uniqueOwner.Add(enemyTransform) == false) continue;                       // 해시 셋에 추가해서 중복검사

            //enemyTransform.TryGetComponent<NavMeshAgent>(out var agent);                  // 적의 네브매쉬 컴포넌트를 뽑아온다.
            var enemyAgent = enemyController.Agent;
            ctx.coroutineRunner.StartCoroutine(PushAgentRoutine(enemyController, enemyAgent));     
        }
    }

    /// <summary>
    /// 단일 적을 "center" 기준 외각 링까지 부드럽게 밀어냄 
    /// </summary>
    private IEnumerator PushAgentRoutine(EnemyController enemyController, NavMeshAgent enemyAgent)
    {
        float   elapsedTime   = 0f;
        var     waiter        = new WaitForFixedUpdate();
        Vector3 start         = enemyController.transform.position;

        enemyController.isBeingControlled = true;

        if (enemyAgent == null) 
        {
            enemyController.isBeingControlled = false;
            yield break; 
        }

        if (GetTargetOnRing(start, center, out Vector3 desired) == false) 
        {
            enemyController.isBeingControlled = false;
            yield break; 
        }

        if ( NavMesh.Raycast(start, desired, out var hit, enemyAgent.areaMask ))      // 벽, 장애물 등으로 직선 경로가 막히면 막히기 직전 지점까지로 조정
        {
            desired = hit.position;
        }

        if ( NavMesh.SamplePosition(desired, out NavMeshHit sampleHit, 2.0f, NavMesh.AllAreas))
        {
            desired = sampleHit.position;
        }

        enemyAgent.isStopped = true;
        enemyAgent.ResetPath();

        while (elapsedTime < pushDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            //float animCurve = ease.Evaluate(Mathf.Clamp01(elapsedTime / pushDuration));     // ease 커브에 따라 선형 보간( 감속/ 가속 느낌 조절)
            float t = elapsedTime / pushDuration;

            if(t >= 1f)
            {
                enemyAgent.Warp(desired);
                break;
            }

            enemyAgent.Warp(Vector3.Lerp(start, desired, t));
            yield return waiter;
        }

        enemyAgent.isStopped = false;
        enemyController.isBeingControlled = false;
    }

    // 현재 위치가 감지 반경 안이면, 중심에서 정확히 pushRadius 원거리의 목표 점 반환
    private bool GetTargetOnRing(Vector3 enemyPos, Vector3 center, out Vector3 target)
    {
        Vector3 playerToEnemy   = enemyPos - center;                    // 플레이어 -> 적 방향벡터
        float distance          = playerToEnemy.magnitude;              // 플레이어로부터 떨어진 거리

        if( distance > detectRadius )                                   // 거리가 감지범위보다 멀리 있는경우
        {
            target = enemyPos;                                          // 바로 리턴하고 코루틴 끔
            return false;
        }

        if( distance < 0.001f )                                         // 플레이어와 적이 너무 가까이 붙어있을 경우 랜덤방향으로 밀어낸다.
        {
            Vector3 randomEnemyPos = Random.insideUnitSphere;           // 원점 기준으로 반지름이 1인 구 내부의 랜덤으로 점을 찍어서 그 방향벡터를 얻는다. (임의의 방향 도출)
            randomEnemyPos.y = 0;                                       // 공중에 있는 경우 y값 0으로 설정
            playerToEnemy = randomEnemyPos;                   
        }

        Vector3 dir = playerToEnemy.normalized;                         // 방향벡터 정규화
        target = center + dir * pushRadius;                             // pushRadius만큼 떨어진 지점을 목표로 설정
        return true;
    }
}
