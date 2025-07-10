using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 맵 타일이 활성화되면 장애물을 랜덤하게 배치하고,
/// 비활성화될 때 장애물을 전부 비활성화한 후 타일 오브젝트를 Pool로 반환하는 클래스
/// </summary>
public class MapTileObj : PooledObject
{
    [Header("Map Tile's Obstacle Setting")]
    [SerializeField] MapTileData tileData;  // 장애물 확률 및 종류를 정의하는 맵 타일 설정 데이터
    [SerializeField] List<MapTileObstacle> enabledObstacles = new();    // 아직 사용되지 않은 장애물 리스트 (비활성 상태)
    [SerializeField] List<MapTileObstacle> currentObstacles = new();    // 현재 타일에 배치된 장애물 리스트 (활성 상태)

    #region Unity Event
    private void OnEnable()
    {
        // 타일이 활성화될 때 장애물을 무작위로 배치
        ActivateRandom();
    }

    private void OnDisable()
    {
        // 타일이 비활성화될 때 장애물을 비활성화하고 원상복구
        foreach (var obstacle in currentObstacles)
        {
            obstacle.gameObject.SetActive(false);
            enabledObstacles.Add(obstacle);
        }
        // 현재 장애물 목록 초기화
        currentObstacles.Clear();
    }
    #endregion

    #region Random Obstacle
    /// <summary>
    /// 설정된 확률 기반으로 랜덤하게 장애물 종류와 개수를 정하고 활성화
    /// </summary>
    private void ActivateRandom()
    {
        // 장애물 개수를 결정하기 위한 확률값 생성 (0~100)
        float countRate = Random.Range(0f, 100f);
        int activateCount = 0;

        // tileData.ObstacleNumOdds의 확률 구간 중 해당하는 개수 찾기
        for (int num = 0; num < tileData.ObstacleNumOdds.Count; num++)
        {
            if (countRate <= tileData.ObstacleNumOdds[num])
            { 
                activateCount = num;
                break;
            }
        }

        // 지정된 개수만큼 장애물 활성화
        while (currentObstacles.Count < activateCount)
        {
            float typeRate = Random.Range(0f, 100f);

            foreach (var oddMap in tileData.OddsMap)
            {
                if (typeRate <= oddMap.Value)
                {
                    // 해당 타입의 비활성화된 장애물 탐색
                    MapTileObstacle obstacle =  TryFindObstacle(oddMap.Key);

                    if (obstacle != null)
                    {
                        currentObstacles.Add(obstacle);     // 현재 활성화 목록에 추가
                        enabledObstacles.Remove(obstacle);  // 비활성 목록에서 제거
                        obstacle.gameObject.SetActive(true);    // 실제 활성화
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 주어진 타입에 해당하는 비활성 장애물을 탐색하여 반환
    /// </summary>
    /// <param name="type">찾고자 하는 장애물 타입</param>
    /// <returns>비활성 상태에서 해당 타입의 장애물 반환, 없으면 null</returns>
    private MapTileObstacle TryFindObstacle(ObstacleType type)
    {
        foreach (MapTileObstacle obstacle in enabledObstacles)
        {
            if (obstacle.Type == type)
                return obstacle;
        }

        return null;
    }
    #endregion
}

/// <summary>
/// 맵 타일에 배치될 수 있는 장애물의 종류를 정의
/// </summary>
public enum ObstacleType
{
    Rock,
    Tree,
    Wall
}