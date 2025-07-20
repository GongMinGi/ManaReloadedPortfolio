using System.Collections.Generic;

/// <summary>
/// 개발자: 이예린
/// 
/// 범용 상태 머신의 기본 구조를 정의한 클래스
/// 
/// TState는 상태를 구분하기 위한 열거형(enum) 또는 식별자 타입으로 사용됨
/// 상태 전환 및 업데이트를 관리하며, 각 상태는 BaseState<TState>를 상속한 개별 클래스에서 구현
/// </summary>
/// <typeparam name="TState">상태를 식별하기 위한 열거형(enum) 또는 타입</typeparam>
public class BaseStateMachine<TState>
{
    // 현재 활성화된 상태
    protected BaseState<TState> currentState;

    // 상태 타입(TState)과 실제 상태 인스턴스를 매핑한 딕셔너리
    protected Dictionary<TState, BaseState<TState>> stateDict = new();

    /// <summary>
    /// 상태 전환 메서드
    /// 
    /// 기존 상태에서 OnStateExit을 호출한 뒤,
    /// 새로운 상태를 currentState로 설정하고 OnStateEnter를 호출
    /// </summary>
    /// <param name="newState"></param>
    public virtual void ChageState(TState newState)
    {
        currentState?.OnStateExit();
        currentState = stateDict[newState];
        currentState.OnStateEnter();
    }

    /// <summary>
    /// 현재 상태의 업데이트 로직을 호출하는 메서드
    /// 매 프레임 호출하는 Update 루프에서 실행 가능
    /// </summary>
    public void Update()
    {
        currentState?.OnStateUpdate();
    }
}
