using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Stats
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 유닛의 현재 스탯을 관리하는 클래스
    /// 기본 스탯은 UnitStatsData에서 불러오며, 
    /// 스탯 변경을 위한 다양한 Modifier를 적용할 수 있는 확장 가능한 구조를 제공
    /// 
    /// Modifier를 통해 추가, 곱셈, 덮어쓰기 방식으로 동적으로 변화를 적용
    /// </summary>
    public class UnitStats : MonoBehaviour
    {
        #region Base Stats
        [Tooltip("Default staff data (UnitStatsData)")]
        [SerializeField] UnitStatsData enemyStatsData;      // 기본 스탯 데이터 참조 (ScriptableObject)

        private float moveSpeed;    // 현재 이동 속도 값
        private float defense;      // 현재 방어력

        /// <summary>
        /// 이동 속도 프로퍼티
        /// 0 미만으로 내려가지 않도록 제한
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0, value);
        }

        /// <summary>
        /// 방어력 프로퍼티
        /// 0 미만으로 내려가지 않도록 제한
        /// </summary>
        public float Defense
        {
            get => defense;
            set => defense = Mathf.Max(0, value);
        }
        #endregion

        #region StatModifier
        private List<StatModifier> modifiers = new();   // 스탯 변화에 영향을 주는 Modifier 리스트

        /// <summary>
        /// Modifier 추가
        /// </summary>
        public void AddModifier(StatModifier mod) => modifiers.Add(mod);
        /// <summary>
        /// Modifier 제거
        /// </summary>
        public void RemoveModifier(StatModifier mod) => modifiers.Remove(mod);
        #endregion

        #region Unity Event
        private void Awake()
        {
            // 기본 스탯 데이터에서 값 할당
            MoveSpeed = enemyStatsData.BaseMoveSpeed;
            Defense = enemyStatsData.BaseDefense;
        }
        #endregion

        #region Stat Getters
        /// <summary>
        /// 현재 이동 속도 값에 Modifier를 적용한 최종 값을 반환
        /// </summary>
        public float GetMoveSpeed()
        {
            return ApplyModifiers(moveSpeed, StatType.MoveSpeed);
        }

        /// <summary>
        /// 현재 방어력에 Modifier를 적용한 최종 값을 반환
        /// </summary>
        public float GetDefense()
        {
            return ApplyModifiers(defense, StatType.Defense);
        }
        #endregion

        /// <summary>
        /// 기본 값에 대해 해당 스탯 타입에 적용되는 모든 Modifier를 처리하여 최종 값을 반환
        /// - Add: 값을 더함
        /// - Multiply: 곱셈 방식으로 적용
        /// - Override: 값을 덮어씀 (가장 우선순위가 높음)
        /// 최종 결과는 0 미만이 되지 않도록 보장
        /// </summary>
        private float ApplyModifiers (float baseValue, StatType statType)
        {
            float finalValue = baseValue;
            float multiplier = 1f;

            foreach (var mod in modifiers)
            {
                if (mod.Type != statType) continue;

                switch (mod.Mode)
                {
                    case ModifierMode.Add:
                        finalValue += mod.Value;
                        break;

                    case ModifierMode.Multiply:
                        multiplier *= mod.Value;
                        break;

                    case ModifierMode.Override:
                        finalValue = mod.Value;
                        multiplier = 1f;
                        break;
                }
            }

            return Mathf.Max(0, finalValue * multiplier);
        }
    }
}