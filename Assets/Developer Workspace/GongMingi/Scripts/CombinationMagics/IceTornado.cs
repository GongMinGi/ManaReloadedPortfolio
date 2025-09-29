using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// * 개발자: 공민기
///  - 마우스 커서가 찍는 지면 위치에 토네이도 투사체 소환
///  - poolmanger를 통해 투사체를 getpool한 후, 파라미터를 전달해서 setup
/// </summary>
public class IceTornado : BaseCombinationMagic
{
    [SerializeField] private float maxRayDistance = 1000f;      // 마우스-> 화면-> 월드로 이어지는 레이케스트의 최대 거리
    [SerializeField] IceTornadoProjectile projectilePrefab;     // 풀의 키가 되는 프리팹 (토네이도 투사체 프리팹)
    private IceTornadoProjectile projectileInstance;            

    [Header("Tornado Config")]
    [SerializeField] private float duration         = 6f;       // 스킬 지속 시간
    [SerializeField] private float pullRadius       = 5f;       // 끌어당기는 반경
    [SerializeField] private float damageRadius     = 3f;       // 데미지 반경
    [SerializeField] private float pullTick         = 0.03f;    // 끌림 적용 주기( 작을수록 부드럽게 끌림)
    [SerializeField] private float damageInterval   = 0.2f;     // 데미지 틱 주기
    [SerializeField] private float pullSpeed        = 6f;       // 초당 끌림 속도
    [SerializeField] private float damagePerTick    = 50f;      // 데미지 틱당 피해량
    [SerializeField] private LayerMask enemyLayer;              // 적 레이어
    [SerializeField] private LayerMask groundLayer;             // 지면 레이어

    [System.NonSerialized] private bool poolCreated = false;    // 런타임 중 SO 값 보존 방지

    // 캐스팅 시점 보조 캐시 (가독성 up)
    private Transform caster;
    private Camera    cam;
    private Vector2   mouse;
    private Vector3   spawnPoint;
    private Ray       ray;

    private void OnEnable()
    {
        poolCreated = false;
    }

    /// <summary>
    /// 최초 1회 풀 생성
    /// TODO: poolmanager에서 pool이 존재하는지 검사하는 로직을 만들어 교체 필요
    ///       현재는 리트라이시에 풀이 만들어지지 않는다.
    /// </summary>
    private void Init()
    {
        if(poolCreated == true)
        {
            return;
        }

        GameModeManager.PoolManager.CreatePool(projectilePrefab, 5, 12);
        poolCreated = true;
    }

    public override void ExecuteSkill()
    {
        Init();

        if(canUseSkill == false) 
        {
           return;                                      // 쿨타임 조건 불충족 시 리턴
        }

        base.ExecuteSkill();                            // 공통 쿨타임/UI 처리

        // 마우스 지점의 지면 포인트 샘플링
        caster = GameModeManager.Player.transform;      // 투사체 스폰 회전값 기준
        cam   = Camera.main;
        mouse = Mouse.current.position.ReadValue();
        ray   = cam.ScreenPointToRay(mouse);

        if( Physics.Raycast(ray, out var hit, maxRayDistance, groundLayer, QueryTriggerInteraction.Ignore ) == true)
        {
            spawnPoint = hit.point;
            Debug.Log("[Ice Tornado] 타겟 평면 감지 성공");

        }
        else
        {
            Debug.Log("[Ice Tornado] 타겟 평면 교차 실패");
        }

        PooledObject go = GameModeManager.PoolManager.GetPool(projectilePrefab, spawnPoint, caster.rotation);
        projectileInstance = go as IceTornadoProjectile;

        var projectileParam = new IceTornadoParam
        {
            // 공통 파라미터 
            speed        = 0f,                  
            maxRange     = 0f,
            radius       = pullRadius,          
            damage       = damagePerTick,
            enemyL       = enemyLayer,

            // 얼음 폭풍 전용 파라미터
            duration        = duration,
            damageRadius    = damageRadius,
            pullTick        = pullTick,
            pullSpeed       = pullSpeed,
            damageInterval  = damageInterval
        };

        projectileInstance.Setup(projectileParam);

    }
}
