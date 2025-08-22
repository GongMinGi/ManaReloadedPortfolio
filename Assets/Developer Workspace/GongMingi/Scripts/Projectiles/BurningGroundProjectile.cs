using Game.Combat.Stats;
using System.Collections;
using UnityEngine;


/// <summary>
/// * 작성자: 공민기
///  - 불장판을 소환하기 위해 추가적으로 필요한 변수를 담은 구조체 클래스
/// </summary>
public class BurningGroundParams : ProjectileParams
{
    public Vector3 center;              // 장판 중심 ( 소환 위치)
    public float duration;              // 장판 유지 시간
    public float tickInterval;          // 데미지 틱 간격 
    public float yOffset = 0.05f;       // 지면에서 살짝 띄우기
}


/// <summary>
/// * 작성자 : 공민기
///  - 운석이 충돌한 이후 그 자리에 깔리는 도트딜 장판
///  - 운석과 마찬가지로 투사체 취급.
///  - MeteorProjectile 에서 소환한다.
/// </summary>
public class BurningGroundProjectile : AbstractProjectile
{


    #region FieldAndProperty

    private BurningGroundParams burnParam;                                   // 불장판 파라미터를 가지고 있는 구조체
    private Coroutine burningGroundCoroutine;
    [SerializeField] private ParticleSystem meteorExplosion;

    /// <summary>
    /// - 불장판 소환에 필요한 매개변수가 담긴 구조체를 받아온다.
    /// </summary>
    public override void Setup(ProjectileParams param)               
    {
        burnParam = param as BurningGroundParams;
        transform.position = burnParam.center;                        // 불장판의 소환지점은 운석 충돌의 중심지
        meteorExplosion.Play();
        burningGroundCoroutine = StartCoroutine(TickRoutine()); 
    }

    #endregion

    #region Skill Implementation

    /// <summary>
    /// * 불장판의 실행 매커니즘
    /// - 소환 즉시 장판위의 적에게 1틱의 데미지를 부여한다.
    /// - 이후 일정 시간마다 적에게 데미지를 입히고, 시간이 다하면 소멸한다.
    /// </summary>
    IEnumerator TickRoutine()
    {
        float elapsed = 0f;                                          // 불장판 지속 경과 시간
        var wait = new WaitForSeconds(burnParam.tickInterval);      

        DoDamage();                                                  // 스폰 즉시 일단 1틱 데미지

        while (elapsed < burnParam.duration)                         // 일정 시간마다 데미지 적용
        {
            yield return wait;
            elapsed += burnParam.tickInterval;
            DoDamage();
        }

        meteorExplosion.Stop();
    
        Release();
    }

    /// <summary>
    /// - overlapSphere로 감지된 적에게 데미지를 준다.
    /// - tick마다 overlapsphere 로 감지하여 데미지 적용
    /// </summary>
    void DoDamage()
    {
        if (burnParam.radius <= 0f) return;
        var hits = Physics.OverlapSphere(burnParam.center, burnParam.radius, 
            burnParam.enemyL, QueryTriggerInteraction.Ignore);      // tickInterval 마다 overlapsphere 실행
        foreach ( var col in hits)
        {
            if (col.TryGetComponent(out UnitStats enemy))
                enemy.TakeDamage(burnParam.damage);
        }
    }

    #endregion

}
