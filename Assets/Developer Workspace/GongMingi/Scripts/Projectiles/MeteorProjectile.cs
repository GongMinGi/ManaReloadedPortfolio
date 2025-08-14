using Game.Combat.Stats;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///  - 메테오 구현을 위해서 추가로 필요한 파라미터들을 담은 구조체
/// </summary>
public class MeteorParams : ProjectileParams
{
    public Vector3 start;           // 운석 소환 위치
    public Vector3 target;          // 착탄 지점 (월드)
    public float travelTime;        // 이동 시간

    // 충돌이후 장판소환을 위한 파라미터
    public BurningGroundProjectile addtionalProjectilePrefab;
    public float groundDuration;                           // 불장판 지속시간
    public float groundAttackTickInterval;                 // 불장판 도트데미지 틱 간격
    public float groundAttackDamage;                       // 불장판 도트메미지
}


/// <summary>
/// * 작성자 : 공민기
///  - abstractProjectile 추상클래스를 상속받는 투사체
///  - 마우스가 가리키는 위치의 상공에서 수직으로 운석을 떨어트린다.
/// </summary>
public class MeteorProjectile : AbstractProjectile
{

    #region Field_And_Projectile

    private MeteorParams meteorParams;
    private BurningGroundProjectile burningGroundInstance;

    public override void Setup(ProjectileParams p)
    {
        meteorParams = p as MeteorParams;
        transform.position = meteorParams.start;
        StartCoroutine(MeteorRoutine()); 
    }

    #endregion

    #region Skill_Implementation

    private IEnumerator MeteorRoutine()
    {
        Vector3 start = transform.position;                     // 운석 생성 위치
        Vector3 target = meteorParams.target;                   // 운석 착탄 위치
        float time = 0f;                                        // 타이머 변수
        float duration = meteorParams.travelTime;               // 운석 채공 시간
        float impactRadius = meteorParams.radius;               // 운석 착탄 폭발 피해 반경
        float damage = meteorParams.damage;                     // 운석 폭발 데미지
        LayerMask enemyLayer = meteorParams.enemyL;             // 데미지를 적용할 적 레이어

        while( time < 1f)
        {
            time += Time.deltaTime / duration;

            Vector3 pos = Vector3.Lerp(start, target, time);    // 선형 보간을 통해 부드럽게 떨어짐 => 추후 dotween으로 시작은 빨리 갈수록 감속하게 수정

            transform.position = pos;                           // 현재 운석 위치 갱신
            yield return null;
        }

        if(impactRadius > 0f)
        {
            Collider[] hits = Physics.OverlapSphere( 
                target, impactRadius, enemyLayer, QueryTriggerInteraction.Ignore);  // 착탄지점에 구체모양의 콜라이더로 데미지 적용
            foreach (var col in hits)
            {
                if (col.TryGetComponent(out UnitStats enemy))
                    enemy.TakeDamage(damage);
            }
        }

        // 운석충돌 이후 후속으로 깔릴 불장판에 필요한 변수 초기화 및 호출
        #region following_Projectile

        PooledObject pooledGo = GameModeManager.PoolManager.GetPool(
            meteorParams.addtionalProjectilePrefab,
            target, Quaternion.identity);
        burningGroundInstance = pooledGo as BurningGroundProjectile;

        var burningGroundParm = new BurningGroundParams
        {
            // < 공통 파라미터 >
            radius = meteorParams.radius,
            damage = meteorParams.groundAttackDamage,
            enemyL = meteorParams.enemyL,

            // < 불장판 전용 파라미터 > 
            center = target,
            duration = meteorParams.groundDuration,
            tickInterval = meteorParams.groundAttackTickInterval,
            yOffset = 0.03f,

        };

        burningGroundInstance.Setup(burningGroundParm);

        #endregion

        Release();
    }

    #endregion

}
