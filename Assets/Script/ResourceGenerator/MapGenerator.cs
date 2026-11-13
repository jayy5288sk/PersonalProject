using System;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private TerrainDataLoader _terrainDataLoader;
    [SerializeField] private GameObject _defaultTilePrefab; // 디폴트 용도 지형

    public const float TERRAIN_SIZE = 1f;
    public int RowCount { get; private set; }
    public int ColumnCount { get; private set; }

    public event Action OnMapGenerateCompleted;
    
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
                
                if (_terrainDataLoader.TryGetTerrainData(currentID, out TerrainData terrainData))
                {
                    currentTerrain = terrainData.TilePrefab;
                }
                else
                {
                    currentTerrain = _defaultTilePrefab;
                }
                
                // 지형 설치 위치
                Vector3 position = new Vector3(j * TERRAIN_SIZE, 0.5f, (RowCount - 1 - i) * TERRAIN_SIZE);
                
                // 위치에 설치
                Instantiate(currentTerrain, position, Quaternion.identity, transform);
            }
        }
        
        // 설치 완료 이벤트 발생
        OnMapGenerateCompleted?.Invoke();
    }
}
