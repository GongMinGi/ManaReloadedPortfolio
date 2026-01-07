using System;

public interface IAttackSignal
{
    // 공격 생명주기 시그널
    event Action OnAttackStarted;                               // 공격 시작(차지 시작/ 빔 시작 등)
    event Action<float> OnProgressUpdated;                       // 0~1 진행도(선택) : 차지/ 채널링 
    event Action OnAttackEnded;                                 // 정상 종료 (발사 해제
    event Action OnAttackInterrupted;                           // 강제 취소 (stop(), 혹은 피격 등)

    int? AnimationBoolHash { get; }
    int? AnimationTriggerHash { get; }
    bool UseProgress { get; }
}

/// <summary>
/// * 작성자 : 공민기
///  - 원거리 공격 (빔 / 콘 / 투사체 등)이 상속하는 인터페이스
///  - Player / ElementalRangedAttackController 에서 호출될 때 
///     동일한 메서드로 도작하도록 강제
/// </summary>
public interface IRangedAttack : IAttackSignal
{
    /// <summary>
    /// - 공격 실행
    /// - ElementalRangedAttackController 가 속성 ( E_CastingType)을 결정해 준다.
    /// - 필요 없을 때는 type을 무시해도 ok  
    /// </summary>
    /// <param name="type"></param>
    abstract void ExecuteAttack(E_CastingType type);

    /// <summary>
    /// - 외부에서 공격을 강제 중단하고 싶을 때 호출
    /// - 빔 홀드 취소, 차지 취소, 채널링 캔슬 등에 사용
    /// </summary>
    abstract void Stop();
}
