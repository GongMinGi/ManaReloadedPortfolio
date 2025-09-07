using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 생명의 빛 조합 마법 클래스
/// 오브젝트 풀에서 LifeLight 오브젝트를 가져와 플레이어 위치에 생성하는 기능을 담당함
/// </summary>
public class LifeLight : BaseCombinationMagic
{
    [SerializeField] LifeLightPooledObject lifeLight;   // 사용할 Thorn 오브젝트 프리팹
    [SerializeField] int size;                  // 풀에 미리 생성할 오브젝트 수
    [SerializeField] int capacity;              // 풀의 최대 수용 가능 오브젝트 수

    PlayerController player = null;             // 플레이어 컨트롤러 참조 (초기화 전 null)

    public override void ExecuteSkill()
    {
        // player가 null이면, 오브젝트 풀을 생성하고 플레이어 참조를 가져옴
        if (player == null)
        {
            GameModeManager.PoolManager.CreatePool(lifeLight, size, capacity);
            player = GameModeManager.Player;
        }

        if (!canUseSkill)   // 스킬 쿨타임이 끝났는지 확인
            return;

        base.ExecuteSkill();    // 스킬 쿨타이머 실행

        // 생명의 빛 소환 직전 사운드 재생
        //GameModeManager.SoundManager.PlaySFX(110019);
        GameModeManager.PoolManager.GetPool(lifeLight, player.transform.position, Quaternion.identity);
    }
}
