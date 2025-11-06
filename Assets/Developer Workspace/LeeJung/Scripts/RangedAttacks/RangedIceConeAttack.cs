using Game.Combat.Stats;
using UnityEngine;
/// <summary>
/// 개발자: 이정
/// 
/// 냉기 원거리 공격 클래스
/// </summary>
public class RangedIceConeAttack : RangedConeAttack
{
    [Header("Ice Parameter")]
    [SerializeField] private float slowDuration = 1f;        // 느려지는 지속 시간
    [SerializeField] private float slowValue = 0.7f;         // 느려지는 정도(배율)
    [SerializeField] private int effectId = 2000009;           // 상태이상 효과 아이디

    override protected void Awake()
    {
        base.Awake();
        // 슬로우 효과 초기화
        if (effect == null)
        {
            effect = new SlowEffect(slowDuration, true, effectId, slowValue, ModifierMode.Multiply);
        }
    }
}
