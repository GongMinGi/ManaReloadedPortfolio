using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개발자: 이예린
/// 
/// 플레이어 위치를 기준으로 무한 맵 형태의 타일을 동적으로 관리하는 매니저 클래스
/// 
/// - 타일 프리팹을 오브젝트 풀로 관리하여 생성/제거 비용을 최소화함
/// - 플레이어 주변 타일만 유지, 범위를 벗어난 타일은 Pool로 반환함
/// - 현재 타일(플레이어가 위치한 타일) 좌표를 기준으로 타일 업데이트 로직 수행
/// </summary>
public class MapTileManager : MonoBehaviour
{
    #region Fields and Properties
    #region Singleton
    private static MapTileManager instance;
    #endregion

    #region Map Object Pool Settings
    [Header("Map Object Pool Settings")]
    [SerializeField] PooledObject tilePrefab;
    [SerializeField] int size;
    [SerializeField] int capacity;
    #endregion

    #region Infinite Tile Map Settings
    [Header("Infinite Tile Map Settings")]
    [SerializeField] PlayerController player;
    public PlayerController Player { get { return player; } set { player = value; } }
    private Vector2Int currentTilePos = new Vector2Int(int.MaxValue, int.MaxValue);

    [Tooltip("Width/height of one tile")]
    [SerializeField] int tileSize = 10;     // 한 타일의 폭/높이
    [Tooltip("Tile radius around the player. 1 = 3x3, 2 = 5x5, etc.")]
    [SerializeField] int tileRange = 1; // 3x3 그리드
    #endregion

    #region Active Tile Data
    // 현재 활성화된 타일을 저장해두는 딕셔너리
    private Dictionary<Vector2Int, PooledObject> activeTiles = new();
    #endregion

    #region State Flags
    private bool isReady;

    public bool IsReady => isReady;
    #endregion
    #endregion

    #region Unity Event
    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameModeManager.Player != null);   // 플레이어를 알 수 있을 때까지 대기

        if (instance == null)
        {
            instance = this;
            GameModeManager.MapTileManager = instance;
        }
        else
            Destroy(instance);

        yield return new WaitUntil(() => player != null);   // 플레이어가 할당된 후 진행

        GameModeManager.PoolManager.CreatePool(tilePrefab, size, capacity);
        UpdateCurrentPos();

        isReady = true;
    }
    #endregion

    #region Update Map Tiles
    /// <summary>
    /// 플레이어 좌표 변경 확인될 시 맵 타이를 업데이트하는 메서드
    /// </summary>
    public void UpdateCurrentPos()
    {
        // 플레이어가 다른 타일로 이동했을 경우에만 업데이트
        Vector2Int newTilePos = GetPlayerTilePos();

        if (currentTilePos != newTilePos)
        {
            currentTilePos = newTilePos;
            UpdateTiles();
        }
    }

    /// <summary>
    /// 플레이어의 월드 좌표를 타일 좌표(Vector2Int)로 변환해주는 메서드
    /// 타일 간격(tileSize)을 기준으로 좌표를 정수 단위로 정렬한다.
    /// </summary>
    /// <returns>현재 플레이어가 위치한 타일의 좌표</returns>
    private Vector2Int GetPlayerTilePos()
    {
        // FloorToInt를 사용해 소수점을 내림 처리함으로써,
        // 음수 좌표도 포함한 타일 그리드 기준으로 정확히 정렬함
        return new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x / tileSize),
            Mathf.FloorToInt(player.transform.position.z / tileSize)
            );
    }

    /// <summary>
    /// 현재 플레이어 위치를 기준으로 필요한 타일은 유지,
    /// 그렇지 못한 타일은 Pool에 반환하는 메서드
    /// </summary>
    private void UpdateTiles()
    {
        // 새로 필요한 타일 좌표 집합
        HashSet<Vector2Int> neededTiles = new();    // 순서 상관없이, 중복을 허용하지 않는 HashSet 사용

        for (int dx = -tileRange; dx <= tileRange; dx++)
        {
            for (int dz = -tileRange; dz <= tileRange; dz++)
            {
                Vector2Int tilePos = currentTilePos + new Vector2Int(dx, dz);
                neededTiles.Add(tilePos);

                // 해당 위치에 아직 타일이 활성화되지 않았다면 Pool에서 오브젝트를 가져옴
                if (!activeTiles.ContainsKey(tilePos))
                {
                    Vector3 worldPos = new Vector3(tilePos.x * tileSize, 0f, tilePos.y *  tileSize);
                    PooledObject tile = GameModeManager.PoolManager.GetPool(tilePrefab, worldPos, Quaternion.identity);
                    activeTiles.Add(tilePos, tile);
                }
            }
        }

        // 제거할 타일 목록
        List<Vector2Int> removeTiles = new();

        foreach (var tile in activeTiles)
        {
            if (!neededTiles.Contains(tile.Key))
            {
                tile.Value.Release();   // 타일 Pool에 반환(비활성화)
                removeTiles.Add(tile.Key);
            }
        }

        // 딕셔너리에서 제거
        foreach (var tile in removeTiles)
            activeTiles.Remove(tile);
    }
    #endregion
}