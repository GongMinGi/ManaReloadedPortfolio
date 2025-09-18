using DG.Tweening;
using Game.Combat.Stats;
using System;
using System.Collections;
using UnityEngine;

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
    [Header("Skill Data Setting")]
    [SerializeField] float range;               // 효과 적용 범위 반경
    private float radiusSqr;                     // 범위의 제곱 값 (거리 계산 최적화용)

    [SerializeField] int effectId;              // 슬로우 효과 식별자
    [SerializeField] float lifeTime;            // Thorn 오브젝트가 활성화되어 있는 시간
    [SerializeField] float coolTime;            // (현재 미사용, 추후 스킬 재사용 대기 시간으로 활용 가능)
    [SerializeField] float duration;            // 슬로우 효과 지속 시간
    [SerializeField] float value;               // 슬로우 효과 수치 (곱셈 방식)
    [SerializeField] float tickInterval;        // 슬로우 효과 틱 간격
    [SerializeField] float damage;              // 슬로우 효과 틱당 피해량

    private float elapsedLifetime;              // 스킬 발동 후 경과한 총 시간
    private float intervalTimer;                // 틱 효과 실행 간격을 추적하는 타이머
    private bool isEffectActive;                // 스킬 효과가 현재 활성 상태인지 여부

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

    private void Update()
    {
        if (isEffectActive == false)
        {
            return;
        }

        elapsedLifetime += Time.deltaTime;
        intervalTimer += Time.deltaTime;

        if (intervalTimer >= tickInterval)
        {
            ApplyDebuffInRange(transform.position);
            intervalTimer -= tickInterval;
        }

        if (elapsedLifetime >= lifeTime)
        {
            isEffectActive = false;
            elapsedLifetime = 0f;

            Release();
        }
    }
    #endregion

    protected override IEnumerator OnActivated()
    {
        seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(transform.position.y + 1f, 0.3f));   // 위로 올라오는 애니메이션 실행

        elapsedLifetime = 0f;
        intervalTimer = tickInterval;
        isEffectActive = true;

        yield break;
    }

    protected override void OnDeactivated(Action onComplete = null)
    {
        isEffectActive = false;
        seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(transform.position.y - 1f, 0.3f))    // 아래로 내려가는 애니메이션 실행
            .OnComplete(() =>
            {
                onComplete?.Invoke();  // 시퀀스 끝났을 때 반납 로직 호출
            });
    }

    private void ApplyDebuffInRange(Vector3 center)
    {
        var enemies = GameModeManager.EnemyManager.Enemies;

        // 현재 존재하는 모든 적을 순회하며 범위 내 여부 판단
        for (int i = 0; i < enemies.Count; i++)
        {
            float distanceSqr = (enemies[i].transform.position - center).sqrMagnitude;

            if (distanceSqr <= radiusSqr)
            {
                // 범위 내 적에게 슬로우 상태효과 추가
                enemies[i].Stats.EffectHandler.AddStatusEffect(effect);

                // 틱마다 피해를 입힘
                enemies[i].Stats.TakeDamage(damage);
            }
        }
    }
}
