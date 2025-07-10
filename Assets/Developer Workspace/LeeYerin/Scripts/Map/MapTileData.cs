using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 맵 타일에 대한 정보를 담는 ScriptableObject
/// 무작위 장애물 배치 로직에 활용되는 확률 데이터를 제공
/// </summary>
[CreateAssetMenu(fileName = "MapTileData", menuName = "Scriptable Objects/MapTileData")]
public class MapTileData : ScriptableObject
{
    #region Serialized Fields
    [Header("Obstacle Settings")]
    [Tooltip("Types of obstacles that can be placed")]
    [SerializeField] List<ObstacleType> tileObstacles = new();  // 배치 가능한 장애물 종류 리스트
    [Tooltip("Probability of appearance for each obstacle type")]
    [SerializeField] List<float> obstacleOdds = new();  // 각 장애물 타입별 출현 확률 (tileObstacles와 인덱스 매칭)
    [Tooltip
        (
        "Obstacle count selection probability\n(index = count, value = cumulative probability)"
        )]
    [SerializeField] List<float> obstacleNumOdds = new();   // 장애물 개수 선택 확률 (인덱스 = 개수, 값 = 누적 확률)
    #endregion

    #region Runtime Data
    private Dictionary <ObstacleType, float> obstacleOddsMap;   // 내부에서 조합한 장애물 타입별 확률 맵
    #endregion

    #region Public Properties
    /// <summary>
    /// 장애물 개수 확률 리스트 (인덱스를 개수로 해석)
    /// </summary>
    public List<float> ObstacleNumOdds => obstacleNumOdds;

    /// <summary>
    /// 장애물 타입별 출현 확률을 담은 Dictionary (지연 초기화 방식)
    /// </summary>
    public Dictionary<ObstacleType, float> OddsMap {
        get 
        {
            if (obstacleOddsMap == null)
                SetObstacleOdds();
            return obstacleOddsMap;
        }
    }
    #endregion

    #region Obstacle Odds Initialization
    /// <summary>
    /// tileObstacles와 obstacleOdds를 이용해 타입별 확률 Dictionary 구성하는 메서드
    /// </summary>
    private void SetObstacleOdds()
    {
        obstacleOddsMap = new();

        for (int i = 0; i < tileObstacles.Count; i++)
            obstacleOddsMap.Add(tileObstacles[i], obstacleOdds[i]);
    }
    #endregion
}