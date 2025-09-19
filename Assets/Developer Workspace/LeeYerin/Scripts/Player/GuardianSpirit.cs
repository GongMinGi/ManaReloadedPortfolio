using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 수호령 소환 조합 마법 클래스
/// - 플레이어 주변에 5개의 수호령을 별 모양으로 배치하여 지속적으로 데미지를 가하는 스킬
/// 
/// 플레이어 위치에 수호령 스킬 풀 오브젝트를 풀에서 꺼내와 배치 및 활성화
/// </summary>
public class GuardianSpirit : BaseCombinationMagic
{
    [Tooltip("Guardian spirit object prefab to use")]
    [SerializeField] GuardianSpiritPooledObject guardian;   // 사용할 GuardianSpirit 오브젝트 프리팹
    [SerializeField] int size;                  // 풀에 미리 생성할 오브젝트 수
    [SerializeField] int capacity;              // 풀의 최대 수용 가능 오브젝트 수

    PlayerController player = null;             // 플레이어 컨트롤러 참조 (초기화 전 null)

    public override void ExecuteSkill()
    {
        // player가 null이면, 오브젝트 풀을 생성하고 플레이어 참조를 가져옴
        if (player == null)
        {
            GameModeManager.PoolManager.CreatePool(guardian, size, capacity);
            player = GameModeManager.Player;
        }

        if (!canUseSkill)   // 스킬 쿨타임이 끝났는지 확인
            return;

        base.ExecuteSkill();    // 스킬 쿨타이머 실행

        // 계산된 위치에 GuardianSpirit 오브젝트를 풀에서 꺼내서 생성 (회전은 기본값)
        GameModeManager.PoolManager.GetPool(guardian, player.transform.position, Quaternion.identity);
    }
}