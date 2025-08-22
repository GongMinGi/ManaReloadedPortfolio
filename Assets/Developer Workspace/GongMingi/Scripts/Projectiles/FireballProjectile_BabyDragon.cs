using Game.Combat.Stats;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// * 작성자: 공민기
/// - 부모 타입 ProjectileParams를 확장하여 'dir(진행 방향)'을 추가.
/// - 발사 시점에 계산된 dir(정규화)이 Setup을 통해 주입된다.
/// </summary>
public class FireBallParams : ProjectileParams
{
    public Vector3 dir;
}


/// <summary>
/// * 작성자 : 공민기
/// 아기 드래곤 전용 파이어볼 투사체.
/// - 풀에서 뽑힌 직후 Setup() 호출로 모든 런타임 파라미터 주입.
/// - Rigidbody 선형 속도(linearVelocity)로 이동(엔진 버전에 따라 velocity 사용 고려).
/// - Trigger 충돌 시 레이어 마스크를 기준으로 처리.
/// </summary>
public class FireballProjectile_BabyDragon : AbstractProjectile
{
    [SerializeField] private Rigidbody fireballRb;
    [SerializeField] private Transform realTransform; // 투사체 파티클이 트랜스폼의 정가운데와 조금 떨어져있음 설정 필요

    // --- 세팅으로 주입될 값들(Setup에서 채워짐) ---
    private float speed;                     // 이동 속도
    private float maxRange;                  // 최대 비행 거리
    private float radius;                    // 충돌 판정 반경(스피어캐스트)
    private float damage;                    // 피해량
    private LayerMask enemyLayer;            // 적(플레이어) 레이어 마스크
    private LayerMask obstacleLayer;         // 장애물 레이어 마스크
    private Vector3 dir;                     // 진행 방향(정규화)

    // --- 내부 상태 ---
    private Vector3 startPos;                // 스폰 지점(사거리 계산용)
    private bool initialized;

    FireBallParams fireballParam;


    /// <summary>
    /// 풀에서 뽑힌 후 반드시 호출되는 초기화.
    /// - 속도/사거리/레이어/방향 등을 세팅하고, Rigidbody 속도를 지정한다.
    /// - Rigidbody가 연결/활성화되어 있어야 하며, 콜라이더는 Trigger여야 OnTriggerEnter가 호출된다.
    /// </summary>
    public override void Setup(ProjectileParams p)
    {
        fireballParam = p as FireBallParams;

        speed           = fireballParam.speed;
        maxRange        = fireballParam.maxRange;
        radius          = fireballParam.radius;
        damage          = fireballParam.damage;
        enemyLayer      = fireballParam.enemyL;
        obstacleLayer   = fireballParam.obstacleL;
        dir             = fireballParam.dir;

        startPos = transform.position;
        initialized = true;

        fireballRb.linearVelocity = dir * speed;
    }


    /// <summary>
    /// 사거리 초과 체크.
    /// - 매 물리 프레임마다 시작점과의 제곱거리로 사거리 비교(루트 연산 회피).
    /// - 초과 시 풀로 반납(Release).
    /// </summary>
    private void FixedUpdate()
    {
        if (!initialized) return;                           // 필요한 정보가 초기화되지 않은 경우 무시

        if (Vector3.SqrMagnitude(transform.position - startPos) >= maxRange * maxRange)  // 이동거리를 제곱으로 계산
            Release();                                      // 이동거리의 제곱이 maxRange 제곱보다 크다면 풀로반납
    }


    /// <summary>
    /// Trigger 충돌 처리.
    /// - enemyLayer(타깃)와 부딪히면 데미지 적용 후 Release.
    /// - 장애물 레이어 처리가 필요하면 조건문을 추가해 Release만 수행 가능.
    /// - 플레이어가 Trigger 콜라이더여도 레이어만 맞으면 명중 처리됨(여기서는 isTrigger 검사 안 함).
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 타깃 레이어(플레이어) 명중 → 데미지, 종료
        if (enemyLayer.Contain(other.gameObject.layer))
        {
            if (other.TryGetComponent(out UnitStats target))
                target.TakeDamage(damage);
            Release();
        }

        // 장애물과 충돌시 소멸
        if(obstacleLayer.Contain(other.gameObject.layer))
        {
            Release();
        }

        
    }

}