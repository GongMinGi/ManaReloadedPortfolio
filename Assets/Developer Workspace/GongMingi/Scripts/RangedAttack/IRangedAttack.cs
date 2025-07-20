using UnityEngine;

public interface IRangedAttack
{

    abstract void ExecuteAttack(E_CastingType type);

    abstract void Stop();

}
