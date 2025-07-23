using UnityEngine;


/// <summary>
/// * 작성자 : 공민기
///  - 모든 풀링되는 투사체가 상속하는 추상 클래스
///  - pooled object를 상속받고, 개별 투사체들이 이 추상 클래스를 상속받는다.
/// </summary>
public abstract class AbstractProjectile : PooledObject
{

    // 개별 투사체가 반드시 구현해야 하는 초기화 메서드 
    public abstract void Setup(float speed, float range, float radius, 
        int damage, LayerMask enemyL, LayerMask obstacleL);



}
