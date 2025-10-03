using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 가시밭 조합 마법 클래스
/// 플레이어 앞 일정 거리 위치에 Thorn(가시밭) 오브젝트를 오브젝트 풀에서 꺼내 배치함
/// 
/// - ThornPooledObject 타입의 오브젝트 풀을 관리하며, 최초 실행 시 풀을 생성함
/// - 플레이어의 현재 위치와 앞 방향을 기준으로 Thorn 생성 위치를 계산함
/// </summary>
public class ThornField : BaseCombinationMagic
{
    [Tooltip("Thorn object prefab to use")]
    [SerializeField] ThornPooledObject thorn;   // 사용할 Thorn 오브젝트 프리팹
    [SerializeField] int size;                  // 풀에 미리 생성할 오브젝트 수
    [SerializeField] int capacity;              // 풀의 최대 수용 가능 오브젝트 수

    [SerializeField] float distance = 1f;        // 플레이어 앞에 배치할 기본 거리

    [Header("Effect Range / Damage")]
    [SerializeField] private float range;         // 효과 적용 범위 반경
    [SerializeField] private float damage;        // 틱마다 입히는 피해량 (일정 주기로 가해짐)

    [Header("Slow Effect Settings")]
    [SerializeField] private int effectId;        // 슬로우 효과 ID (EffectHandler에서 구분용)
    [SerializeField] private float duration;      // 슬로우 효과 지속 시간
    [SerializeField] private float value;         // 슬로우 강도 (곱연산 비율, ex. 0.8f → 20% 슬로우)

    [Header("Lifetime & Tick Settings")]
    [SerializeField] private float lifeTime;      // 전체 지속 시간 (가시밭이 사라지기 전까지)
    [SerializeField] private float tickInterval;  // 틱 주기 (ex. 0.5초마다 데미지 & 슬로우 재적용)

    PlayerController player = null;             // 플레이어 컨트롤러 참조 (초기화 전 null)

    public override void ExecuteSkill()
    {
        // player가 null이면, 오브젝트 풀을 생성하고 플레이어 참조를 가져옴
        if (player == null)
        {
            GameModeManager.PoolManager.CreatePool(thorn, size, capacity);
            player = GameModeManager.Player;
        }

        if (!canUseSkill)   // 스킬 쿨타임이 끝났는지 확인
            return;

        base.ExecuteSkill();    // 스킬 쿨타이머 실행

        // 플레이어 현재 위치
        Vector3 playerPosition = player.transform.position;
        // 플레이어가 바라보는 앞 방향 (정규화된 단위 벡터)
        Vector3 forwardDirection = player.transform.forward.normalized;

        // 플레이어 위치에서 앞 방향으로 distance만큼 이동한 뒤, thorn의 Range만큼 더한 위치를 계산하고,
        // y축 방향으로 -1만큼 내려간 위치
        Vector3 targetPosition = playerPosition + forwardDirection * (distance + range) + Vector3.down;

        // 생성 직전 가시밭 생성 사운드 실행
        //GameModeManager.SoundManager.PlaySFX(110018);
        // 계산된 위치에 thorn 오브젝트를 풀에서 꺼내서 생성 (회전은 기본값)
        var obj = GameModeManager.PoolManager.GetPool(thorn, targetPosition, Quaternion.identity) as ThornPooledObject;

        var projectileParams = new ThornParams
        {
            // < 공통 파라미터 >
            radius = range,                                                 // 효과 적용 범위 반경
            damage = damage,                                                // 주는 데미지

            // < Thorn 전용 파라미터 >
            effectId = effectId,                                            // 슬로우 효과 ID
            lifeTime = lifeTime,
            duration = duration,
            value = value,
            tickInterval = tickInterval
        };

        obj.Setup(projectileParams);
    }
}