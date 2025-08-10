using UnityEngine;



/// <summary>
/// * 작성자 : 공민기
///  - 투사체 매개변수를 들고 있는 구조체
///  - 공통적으로 쓰이는 매개변수를 들고 있다.
///  - 투사체별로 추가적인 매개변수가 필요한경우 해당 클래스를 상속하여 추가해주면 된다.
/// </summary>
public class ProjectileParams
{
    public float speed;             // 투사체 이동속도
    public float maxRange;          // 최대 사거리
    public float radius;            // 폭발 반경
    public float damage;            // 데미지
    public LayerMask enemyL;        // 적 레이어
    public LayerMask obstacleL;     // 장애물 레이어
}


/// <summary>
/// * 작성자 : 공민기
///  - 모든 풀링되는 투사체가 상속하는 추상 클래스
///  - pooled object를 상속받고, 개별 투사체들이 이 추상 클래스를 상속받는다.
/// </summary>
public abstract class AbstractProjectile : PooledObject
{
    public abstract void Setup(ProjectileParams p);
}
