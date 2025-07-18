using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 맵 타일의 장애물에 관한 내용을 관리하는 클래스
/// 장애물의 종류 정보를 담고 있음
/// </summary>
public class MapTileObstacle : MonoBehaviour
{
    [SerializeField] ObstacleType type;
    public ObstacleType Type => type;
}