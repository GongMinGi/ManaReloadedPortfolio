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
}


/// <summary>
/// * 작성자 : 공민기
///  - abstractProjectile 추상클래스를 상속받는 투사체
///  - 마우스가 가리키는 위치의 상공에서 수직으로 운석을 떨어트린다.
/// </summary>
public class MeteorProjectile : AbstractProjectile
{
    private MeteorParams meteorParams;


    public override void Setup(ProjectileParams p)
    {
        meteorParams = p as MeteorParams;

        transform.position = meteorParams.start;
        StartCoroutine(MeteorRoutine()); 
    }

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
            Debug.Log("while 문 들어옴"); 
            time += Time.deltaTime / duration;

            // 선형 보간
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
                if (col.TryGetComponent(out IDamageable enemy))
                    enemy.TakeDamage(damage);
            }
        }

        Release();
    }

}
