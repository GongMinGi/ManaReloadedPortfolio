using DG.Tweening;
using Game.Combat.Stats;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 작성자 : 이예린
/// - ProjectileParams를 상속받아 Thorn 전용 매개변수 추가
/// </summary>
public class ThornParams : ProjectileParams
{
    public float RadiusSqr => radius * radius;       // 범위의 제곱 값 (거리 계산 최적화용)
    public float lifeTime;                           // 가시밭 지속 시간
    public float coolTime;                           // 가시밭 쿨타임 (현재 미사용)
    public float duration;                           // 슬로우 지속 시간
    public float value;                              // 슬로우 강도 (곱연산)
    public float tickInterval;                       // 피해 및 슬로우 틱 간격
    public int effectId;                             // 슬로우 효과 식별자
}

/// <summary>
/// 개발자: 이예린
/// 
/// Thorn 오브젝트 풀링용 클래스
/// 
/// - 범위 내 적들에게 지속적인 슬로우 효과 및 주기적 피해를 적용함
/// - PooledObject를 상속받아 활성화 시 자동으로 스킬 동작 시작
/// </summary>
public class ThornPooledObject : AbstractProjectile
{
    private ThornParams thornParams;

    private float elapsedLifetime;              // 스킬 발동 후 경과한 총 시간
    private float intervalTimer;                // 틱 효과 실행 간격을 추적하는 타이머
    private bool isEffectActive;                // 스킬 효과가 현재 활성 상태인지 여부

    private SlowEffect effect;                   // 슬로우 상태 효과 객체

    Sequence seq;

    #region Unity Event
    private void Update()
    {
        if (isEffectActive == false)
        {
            return;
        }

        elapsedLifetime += Time.deltaTime;
        intervalTimer += Time.deltaTime;

        if (intervalTimer >= thornParams.tickInterval)
        {
            ApplyDebuffInRange(transform.position);
            intervalTimer -= thornParams.tickInterval;
        }

        if (elapsedLifetime >= thornParams.lifeTime)
        {
            isEffectActive = false;
            elapsedLifetime = 0f;

            Release();
        }
    }
    #endregion

    public override void Setup(ProjectileParams p)
    {
        if (p is ThornParams == false)
        {
            Debug.LogError("ThornPooledObject: ThornParams 타입이 필요합니다!");
            return;
        }

        // ThornParams 값 세팅
        thornParams = p as ThornParams;

        // 타이머 세팅
        elapsedLifetime = 0f;
        intervalTimer = thornParams.tickInterval;

        // 슬로우 효과 초기화
        if (effect == null)
        {
            effect = new SlowEffect(thornParams.duration, true, thornParams.effectId, thornParams.value, ModifierMode.Multiply);
        }
        else
        {
            effect.UpdateEffect(thornParams.duration, thornParams.value);
        }

        effect.TickInterval = thornParams.tickInterval;
    }

    #region Hook
    protected override IEnumerator OnActivated()
    {
        seq = DOTween.Sequence();
        seq.Join(transform.DOMoveY(transform.position.y + 1f, 0.3f));   // 위로 올라오는 애니메이션 실행

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
    #endregion

    private void ApplyDebuffInRange(Vector3 center)
    {
        var enemies = GameModeManager.EnemyManager.Enemies;

        // 현재 존재하는 모든 적을 순회하며 범위 내 여부 판단
        for (int i = 0; i < enemies.Count; i++)
        {
            float distanceSqr = (enemies[i].transform.position - center).sqrMagnitude;

            if (distanceSqr <= thornParams.RadiusSqr)
            {
                // 범위 내 적에게 슬로우 상태효과 추가
                enemies[i].Stats.EffectHandler.AddStatusEffect(effect);

                // 틱마다 피해를 입힘
                enemies[i].Stats.TakeDamage(thornParams.damage);
            }
        }
    }
}