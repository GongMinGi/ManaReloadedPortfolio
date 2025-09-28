using Game.Combat.Stats;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 수호령(GuardianSpirit) 개체를 제어하는 클래스
/// - 초기 위치 및 공격력 설정
/// - 트리거 충돌 시 적에게 피해를 가함
/// </summary>
public class GuardianSpirit : MonoBehaviour
{
    private float damage;
    [SerializeField] LayerMask enemyL;

    /// <summary>
    /// 수호령의 위치와 공격력을 초기화하는 메서드
    /// </summary>
    /// <param name="position">배치할 월드 좌표</param>
    /// <param name="damage">수호령이 적에게 입힐 데미지</param>
    public void Setup(Vector3 position, float damage)
    {
        gameObject.transform.position = position;
        this.damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemyL.Contain(other.gameObject.layer))
        {
            other.TryGetComponent<UnitStats>(out var enemy);
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}