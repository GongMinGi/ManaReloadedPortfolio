using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// * 작성자 : 공민기
///  - 플레이어와 적의 Stat 클래스가 상속받는 인터페이스
///  - currentHp , MaxHp 를 프로퍼티로 선언
///  - 외부에서 TakeDamage 호출을 통해서 상속받는 객체에게 데미지를 입힐 수 있다.
/// </summary>
public interface IDamageable
{
    float MaxHp { get; set; }
    public float CurrentHp { get; set; }
    void TakeDamage(float damage);
}
