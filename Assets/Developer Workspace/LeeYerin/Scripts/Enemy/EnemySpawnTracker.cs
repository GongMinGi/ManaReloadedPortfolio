using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 적 생성 제한 및 생존 상태를 추적하기 위한 헬퍼 클래스
/// 
/// 특정 타입의 적에 대해 스폰 수 제한, 현재 살아있는 적의 수
/// 해당 적 타입이 모두 생성되고 제거되었는지를 판단하는 데 사용됨
/// 
/// EnemyPool 또는 스폰 제어 로직에서 활용됨
/// </summary>
public class EnemySpawnTracker
{
    private int maxEnemyCount = 0;  // 해당 타입의 최대 생성 가능한 적 개수 (스폰 제한 수)
    private int spawnedEnemyCount = 0;  // 현재까지 생성된 적 개수
    private int currentAliveCount;  // 현재 살아 있는 적 개수

    /// <summary>
    /// 현재 해당 타입의 적을 생성할 수 있는지 여부를 나타내는 속성
    /// </summary>
    public bool CanSpawn => spawnedEnemyCount < maxEnemyCount;

    /// <summary>
    /// 해당 타입의 적을 정해진 수만큼 생성했고, 그 모두가 사망했는가를 판단하는 속성
    /// </summary>
    private bool IsCleared => !CanSpawn && currentAliveCount == 0;

    /// <summary>
    /// 현재까지 스폰한 해당 타입의 적 및 생존 적 수량을 업데이트하는 메서드
    /// 호출 시 spawnedEnemyCount와 currentAliveCount +1
    /// </summary>
    public void NotifySpawned()
    {
        spawnedEnemyCount++;
        currentAliveCount++;
    }

    public void NotifyDied()
    {
        currentAliveCount--;

        if (IsCleared)  // 현재 페이즈에서 해당 타입의 모든 적이 사망했을 경우
            GameModeManager.EnemyManager.TryAdvancePhase(); // 이를 EnemyManager에 알림   
    }

    /// <summary>
    /// EnemyPoolLimiter의 최대 적 생성 수를 설정하고,
    /// 현재까지 생성된 및 살아있는 적의 개수를 초기화하는 메서드
    /// 
    /// 페이즈 전환 등 외부 조건에 따라 적 생성 한도를 갱신할 때 사용함
    /// </summary>
    /// <param name="maxEnemyCount">최대 생성 가능한 적 개수 (스폰 제한 수)</param>
    public void Set(int maxEnemyCount)
    {
        this.maxEnemyCount = maxEnemyCount;
        spawnedEnemyCount = 0;
        currentAliveCount = 0;
    }
}