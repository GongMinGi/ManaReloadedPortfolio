using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 적 풀링을 위한 확장 클래스
/// ObjectPool을 상속하며, 각 풀마다 적의 생성/사망을 추적하는 EnemySpawnTracker를 함께 관리
/// EnemySpawnTracker는 적 타입별로 개별 생성 수 제한 및 상태 추적을 담당함
/// </summary>
public class EnemyPool : ObjectPool
{
    [SerializeField] EnemySpawnTracker tracker;

    /// <summary>
    /// 적 생성 가능 여부 또는 정리 완료 여부 등을 판단할 때 사용
    /// </summary>
    public EnemySpawnTracker Tracker => tracker;

    /// <summary>
    /// EnemySpawnTracker를 Pool에 설정하는 초기화 메서드
    /// 보통 EnemyPool 생성 직후 1회 호출됨
    /// </summary>
    /// <param name="tracker">생성 수/사망 수 상태를 추적할 EnemySpawnTracker 인스턴스</param>
    public void Initialize(EnemySpawnTracker tracker)
    {
        this.tracker = tracker;
    }
}