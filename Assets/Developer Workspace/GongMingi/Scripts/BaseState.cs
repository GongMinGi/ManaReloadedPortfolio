using System.Threading;
using UnityEngine;

public abstract class BaseState
{
    //protected Monster _monster;

    //protected BaseState(Monster monster)
    //{
    //    _monster = monster;
    //}

    public abstract void OnStateEnter();
    public abstract void OnStateUpdate();
    public abstract void OnStateExit();
}
