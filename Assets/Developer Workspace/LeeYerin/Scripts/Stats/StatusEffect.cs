using System;
using System.Collections.Generic;

namespace Game.Combat.Stats
{
    /// <summary>
    /// 개발자: 이예린
    /// 
    /// 상태 효과의 기본 구조를 정의하는 추상 클래스
    /// 지속 시간, 틱(Tick) 처리, 적용/만료 로직을 공통으로 관리함
    /// </summary>
    public abstract class StatusEffect
    {
        #region Tick-related Variables and Properties
        public int EffectId { get; set; }   // 상태 효과의 고유 ID
        public float Duration { get; set; } // 상태 효과의 총 지속 시간을 저장
        public float TickInterval { get; set; } = 1.0f;  // 틱 주기(초 단위)를 저장함. 기본값은 1초

        private float tickTimer = 0f;           // 틱 타이머를 관리
        private Action<UnitStats> tickAction;    // 틱마다 실행할 로직을 델리게이트로 저장

        /// <summary>
        /// 중복 등록을 방지하기 위해 현재 등록된 틱 효과 델리게이트를 관리하는 컬렉션입니다.
        /// HashSet을 사용하여 빠른 검색 및 중복 체크를 수행합니다.
        /// </summary>
        private HashSet<Action<UnitStats>> registeredTickActions = new();

        public bool HasTickEffect { get; set; }  // 틱 효과 유무
        #endregion

        /// <summary>
        /// 생성자: 상태 효과 지속 시간, 틱 여부, ID를 초기화
        /// </summary>
        /// <param name="duration">지속 시간(초). -1이면 무한 지속</param>
        /// <param name="hasTickEffect">틱 효과가 있는지 여부를 설정</param>
        /// <param name="effectId">상태 효과의 고유 ID를 설정</param>
        protected StatusEffect(float duration, bool hasTickEffect, int effectId)
        {
            Duration = duration;    // 지속 시간을 설정
            HasTickEffect = hasTickEffect;  // 틱 효과 여부를 설정
            EffectId = effectId;    // 효과 ID를 설정
        }

        /// <summary>
        /// 상태 효과를 적용할 때 호출
        /// UnitStats에 필요한 Modifier 추가 등 처리
        /// </summary>
        public abstract void OnApply(UnitStats unitStats);

        /// <summary>
        /// 상태 효과가 만료되거나 제거될 때 호출
        /// UnitStats에서 Modifier 제거 등 처리
        /// </summary>
        public abstract void OnExpire(UnitStats unitStats);

        /// <summary>
        /// 틱마다 호출되는 로직을 실행하는 메서드
        /// TickAction이 설정되어 있으면 해당 델리게이트를 호출함
        /// </summary>
        public virtual void OnTick(UnitStats unitStats) => tickAction?.Invoke(unitStats);

        /// <summary>
        /// 틱 효과용 델리게이트를 중복 없이 등록하는 메서드
        /// </summary>
        /// <param name="tickAction">등록할 틱 효과 델리게이트 (UnitStats를 매개변수로 받음)</param>
        public void RegisterTickAction(Action<UnitStats> tickAction)
        {
            // 이미 등록된 델리게이트가 아니라면
            if (!registeredTickActions.Contains(tickAction))
            {
                // 틱 액션 델리게이트에 추가하고
                this.tickAction += tickAction;
                // 등록된 델리게이트 목록에도 추가하여 중복 방지
                registeredTickActions.Add(tickAction);
            }
        }

        /// <summary>
        /// 매 프레임마다 호출하여 틱 타이머를 관리하는 메서드
        /// TickInterval이 0 이하이면 틱 처리를 하지 않음
        /// </summary>
        /// <param name="deltaTime">프레임 경과 시간(초)</param>
        /// <param name="unitStats">대상 유닛 스탯</param>
        public void UpdateTick(float deltaTime, UnitStats unitStats)
        {
            if (TickInterval <= 0) return;  // Tick 불필요한 경우 처리

            tickTimer += deltaTime;     // 타이머를 증가

            // 누적된 시간이 틱 간격 이상일 때 한 번만 틱 실행
            if (tickTimer >= TickInterval)
            {
                tickTimer -= TickInterval;  // 틱 간격만큼 타이머 차감
                OnTick(unitStats);          // 틱 로직 실행
            }
        }

        /// <summary>
        /// 상태 효과 갱신(재적용) 시 호출
        /// 기본 구현은 Duration 갱신
        /// 필요하면 파생 클래스에서 오버라이드 가능
        /// </summary>
        /// <param name="newDuration"></param>
        public virtual void Refresh() { }
    }
}
