namespace Game.Combat.Stats
{
    /// <summary>
    ///  개발자: 이예린
    ///  
    /// 이동 속도를 감소시키고 주기적으로 피해를 주는 상태이상 효과를 정의함.
    /// </summary>
    public class SlowEffect : StatusEffect
    {
        private StatModifier slowModifier;      // 이동 속도 감소 효과를 정의함

        /// <summary>
        /// SlowEffect 초기화
        /// </summary>
        /// <param name="duration">효과 지속 시간</param>
        /// <param name="hasTickEffect">틱 효과 여부</param>
        /// <param name="effectId">효과 ID</param>
        /// <param name="slowAmount">이동 속도 감소량</param>
        /// <param name="mode">감소 모드</param>
        public SlowEffect(
            float duration,
            bool hasTickEffect,
            int effectId,
            float slowAmount, 
            ModifierMode mode = ModifierMode.Add) : base(duration, hasTickEffect, effectId)
        {
            // 이동 속도 감소 수정자를 생성
            slowModifier = new StatModifier(StatType.MoveSpeed, slowAmount, duration, mode);
        }

        /// <summary>
        /// 효과가 적용될 때 호출됨.
        /// </summary>
        /// <param name="unitStats">대상 유닛의 스탯</param>
        public override void OnApply(UnitStats unitStats)
        {
            // 대상 유닛에 이동 속도 감소 효과를 추가
            unitStats.AddModifier(slowModifier);
        }

        /// <summary>
        /// 효과가 만료될 때 호출됨.
        /// </summary>
        /// <param name="unitStats">대상 유닛의 스탯</param>
        public override void OnExpire(UnitStats unitStats)
        {
            // 대상 유닛에서 이동 속도 감소 효과를 제거
            unitStats.RemoveModifier(slowModifier);
        }
    }
}
