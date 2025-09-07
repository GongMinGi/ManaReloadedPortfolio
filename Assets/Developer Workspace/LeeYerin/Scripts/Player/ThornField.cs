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
        Vector3 targetPosition = playerPosition + forwardDirection * (distance + thorn.Range) + -Vector3.up;

        // 생성 직전 가시밭 생성 사운드 실행
        //GameModeManager.SoundManager.PlaySFX(110018);
        // 계산된 위치에 thorn 오브젝트를 풀에서 꺼내서 생성 (회전은 기본값)
        GameModeManager.PoolManager.GetPool(thorn, targetPosition, Quaternion.identity);
    }
}
