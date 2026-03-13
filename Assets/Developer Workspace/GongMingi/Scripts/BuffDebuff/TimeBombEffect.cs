using Game.Combat.Stats;
using UnityEngine;

/// <summary>
/// * 작성자 : 공민기
///  - 대상에게 부착된 후, 지정된 시간이 지나면 데미지를 입히는 상태이상 
/// </summary>
public class TimeBombEffect : StatusEffect
{
    private float damageAmount; // 터질 때 입힐 데미지량

    /// <summary>
    /// TimebomeEffect 초기화
    /// </summary>
    /// <param name="duration">폭발까지 걸리는 시간</param>
    /// <param name="effectId">효과 ID</param>
    /// <param name="damageAmount">입힐 데미지</param>
    public TimeBombEffect(float duration, int effectId, float damageAmount) : base(duration, false, effectId)
    {
        this.damageAmount = damageAmount;
        Debug.Log(this.damageAmount);
    }

    public override void OnApply(UnitStats unitStats)
    {
        //TODO: 이팩트 부여 시에 띄울 시각효과나 ui   
    }

    public override void OnExpire(UnitStats unitStats)
    {
        // 시간이 지나 효과가끝나는 순간 데미지를 입힌다.
        unitStats.TakeDamage(damageAmount);
    }
}
