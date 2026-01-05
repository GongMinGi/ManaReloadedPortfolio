using System;
using UnityEngine;

public interface IAnimationDriver                       // Animator에 직접 의존하지 않고, 필요한 기능만 추상화한 드라이버 인터페이스
{
    void SetBool(int hash, bool value);                
    void SetTrigger(int hash);
    void SetFloat(int hash, float value);
}

/// <summary>
/// 애니메이터를 인터페이스로 감싸기
/// 사용할떄 클래스가 아닌 인터페이스 변수로 만들어 사용
/// => Animator를 Animancer등 다른 클래스로바꿔도 인터페이스의 메서드이름, 파라미터만 똑같이 쓰면 동일하게 작동
/// => 교체가 매우 용이해짐
/// </summary>
public sealed class AnimatorDriver : IAnimationDriver    // 실제 Unity Animator를 감싸는 구체 드라이버 
{
    private Animator _anim;                              // 내부에서 사용할 Animator 참조 ( 외부에 노출하지 않음, 불변( 이건 readonly 붙여야함))
    public AnimatorDriver(Animator anim)                 => _anim = anim;                         // 생성자: Animator 주입 및 보관 
    public void SetBool(int hash, bool value)            => _anim.SetBool(hash, value);           // bool 파라미터 설정(문자열 대신 해시 사용)
    public void SetTrigger(int hash)                     => _anim.SetTrigger(hash);               // trigger 파라미터 발동
    public void SetFloat(int hash, float parameterValue) => _anim.SetFloat(hash, parameterValue); // float 파라미터 설정 
}

/// <summary>
/// 애니메이터에 대한 의존성 제거
/// 유니티 에디터에서 파라미터 이름을 바꿔도 여기 하나만 수정하면 모든 스크립트 변경
/// </summary>
public static class AnimParams                            // Animator 파라미터 이름을 해시로 미리 변환해 캐시하는 유틸 클래스
{
    public static readonly int Beam = Animator.StringToHash("isRangedBeamAttack");                      // bool : 빔 공격 상태 제어
    public static readonly int ChargeProjectile = Animator.StringToHash("isRangedProjectileAttack");    // bool : 차지 투사체 상태 제어
    public static readonly int ChargeCone = Animator.StringToHash("isRangedChargeConeAttack");          // bool : 차지 콘 상태 제어
    public static readonly int HoldConeAttack = Animator.StringToHash("isRangedConeAttack");                  // trigger : 즉발 콘 공격 트리거

    // 차지/ 채널링 진행도 (필요하면)
    public static readonly int RangedProgress = Animator.StringToHash("RangedProgress");                // float : 진행도(0..1) 바인딩용
}

#region legacy
///// <summary>
/////  * 컨텍스트 주입
/////   - 컨트롤러의 WireAttack에서 해당 원거리 공격이 IRequireAttackContext 를 구현했는지 확인한다.
/////   - 만약 구현되어 있다면, BindContext를 통해 필요한 정보(컨텍스트)를 넘겨준다.
/////   - 공격 스크립트가 컨트롤러를 찾아다니지 않고도 플레이어 위치, 원거리 발사위치, 애니메이터를 사용할 수 있다.
/////     => 결합도 감소
///// </summary>
//public interface IRequireAttackContext                  // 공격이 실행 시 활용할 공통 컨텍슽르르 주입받아야 함을 나타내는 인터페이스
//{                                                       
//    void BindContext(RangedAttackContext ctx);          // 컨트롤러 생성한 RangedAttackContext 를 참조하는 메서드 (의존성 주입)
//}

///// <summary>
/////  * 컨텍스트 : 공격 로직이 공통으로 필요로 하는 의존성 묶음
/////   - 플레이어 위치, 공격 발사 위치, 애니메이터 에 대한 정보를 들고 있는 클래스
/////   - 컨트롤러의 Awake에서 한 번만 생성된다.
///// </summary>
//public sealed class RangedAttackContext                 // 공격들이 공유하는 컨텍스트 ( Animator 직접노출 x)
//{
//    public Transform Owner { get; }                     // 공격의 소유자( 플레이어 또는 무기 루트 Transform)
//    public Transform DefaultMuzzle { get; }             // 기본 발사 원점(총구) Transform(없다면 Owner를 그대로 줄 수 있음)
//    public IAnimationDriver Anim { get; }               // Animator를 직접 노출하지 않고, 추상 드라이버로 감싼 애니메이션 제어 핸들

//    public RangedAttackContext(Transform owner, Transform defaultMuzzle, IAnimationDriver anim) // 컨텍스트 생성자
//    {
//        Owner = owner;                                  // 소유자 Transform 저장 (읽기 전용 자동 프로퍼티)
//        DefaultMuzzle = defaultMuzzle;                  // 기본 머즐 Transform 저장 ( 읽기 전용 자동 프로퍼티)
//        Anim = anim;                                    // 애니메이션 제어 드라이버 저장( 읽기 전용 자동 프로퍼티)
//    }
//}

#endregion