using Newtonsoft.Json.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] private TerrainDataLoader _terrainDataLoader;
    [SerializeField] private GameObject _defaultTilePrefab; // 디폴트 용도 지형

    private const float TERRAIN_SIZE = 1f;

    public void GenerateMap(JArray mapArray)
    {
        int rowCount = mapArray.Count;
        int columnCount = ((JArray)mapArray[0]).Count;

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
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
                Vector3 position = new Vector3(j * TERRAIN_SIZE, 0.5f, (rowCount - 1 - i) * TERRAIN_SIZE);
                
                // 위치에 설치
                Instantiate(currentTerrain, position, Quaternion.identity, transform);
            }
        }
    }
}
