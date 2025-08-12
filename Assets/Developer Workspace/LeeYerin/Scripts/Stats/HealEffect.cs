namespace Game.Combat.Stats
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 체력 회복 상태 효과 클래스
    /// 일정 시간 동안 체력을 회복시키는 효과를 유닛에 부여
    /// </summary>
    public class HealEffect : StatusEffect
    {
        private StatModifier healModifier;      // 체력 회복 효과를 정의함

        /// <summary>
        /// HealEffect 초기화
        /// </summary>
        /// <param name="duration">효과 지속 시간(초)</param>
        /// <param name="hasTickEffect">틱 단위 효과 여부</param>
        /// <param name="effectId">효과 고유 ID</param>
        /// <param name="slowAmount">회복량 (현재는 MoveSpeed 타입으로 설정됨, 추후 수정 필요)</param>
        /// <param name="mode">수정자 적용 모드 (기본값: Add)</param>
        public HealEffect(float duration,
                bool hasTickEffect,
                int effectId,
                float slowAmount,
                ModifierMode mode = ModifierMode.Add) : base(duration, hasTickEffect, effectId)
        {
            // 체력 회복 수정자를 생성
            healModifier = new StatModifier(StatType.MoveSpeed, slowAmount, duration, mode);
        }

        /// <summary>
        /// 효과가 유닛에 적용될 때 호출됨
        /// </summary>
        /// <param name="unitStats">적용 대상 유닛의 스탯</param>
        public override void OnApply(UnitStats unitStats)
        {
            if (healModifier.Mode == ModifierMode.TickOnly) return;

            // 대상 유닛에 체력 회복 효과를 추가
            unitStats.AddModifier(healModifier);
        }

        public override void OnExpire(UnitStats unitStats) { }
    }
}