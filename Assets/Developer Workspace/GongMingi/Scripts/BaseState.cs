using System.Threading;
using UnityEngine;

public abstract class BaseState<TState>
{
    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}
