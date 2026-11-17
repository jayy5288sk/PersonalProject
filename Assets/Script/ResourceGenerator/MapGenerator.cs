using System;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private TerrainDataLoader _terrainDataLoader;
    [SerializeField] private GameObject _defaultTilePrefab; // 디폴트 용도 지형
    [SerializeField] private float _groundHeight = 0.75f;

    private Dictionary<Vector2Int, TerrainData> _terrainBySection = new();
    public IReadOnlyDictionary<Vector2Int, TerrainData> TerrainBySection => _terrainBySection;

    public const float TERRAIN_SIZE = 1f;
    public int RowCount { get; private set; }
    public int ColumnCount { get; private set; }
    public float GroundHeight => _groundHeight;

    public event Action OnMapGenerateCompleted;
    
    /// <summary>
    /// 구글 스프레드시트 상의 지형 ID대로 맵 생성
    /// </summary>
    public void GenerateMap(JArray mapArray)
    {
        RowCount = mapArray.Count;
        ColumnCount = ((JArray)mapArray[0]).Count;

        for (int i = 0; i < RowCount; i++)
        {
            for (int j = 0; j < ColumnCount; j++)
            {
                int currentID = mapArray[i][j].Value<int>(); // 만들어질 지형의 ID
                GameObject currentTerrain; // 만들어질 지형
                Vector2Int currentSection = new Vector2Int(j, i); // 만들어질 지형 위치
                
                if (_terrainDataLoader.TryGetTerrainData(currentID, out TerrainData terrainData))
                {
                    _terrainBySection.Add(currentSection, terrainData);
                    currentTerrain = terrainData.TilePrefab;
                }
                else
                {
                    currentTerrain = _defaultTilePrefab;
                }
                
                // 지형 설치 위치
                Vector3 position = new Vector3(currentSection.x * TERRAIN_SIZE, 0.5f, 
                                              (RowCount - 1 - currentSection.y) * TERRAIN_SIZE);
                
                // 위치에 설치
                Instantiate(currentTerrain, position, Quaternion.identity, transform);
            }
        }
        
        // 설치 완료 이벤트 발생
        OnMapGenerateCompleted?.Invoke();
    }

    // (열, 행) → 바닥 위치를 놓을 월드 좌표
    public Vector3 ReturnTerrainPosition(Vector2Int mapPosition)
    {
        float x = mapPosition.x * TERRAIN_SIZE;
        float y = _groundHeight;
        float z = (RowCount - 1 - mapPosition.y) * TERRAIN_SIZE;

        return new Vector3(x, y, z);
    }
    
    // 월드 좌표 → (열, 행). 칸 중심이 정수 좌표이므로 반올림
    public Vector2Int WorldToSection(Vector3 worldPosition)
    {
        int column = Mathf.RoundToInt(worldPosition.x / TERRAIN_SIZE);
        int row = RowCount - 1 - Mathf.RoundToInt
        (
            worldPosition.z / TERRAIN_SIZE
        );

        return new Vector2Int(column, row);
    }

    public bool IsInsideMap(Vector2Int section)
    {
        bool isColumnInside = section.x >= 0 && section.x < ColumnCount;
        bool isRowInside = section.y >= 0 && section.y < RowCount;

        return isColumnInside && isRowInside;
    }
}
