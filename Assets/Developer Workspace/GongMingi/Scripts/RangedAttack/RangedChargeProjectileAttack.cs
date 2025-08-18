using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// * 작성자 : 공민기
///  - 캐스팅 속성에 땅 속성이 포함되어 있을 때 시전되는 원거리공격
///  - 마우스 좌클릭을 누르는 동안 차지하고 때는 순간 투사체를 발사한다
///  - 풀링으로 관리
/// </summary>
public class RangedChargeProjectileAttack : MonoBehaviour, IRangedAttack, IRequireAttackContext, IAttackSignals
{

    #region Field and Property

    private RangedAttackContext _ctx;

    public event Action Started;
    public event Action<float> Progress;
    public event Action Ended;
    public event Action Interrupted;


    [Header("Projectile Pool / Muzzle")]
    [SerializeField] private RangedEarthProjectile projectilePrefab;    // 원거리 공격 시 발사할 바위 프리팹
    [SerializeField] private Transform muzzle;                          // 발사할 위치 ( 발사체 시작 위치 )


    [Header("Projectile Spec")]
    [SerializeField] private float projectileSpeed = 15f;               // 투사체 속도
    [SerializeField] private float projectileRange = 18f;               // 투사체 최대 사거리
    [SerializeField] private float explosionRadius = 4f;                // 투사체 폭발 반경


    [Header("Charge")]
    [SerializeField] private int baseDamage = 20;                       // 최소 데미지
    [SerializeField] private int damageStep = 10;                       // 차지 한 단계당 데미지 증가량
    [SerializeField] private int maxDamage = 100;                       // 차지 한계 데미지
    [SerializeField] private float chargeInteval = 0.4f;                // 차지 데미지 상승 간격(초)

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;                      // 적 식별 레이어
    [SerializeField] private LayerMask obstacleLayer;                   // 장애물 식별 레이어

    Coroutine chargeCo;                                                 // 차지 코루틴 핸들러
    bool isCharging;                                                    // 차지 중 여부
    int currentDamage;                                                  // 현재 누적(강화)된 데미지
    RangedEarthProjectile projectileInstance;                           // 발사할 투사체에 정보(매개변수)를 전달하기 위해 다운캐스팅한 인스턴스를 저장 할

    #endregion


    #region Unity Event
    void Awake()
    {
        if (!muzzle) muzzle = transform;                                // 투사체 시작 위치가 정해지지 않았다면 본인의 transform 사용
    }

    void Start()
    {
        GameModeManager.PoolManager.CreatePool(projectilePrefab, 5, 10);   // 풀매니저에 투사체 풀 생성 (초기5개, 최대 10개) 
    }

    #endregion


    public void BindContext(RangedAttackContext ctx) => _ctx = ctx;


    #region Interface Implementation

    /// <summary>
    /// - rangedAttack을 상속하는 클래스가 공통으로 가지는 메서드
    /// - 원거리 공격을 실행시키는 트리거
    /// - 현재는 차지여부를 확인하고 차지를 시작.
    /// </summary>
    /// <param name="type"></param>
    public void ExecuteAttack(E_CastingType type)
    {
        if (isCharging) return;                                         // 이미 차지 중이라면 무시
        chargeCo = StartCoroutine(ChargeProcess());                     // 차지 시작
    }


    /// <summary>
    /// - 클래스 외부에서 차지를 멈출 수 있는 메서드
    /// </summary>
    public void Stop()
    {
        if (!isCharging) return;                                        // 차지 중이 아니라면 무시
        StopCoroutine(chargeCo);                                        // 차지 취소
        isCharging = false;

    }

    #endregion


    #region Charge And Projectile

    /// <summary>
    /// - 마우스( 원거리 공격 버튼 )를 누르고 있는 동안 계속해서 차지
    /// - 일정 시간이 지날 때마다 데미지가 단계적으로 증가한다
    /// - 마우스에서 손을 땠을 때 차지를 종료하고 투사체를 발사합니다.
    /// </summary>
    /// <returns></returns>
    IEnumerator ChargeProcess()
    {
        isCharging = true;
        currentDamage = baseDamage;                                     // 초기 데미지 설정
        float timer = 0f;                                               // 차지 간격 타이머

        Started?.Invoke();              // 차지 시작

        while (Mouse.current.leftButton.isPressed)                      // 마우스를 누르고 있는 동안
        {
            timer += Time.deltaTime;                                    // 타이머 증가
            if ( timer >= chargeInteval )                               // 일정 간격을 넘으면
            {
                timer -= chargeInteval;                                 // 타이머 초기화
                currentDamage = Mathf.Min(currentDamage + damageStep, maxDamage);   // 데미지 강화
                // Todo: 차지 단계벌 VFX / UI 게이지 업데이트
            }

            //진행중 처리
            float denom = Mathf.Max(1, maxDamage - baseDamage);
            float t = Mathf.Clamp01((currentDamage - baseDamage) / (float)denom);
            Progress?.Invoke(t);


            yield return null;                                          // 다음 프레임까지 대기
        }

        FireProjectile();                                               // 버튼을 놓는 순간 발사
        isCharging = false;                                             // 차지 종료
        Ended?.Invoke();        // 발사 후 종료
    }   


    /// <summary>
    /// - 인스펙터에 등록된 투사체 프리팹을 발사한다.
    /// </summary>
    void FireProjectile()
    {
        PooledObject go = GameModeManager.PoolManager.GetPool(
            projectilePrefab, muzzle.position, muzzle.rotation);        // 풀에서 오브젝트 가져오기 (위치, 회전 지정)
        projectileInstance = go as RangedEarthProjectile;               // 투사체 내부에 setup함수를 호출하기 위해서 다운캐스팅

        var projectileParam = new ProjectileParams
        {
            speed = projectileSpeed,
            maxRange = projectileRange,
            radius = explosionRadius,
            damage = currentDamage,
            enemyL = enemyLayer,
            obstacleL = obstacleLayer,
        };


        projectileInstance.Setup(projectileParam);                                      // 투사체 오브젝트에 변수 전
    }

    #endregion
}
