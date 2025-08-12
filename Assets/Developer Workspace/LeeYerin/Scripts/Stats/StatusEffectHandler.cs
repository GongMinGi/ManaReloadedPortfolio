using System.Collections.Generic;
using UnityEngine;

namespace Game.Combat.Stats
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 유닛의 상태 효과를 관리하는 클래스
    /// 상태 효과의 적용, 지속 시간 관리, 틱(Tick) 처리, 만료 처리를 담당함
    /// </summary>
    public class StatusEffectHandler : MonoBehaviour
    {
        private Dictionary<int, StatusEffect> activeEffectMap = new();  // 활성 상태 효과를 ID 기준으로 저장
        private List<int> activeEffects = new();    // 활성 상태 효과 ID 목록
        private Dictionary<int, float> effectDurations = new();     // 각 상태 효과의 남은 지속 시간

        /// <summary>
        /// 이 핸들러가 관리하는 유닛의 스탯
        /// </summary>
        public UnitStats UnitStats { get; set; }

        #region Unity Event
        private void Update()
        {
            // 상태 효과 지속시간 갱신 및 만료 처리
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffectMap[activeEffects[i]];
                effectDurations[effect.EffectId] -= Time.deltaTime;

                // 틱 효과가 있는 경우 주기적으로 실행
                if (effect.HasTickEffect)
                    effect.UpdateTick(Time.deltaTime, UnitStats);

                // 지속 시간이 끝난 경우 종료 처리
                if (effectDurations[effect.EffectId] <= 0)
                {
                    Debug.Log("버프 종료");
                    activeEffects.RemoveAt(i);
                    effectDurations.Remove(effect.EffectId);
                    activeEffectMap.Remove(effect.EffectId);

                    // 효과 종료 처리
                    effect.OnExpire(UnitStats);
                }
            }
        }
        #endregion

        /// <summary>
        /// 새로운 상태 효과를 추가하고 적용하는 메서드
        /// 중복 적용이나 갱신 정책은 여기서 결정
        /// </summary>
        public void AddStatusEffect(StatusEffect newEffect)
        {
            if (activeEffectMap.ContainsKey(newEffect.EffectId))
            {
                // 이미 동일한 효과가 있으면 지속 시간만 갱신
                effectDurations[newEffect.EffectId] = newEffect.Duration;
                return;
            }

            // 무한 지속(-1)이 아니면 목록에 추가
            if (newEffect.Duration != -1)
            {
                activeEffectMap[newEffect.EffectId] = newEffect;
                activeEffects.Add(newEffect.EffectId);
                effectDurations.Add(newEffect.EffectId, newEffect.Duration);
            }
            // 효과 즉시 적용
            newEffect.OnApply(UnitStats);
        }
    }
}
