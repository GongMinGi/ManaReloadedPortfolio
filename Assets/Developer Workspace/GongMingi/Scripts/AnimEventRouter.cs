using Game.combat.EnemyAttack;
using UnityEngine;


/// <summary>
/// * 작성자: 공민기
/// 애니메이션 이벤트 라우터.
/// - 애니메이터 클립의 이벤트에서 이 컴포넌트를 호출하면,
///   실제 공격 로직이 있는 컴포넌트(BabyDragonDefaultAttack)의 메서드로 전달.
/// - 현재는 BabyDragon 전용으로 연결되어 있어 확장성이 낮음.
///   => TODO: 인터페이스 기반(IAnimEventReceiver)으로 일반화하면 모든 적에서 재사용 가능.
/// </summary>
public class AnimEventRouter : MonoBehaviour
{

    [SerializeField] BabyDragonDefaultAttack target;

    /// <summary>
    /// 애니메이션 클립 중간에 심는 '발사' 이벤트 콜백.
    /// </summary>
    public void AE_Fire()
    {
        if (target != null) target.BabyDragonFire();
    }


    /// <summary>
    /// 애니메이션 마지막에 심는 '콤보 종료' 이벤트 콜백.
    /// - 조준 회전을 조금 더 유지했다가 끊는 로직을 실행.
    /// </summary>
    public void AE_ComboEnd()
    {
        if (target != null) target.AE_ComboEnd();

    }
}
