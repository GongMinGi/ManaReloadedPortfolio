using UnityEngine;

namespace Game.Combat.Stats
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 스탯에 적용되는 변형(Modifier)을 나타내는 클래스
    /// 각 Modifier는 특정 스탯 타입에 대해 일정 기간 동안 (무한 지속 가능)
    /// 덧셈, 곱셈, 덮어쓰기 방식으로 스탯 값을 변경할 수 있음
    /// </summary>
    public class StatModifier
    {
        [Tooltip("Stat type to be applied")]
        [SerializeField] StatType type;     // 적용 대상 스탯 타입
        [Tooltip("Value applied by Modifier")]
        [SerializeField] float value;       // Modifier가 적용하는 값
        [Tooltip("Modifier applied mode (Add, Multiply, Override)")]
        [SerializeField] ModifierMode mode; // Modifier 적용 방식
        [Tooltip("Modifier duration in seconds")]
        [SerializeField] float duration;    // Modifier 지속 시간 (초 단위) // -1이면 무한 지속

        /// <summary>
        /// 적용 대상 스탯 타입
        /// </summary>
        public StatType Type => type;
        /// <summary>
        /// Modifier가 적용하는 값
        /// </summary>
        public float Value => value;
        /// <summary>
        /// Modifier 적용 방식 (Add, Multiply, Override)
        /// </summary>
        public ModifierMode Mode => mode;
        /// <summary>
        /// Modifier 지속 시간 (초 단위)
        /// -1이면 무한 지속
        /// </summary>
        public float Duration => duration;

        /// <summary>
        /// StatModifier 생성자
        /// </summary>
        /// <param name="type">적용할 스탯 타입</param>
        /// <param name="value">>Modifier 값</param>
        /// <param name="duration">지속 시간 (초), -1이면 무한 지속</param>
        /// <param name="mode">적용 방식, 기본값은 Add</param>
        public StatModifier(StatType type, float value, float duration, ModifierMode mode = ModifierMode.Add)
        {
            this.type = type;
            this.value = value;
            this.mode = mode;
            this.duration = duration;
        }
    }

    /// <summary>
    /// 스탯 종류를 정의하는 열거형
    /// </summary>
    public enum StatType
    {
        MoveSpeed,  // 이동 속도
        Defense,    // 방어력
    }

    /// <summary>
    /// Modifier가 스탯에 적용되는 방식을 정의하는 열거형
    /// </summary>
    public enum ModifierMode
    {
        Add,        // 스탯에 값을 더함
        Multiply,   // 스탯에 값을 곱함
        Override    // 스탯 값을 특정 값으로 덮어씀
    }
}