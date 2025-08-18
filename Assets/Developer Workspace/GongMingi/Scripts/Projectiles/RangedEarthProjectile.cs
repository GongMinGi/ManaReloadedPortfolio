using Game.Combat.Stats;
using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///  - 땅 속성 원거리공격 투사체 로직 구현
/// </summary>
public class RangedEarthProjectile : AbstractProjectile
{

    #region Field and Property

    float speed;                                        // 이동속도
    float maxRange;                                     // 최대 사거리
    float damage;                                         // 피해량
    float explosionRadius;                                       // 폭발 반경
    LayerMask enemyLayer;                               // 적 레이어
    LayerMask obstacleLayer;                            // 장애물 레이어

    [SerializeField] private Rigidbody rb;              // 물리 이동용 rigidbody

    Vector3 startPos;                                   // 투사체 발사 시작 위치
    bool initialized;                                   // 초기화 여부

    // 필요없으면 지우거나 상위 추상 클래스로 올리기
    public delegate void OnSetup(float speed, float range, float radius, int damage,
                  LayerMask enemyL, LayerMask obstacleL);
    public static OnSetup onSetup;                      // 델리게이트 (아직 테스트용)

    #endregion


    #region Unity Event
    void Awake()
    {
        //onSetup = new OnSetup(Setup);
    }


    private void FixedUpdate()
    {
        if (!initialized) return;                           // 필요한 정보가 초기화되지 않은 경우 무시

        if (Vector3.SqrMagnitude(transform.position - startPos) >= maxRange * maxRange)  // 이동거리를 제곱으로 계산
            Explode();                                      // 이동거리의 제곱이 maxRange 제곱보다 크다면 Explode 호출
    }

    #endregion


    #region AbstractProjectile Implementation

    /// <summary>
    /// - 가져온 투사체 값 초기화
    /// </summary>
    /// <param name="speed"></param>
    /// <param name="range"></param>
    /// <param name="radius"></param>
    /// <param name="damage"></param>
    /// <param name="enemyL"></param>
    /// <param name="obstacleL"></param>
    public override void Setup(ProjectileParams param)
    {

        this.speed = param.speed;
        this.maxRange = param.maxRange;
        this.damage = param.damage;
        this.explosionRadius = param.radius;
        enemyLayer = param.enemyL;
        obstacleLayer = param.obstacleL;
        
        startPos = transform.position;                      // 시작 지점 기록
        initialized = true;                                 // 활성화 true로 변경

        rb.linearVelocity = transform.forward * speed;      // 플레이어 전방 방향으로 발사
    }

    #endregion


    #region Collision And Explode

    private void OnTriggerEnter(Collider other)
    {
        // 트리거가 아니면서(적의 공격박스 등과는 충돌 무시), 적 레이어에 속하면 폭발
        if(!other.isTrigger && enemyLayer.Contain(other.gameObject.layer))
        {
            Explode();
        }
    }


    /// <summary>
    /// - 장애물이나 적과 충돌했을 경우 해당 위치에서 폭발반경 안 적에게 데미지 적용
    /// - 폭발 이펙트 생성
    /// </summary>
    void Explode()
    {
        
        Collider[] hits = Physics.OverlapSphere(
            transform.position, 
            explosionRadius, 
            enemyLayer, 
            QueryTriggerInteraction.Ignore);    // 폭발 범위 내 적 탐색
        foreach (var hit in hits)
        {
            Debug.Log("폭발 피해 적용");
            if (hit.TryGetComponent(out UnitStats target))
                target.TakeDamage(damage);
        }

        // 추후 폭발 이펙트 적용 필요

        rb.linearVelocity = Vector3.zero;                   // 속도 정지
        Release();                                          // 풀로 반환
        initialized = false;                                // 재사용 대기
    }

    #endregion



#if UNITY_EDITOR
    //private void OnDrawGizmos()
    //{
    //    void OnDrawGizemoSelected()                         // 선택 시 폭발 반경 시각화
    //    {
    //        Gizmos.color = new Color(1, 0.5f, 0, 0.25f);
    //        Gizmos.DrawSphere(transform.position, radius);
    //    }
    //}
#endif

}
