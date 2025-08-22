using Game.Combat.Stats;
using System.Collections;
using UnityEngine;
using DG.Tweening;
using System;

/// <summary>
/// 개발자: 이예린
/// 
/// Thorn 오브젝트 풀링용 클래스
/// 
/// - 범위 내 적들에게 지속적인 슬로우 효과 및 주기적 피해를 적용함
/// - PooledObject를 상속받아 활성화 시 자동으로 스킬 동작 시작
/// </summary>
public class ThornPooledObject : PooledObject
{
    [SerializeField] float range;               // 효과 적용 범위 반경
    private float radiusSqr;                     // 범위의 제곱 값 (거리 계산 최적화용)

    [SerializeField] int effectId;              // 슬로우 효과 식별자
    [SerializeField] float lifeTime;            // Thorn 오브젝트가 활성화되어 있는 시간
    [SerializeField] float coolTime;            // (현재 미사용, 추후 스킬 재사용 대기 시간으로 활용 가능)
    [SerializeField] float duration;            // 슬로우 효과 지속 시간
    [SerializeField] float value;               // 슬로우 효과 수치 (곱셈 방식)
    [SerializeField] float tickInterval;        // 슬로우 효과 틱 간격
    [SerializeField] float damage;              // 슬로우 효과 틱당 피해량

    private SlowEffect effect;                   // 슬로우 상태 효과 객체

    Sequence seq;

    /// <summary>
    /// 효과 적용 범위 반경
    /// </summary>
    public float Range
    {
        get { return range; }
        set
        {
            range = value;
            radiusSqr = range * range;      // 거리 비교용 제곱값 갱신
        }
    }

    #region Unity Event
    private void Awake()
    {
        radiusSqr = range * range;  // 범위 제곱값 초기화
        // 슬로우 효과 초기화 (지속시간, 중첩 허용, 효과 ID, 수치, 곱셈 모드)
        effect = new SlowEffect(duration, true, effectId, value, ModifierMode.Multiply);
        // 틱 간격 설정
        effect.TickInterval = tickInterval;
    }
    #endregion

    protected override IEnumerator OnActivated()
    {
        seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(transform.position.y + 1f, 0.3f));   // 위로 올라오는 애니메이션 실행

        // 오브젝트 활성화 시 스킬 동작 코루틴 시작
        StartCoroutine(SkillCoroutine());
        yield break;
    }

    protected override void OnDeactivated(Action onComplete = null)
    {
        seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(transform.position.y - 1f, 0.3f))    // 아래로 내려가는 애니메이션 실행
            .OnComplete(() =>
            {
                onComplete?.Invoke();  // 시퀀스 끝났을 때 반납 로직 호출
            });
    }

    private IEnumerator SkillCoroutine()
    {
        float time = 0f;

        // lifeTime 동안 1초 간격으로 범위 내 적에게 디버프 적용
        while (time < lifeTime)
        {
            ApplyDebuffInRange(transform.position);

            yield return new WaitForSeconds(1f);
            time += 1f;
        }
    }

    private void ApplyDebuffInRange(Vector3 center)
    {
        // 현재 존재하는 모든 적을 순회하며 범위 내 여부 판단
        foreach (var enemy in GameModeManager.EnemyManager.Enemies)
        {
            float distanceSqr = (enemy.transform.position - center).sqrMagnitude;

            if (distanceSqr <= radiusSqr)
            {
                // 범위 내 적에게 슬로우 상태효과 추가
                enemy.Stats.EffectHandler.AddStatusEffect(effect);

                // 틱마다 피해를 입히는 델리게이트 등록
                effect.RegisterTickAction(ApplyTickDamage);
            }
        }
    }

    // 슬로우 틱마다 호출되어 대상에게 피해를 입힘
    private void ApplyTickDamage(UnitStats unitStats) => unitStats.TakeDamage(damage);
}
