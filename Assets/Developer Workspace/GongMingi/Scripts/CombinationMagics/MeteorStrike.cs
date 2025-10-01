using Cinemachine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 개발자: 공민기 
///  - 플레이어 머리 위에서 생성되어, 마우스 커서가 가리키는 지점으로 일정시간 이동.
///  - 보간 이동한 뒤 착탄/ 피해/ 이펙트를 처리하는 운석 스킬
/// </summary>
public class MeteorStrike : BaseCombinationMagic
{
    [Header("Projectile")]
    [SerializeField] private BurningGroundProjectile addtionalProjectilePrefab;     // 충돌 이후 추가로 소환할 불장판 프리팹
    [SerializeField] private MeteorProjectile projectilePrefab;   // 투사체로 사용할 프리팹 변수
    private MeteorProjectile projectileInstance;                  // 풀에서 가져온 오브젝트를 다운 캐스팅 하기 위한 인스턴스 변수

    [Header("Spawn")]
    [SerializeField] private float spawnHeight = 5f;              // 캐스터 머리 위 생성 높이
    [SerializeField] private float travleTime = 0.6f;             // 이동(낙하) 시간

    [Header("Targeting")]
    [SerializeField] private LayerMask groundLayer;               // 바닥 레이어
    [SerializeField] private float maxRayDistance = 1000f;        // 타겟 레이 거리
    [SerializeField] private float fallbackPlaneY = 0f;           // 바닥이 없을 때 사용할 평면 높이 (운석 최저 고도)

    [Header("Impact")]
    [SerializeField] private float impactRadius = 3f;             // 착탄 범위
    [SerializeField] private int damage = 20;                     // 피해량(프로토타입)
    [SerializeField] private GameObject impactVfxPrefab;          // 착탄 이펙트
    [SerializeField] private LayerMask enemyLayer;                // 적 탐지 레이어


    [Header("BurningGround")]
    [SerializeField] public float groundDuration = 2f;                           // 불장판 지속시간
    [SerializeField] public float groundAttackTickInterval = 0.5f;               // 불장판 도트데미지 틱 간격
    [SerializeField] public float groundAttackDamage = 50f;                      // 불장판 도트메미지 

    private Transform caster;                                     // 스킬 시전자 (플레이어)
    private Camera cam;                                           // 마우스 기준 카메라

    public void Init()
    {
        if (GameModeManager.PoolManager.HasPool(projectilePrefab))
        {
            return;
        }

        GameModeManager.PoolManager.CreatePool(projectilePrefab, 5, 10);   // 풀매니저에 투사체 풀 생성 (초기5개, 최대 10개) 
        GameModeManager.PoolManager.CreatePool(addtionalProjectilePrefab, 5, 10);   // 충돌 이후 불장판을 위한 풀 생성
    }

    /// <summary>
    /// - 소환할 운석의 시작점과 착탄 지점을 연산 
    /// - 운석과 후속으로 소환될 불장판에 필요한 변수값 projectile param에 저장
    /// - 운석 투사체 스크립트의 setup 함수 호출
    /// </summary>
    public override void ExecuteSkill()
    {
        Init();

        Debug.Log("canuseSkill:" + canUseSkill);
        if (!canUseSkill)   // 스킬 쿨타임이 끝났는지 확인
            return;

        base.ExecuteSkill();    // 스킬 쿨타이머 실행

        this.caster = GameModeManager.Player.transform;
        this.cam = Camera.main;


        if (cam == null || caster == null)
        {
            Debug.LogWarning("[MeteorStrike] 카메라 / 캐스터가 없습니다.");
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();              // 마우스 스크린 좌표 획득 
        if (mousePos == null) Debug.LogWarning("[MeteorStrike] 마우스 위치가 null 입니다.");
        Ray ray = cam.ScreenPointToRay(mousePos);                           // 마우스 스크린 좌표이용해 Ray 구조체 획득 ( 카메라 월드 좌표 + perspective 방향 벡터)

        Vector3 targetPoint;                                                // 운석이 떨어질 위치 변수
        if (Physics.Raycast(ray, out var hit, maxRayDistance, groundLayer,
            QueryTriggerInteraction.Ignore))                                // Ray를 쏴서 운석을 떨어트릴 땅의 좌표를 얻어낸다.
            targetPoint = hit.point;
        else                                                                // 땅이 없는 경우 임의의 평면을 만들어서 운석을 충돌시킨다.
        {
            Plane plane = new Plane(Vector3.up, new Vector3(0, fallbackPlaneY, 0));
            if (!plane.Raycast(ray, out float dist))                        // 광선의 시작점과 평면 사이의 거리를 반환한다. 광선과 평면이 만나지 않으면 false를 반환하고 거리를 0으로 설정한다.
            {
                Debug.LogWarning("[MeteorStrike] 타겟 평면 교차 실패");
                return;
            }
            targetPoint = ray.GetPoint(dist);                               // ray 방향으로 dist만큼 떨어진 곳의 좌표를 반환한다. 
        }

        Vector3 start = targetPoint + Vector3.up * spawnHeight;             // 시작 위치 : 착탄 지점 상공

        PooledObject go = GameModeManager.PoolManager.GetPool(
            projectilePrefab, start, caster.rotation);
        projectileInstance = go as MeteorProjectile;

        var projectileParams = new MeteorParams
        {
            // < 공통 파라미터 >
            radius = impactRadius,                                          // 착탄 폭발 반경
            damage = damage,                                                // 주는 데미지
            enemyL = enemyLayer,                                            // 적 레이어

            // < 운석 투사체 전용 파라미터 >
            start = start,                                                  // 운석 시작지점
            target = targetPoint,                                           // 운석 충돌지점
            travelTime = travleTime,                                         // 운석 낙하시간(속도)

            // < 후속 장판 파라미터 >
            addtionalProjectilePrefab = this.addtionalProjectilePrefab,
            groundDuration = this.groundDuration,
            groundAttackTickInterval  = this.groundAttackTickInterval,
            groundAttackDamage = this.groundAttackDamage,
        };

        //GameModeManager.SoundManager.PlaySFX(110020);                       // 운석 소환 직전 운석 낙하 사운드 재생 시작
        projectileInstance.Setup(projectileParams);                         // 운석 소환
    }

}
