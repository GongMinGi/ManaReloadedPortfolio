using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] UnitStatsData statsData;      // 기본 스탯 데이터 참조 (ScriptableObject)
        public UnitStatsData StatsData => statsData;

        [SerializeField] private float hp;           // 현재 적 체력
        [SerializeField] private float moveSpeed;    // 현재 이동 속도 값
        private float defense;      // 현재 방어력

        // 스탯 값이 변경되어 다시 계산이 필요할 때 true
        private bool moveSpeedDirty = false;
        private bool defenseDirty = false;

        /// <summary>
        /// 현재 체력 
        /// 0 미만으로, 최대 체력(BaseHP)을 넘지 않도록 제한
        /// </summary>
        public float HP
        {
            get => hp;
            set => hp = Mathf.Min(Mathf.Max(0, value), statsData.BaseHP);
        }

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
        // 현재 적용 중인 스탯 Modifier 리스트
        private List<StatModifier> modifiers = new();
        // 제거 대기 상태인 Modifier 리스트
        private List<StatModifier> removeModifiers = new();

        /// <summary>
        /// 새로운 Modifier 추가 후 Dirty 플래그 설정
        /// </summary>
        public void AddModifier(StatModifier mod)
        {
            if (mod.Mode == ModifierMode.TickOnly) return;

            modifiers.Add(mod);
            MarkDirty(mod.Type);
        }

        /// <summary>
        /// Modifier 제거 요청 
        /// 
        /// 즉시 제거하지 않고 Dirty 플래그 설정 후 처리
        /// </summary>
        public void RemoveModifier(StatModifier mod)
        {
            if (mod.Mode == ModifierMode.TickOnly) return;

            MarkDirty(mod.Type, false);
        }
        #endregion

        #region Status Effect
        // 상태 효과 관리 클래스 참조
        [SerializeField] private StatusEffectHandler effectHandler;
        /// <summary>
        /// 상태 효과 관리 클래스
        /// </summary>
        public StatusEffectHandler EffectHandler => effectHandler;

        // 스탯 적용 시 실행할 추가 로직 저장
        private Dictionary<StatType, Action> statApplyHandlers = new();
        // 스탯 해제 시 실행할 추가 로직 저장
        private Dictionary<StatType, Action> statRevertHandlers = new();

        /// <summary>
        /// 스탯 적용 시 실행할 추가 로직 저장
        /// </summary>
        public Dictionary<StatType, Action> StatApplyHandlers => statApplyHandlers;
        /// <summary>
        /// 스탯 해제 시 실행할 추가 로직 저장
        /// </summary>
        public Dictionary<StatType, Action> StatRevertHandlers => statRevertHandlers;
        #endregion

        #region Unity Event
        private void Awake()
        {
            // 기본 스탯 데이터에서 값 할당
            HP = statsData.BaseHP;
            MoveSpeed = statsData.BaseMoveSpeed;
            Defense = statsData.BaseDefense;

            // StatusEffectHandler에 자신 참조 전달
            effectHandler.UnitStats = this;
        }
        #endregion

        #region Stat Getters
        /// <summary>
        /// 지정된 데미지만큼 체력 감소
        /// </summary>
        /// <param name="damage">데미지</param>
        public void TakeDamage(float damage)
        {
            HP -= damage;
        }

        /// <summary>
        /// 현재 이동 속도 반환 (Dirty 상태면 Modifier 적용 후 갱신)
        /// </summary>
        /// <param name="apply">true면 Modifier를 적용, false면 Modifier 해제</param>
        /// <returns>Modifier가 적용된 최종 이동 속도 값</returns>
        public float GetMoveSpeed(bool apply = true)
        {
            if (moveSpeedDirty)
            {
                // Modifier 적용 또는 해제에 따라 값 계산
                float value = apply ? 
                    ApplyModifiers(moveSpeed, StatType.MoveSpeed) : 
                    RevertModifiers(moveSpeed, StatType.MoveSpeed);

                // 유효한 값이면 MoveSpeed 갱신
                if (value != -1) MoveSpeed = value;

                // Dirty 상태 해제
                moveSpeedDirty = false;
            }

            return moveSpeed;
        }

        /// <summary>
        /// 현재 방어력 반환 (Dirty 상태면 Modifier 적용 후 갱신)
        /// </summary>
        /// <param name="apply">true면 Modifier를 적용, false면 Modifier 해제</param>
        /// <returns>Modifier가 적용된 최종 방어력 값</returns>
        public float GetDefense(bool apply = true)
        {
            if (defenseDirty)
            {
                // Modifier 적용 또는 해제에 따라 값 계산
                float value = apply ?
                    ApplyModifiers(defense, StatType.Defense) :
                    RevertModifiers(defense, StatType.Defense);

                // 유효한 값이면 Defense 갱신
                if (value != -1) Defense = value;

                // Dirty 상태 해제
                defenseDirty = false;
            }

            return defense;
        }
        #endregion

        #region Modifier Application and Reversion
        /// <summary>
        /// 기본 값에 대해 해당 스탯 타입에 적용되는 모든 Modifier를 처리하여 최종 값을 반환
        /// - Add: 값을 더함
        /// - Multiply: 곱셈 방식으로 적용
        /// - Override: 값을 덮어씀 (가장 우선순위가 높음)
        /// 최종 결과는 0 미만이 되지 않도록 보장
        /// </summary>
        /// <param name="baseValue">Modifier가 적용될 기본 스탯 값</param>
        /// <param name="statType">적용 대상인 스탯 타입</param>
        /// <returns>Modifier가 적용된 최종 스탯 값 (0 이상)</returns>
        private float ApplyModifiers (float baseValue, StatType statType)
        {
            float finalValue = baseValue;
            float multiplier = 1f;

            foreach (var mod in modifiers)
            {
                // 현재 스탯 타입에 해당하는 Modifier만 처리
                if (mod.Type != statType) continue;

                switch (mod.Mode)
                {
                    case ModifierMode.Add:
                        finalValue += mod.Value;    // 덧셈 적용
                        break;

                    case ModifierMode.Multiply:
                        multiplier *= mod.Value;     // 곱셈 누적
                        break;

                    case ModifierMode.Override:
                        // 우선순위 최고, 값 덮어쓰기 및 곱셈 초기화
                        finalValue = mod.Value;
                        multiplier = 1f;
                        break;
                }
            }

            // 0 미만 방지 후 최종 값 반환
            return Mathf.Max(0, finalValue * multiplier);
        }

        /// <summary>
        /// Modifier 해제 로직 (Add, Multiply만 되돌림 / Override는 -1 반환)
        /// </summary>
        /// <param name="baseValue">Modifier가 해제될 기본 스탯 값</param>
        /// <param name="statType">적용 대상인 스탯 타입</param>
        /// <returns>Modifier가 적용된 최종 스탯 값 (0 이상)</returns>
        private float RevertModifiers(float baseValue, StatType statType)
        {
            float finalValue = baseValue;
            float multiplier = 1f;

            foreach (var mod in modifiers)
            {
                // 현재 스탯 타입에 해당하는 Modifier만 처리
                if (mod.Type != statType) continue;

                switch (mod.Mode)
                {
                    case ModifierMode.Add:
                        finalValue -= mod.Value;    // 덧셈 효과 되돌리기
                        break;

                    case ModifierMode.Multiply:
                        multiplier /= mod.Value;    // 곱셈 효과 되돌리기
                        break;

                    case ModifierMode.Override:
                        // Override는 되돌릴 수 없으므로 -1 반환하여 무시
                        return -1;
                }

                // 되돌린 Modifier는 제거 리스트에 추가
                removeModifiers.Add(mod);
            }

            // 제거 대상 Modifier 모두 삭제 후 리스트 초기화
            foreach (var mod in removeModifiers)
                modifiers.Remove(mod);

            removeModifiers.Clear();

            // 0 미만 방지 후 최종 값 반환
            return Mathf.Max(0, finalValue * multiplier);
        }

        /// <summary>
        /// 해당 스탯 타입의 Dirty 플래그 설정 및 관련 핸들러 실행
        /// </summary>
        /// <param name="type">Dirty 플래그 설정할 스탯 타입</param>
        /// <param name="apply">true면 Modifier를 적용, false면 Modifier 해제</param>
        private void MarkDirty(StatType type, bool apply = true)
        {
            // Dirty 플래그 설정
            switch (type)
            {
                case StatType.MoveSpeed:
                    moveSpeedDirty = true;
                    break;
                case StatType.Defense:
                    defenseDirty = true;
                    break;
            }

            // 해당 동작에 맞는 핸들러 호출
            Action handler;

            if (apply)
                statApplyHandlers.TryGetValue(type, out handler);
            else
                statRevertHandlers.TryGetValue(type, out handler);

            handler?.Invoke();
        }
        #endregion
    }
}