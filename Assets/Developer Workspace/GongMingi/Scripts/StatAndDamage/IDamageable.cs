using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// * 작성자 : 공민기
///  - 플레이어와 적의 Stat 클래스가 상속받는 인터페이스
///  - 현재 hp 변수만 존재,
///  - 외부에서 TakeDamage 호출을 통해서 상속받는 객체에게 데미지를 입힐 수 있다.
/// </summary>
public interface IDamageable
{
    float HP { get; set; }

    void TakeDamage(float damage);
}
