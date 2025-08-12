using Game.Combat.Stats;
using System.Collections;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// LifeLight 오브젝트 풀링용 클래스
/// 플레이어 위치에 지속적으로 위치하며, 일정 시간 동안 체력 회복 효과를 부여함
/// </summary>
public class LifeLightPooledObject : PooledObject
{
    [SerializeField] int effectId;              // 체력 회복 효과 식별자
    [SerializeField] float lifeTime;            // LifeLight 오브젝트가 활성화되어 있는 시간
    [SerializeField] float duration;            // 체력 회복 효과 지속 시간
    [SerializeField] float value;               // 체력 회복 효과 수치 (Tick Only 방식)
    [SerializeField] float tickInterval;        // 효과 틱 간격

    [SerializeField] float healingRatio;        // 체력 회복 비율 (기준: 최대 체력 대비)

    float healPerTick;                          // 틱마다 회복할 체력량

    private HealEffect effect;                  // 체력 회복 상태 효과 객체

    #region unity Event
    private void Awake()
    {
        // HealEffect 객체 초기화 (지속시간, 틱 단위 효과 여부, 효과 ID, 수치, TickOnly 모드)
        effect = new HealEffect(duration, true, effectId, value, ModifierMode.TickOnly);
        // 틱 간격 설정
        effect.TickInterval = tickInterval;
        healPerTick = -1f;      // 초기값 세팅 (계산 전 상태)
    }

    private void Update()
    {
        // 오브젝트 위치를 항상 플레이어 위치로 동기화
        transform.position = GameModeManager.Player.transform.position;
    }
    #endregion

    protected override IEnumerator OnActivated()
    {
        ApplyDebuffToPlayer();
        yield break;
    }

    /// <summary>
    /// 플레이어에게 체력 회복 효과를 추가하고 틱 단위 회복 함수를 등록
    /// </summary>
    private void ApplyDebuffToPlayer()
    {
        // 플레이어에게 체력 회복 효과 추가
        GameModeManager.Player.Stats.EffectHandler.AddStatusEffect(effect);

        // 틱마다 체력 회복 효과 델리게이트 등록
        effect.RegisterTickAction(ApplyTickHeal);
    }

    /// <summary>
    /// 틱 단위로 체력을 회복시키는 함수
    /// </summary>
    /// <param name="unitStats">회복 대상 유닛의 스탯</param>
    private void ApplyTickHeal(UnitStats unitStats)
    {
        // healPerTick 값이 초기 상태라면 최대 체력과 힐링 비율 기반으로 계산
        if (healPerTick < 0f)
            healPerTick = unitStats.StatsData.BaseHP * healingRatio / (duration / tickInterval);

        // 계산된 틱당 회복량만큼 체력 증가
        unitStats.HP += healPerTick;
    }
}
