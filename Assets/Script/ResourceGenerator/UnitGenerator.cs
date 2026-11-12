using Newtonsoft.Json.Linq;
using UnityEngine;

public class UnitGenerator : MonoBehaviour
{
    [SerializeField] private UnitDataLoader _unitDataLoader;

    private const float UNIT_SPAWNPOINT_SIZE = 1f; // MapGenerator의 TERRAIN_SIZE와 같아야 한다.
    private const float UNIT_SPAWN_Y_POSITION = 1.7f; // 임시 상수. 유닛 배치 시 높이.
    
    public void PlacementUnit(JArray spawnArray, int unitSpawnArrayRow)
    {
        // 스폰 데이터를 불러오지 못한 경우. 원인 로그는 UnitSpawnDataLoader 쪽에서 출력
        if (spawnArray == null)
        {
            return;
        }

        int spawnRowCount = spawnArray.Count;
        int columnCount = ((JArray)spawnArray[0]).Count;

        for (int i = 0; i < spawnRowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                int currentID = spawnArray[i][j].Value<int>(); // 배치될 유닛의 ID
                GameObject currentUnit; // 배치될 유닛
                
                if (_unitDataLoader.TryGetUnitData(currentID, out UnitData unitData))
                {
                    currentUnit = unitData.UnitPrefab;
                }
                else
                {
                    continue;
                }
                
                // 유닛 설치 위치
                Vector3 position = new Vector3(j * UNIT_SPAWNPOINT_SIZE, UNIT_SPAWN_Y_POSITION,
                                              (unitSpawnArrayRow - 1 - i) * UNIT_SPAWNPOINT_SIZE);
                
                // 위치에 설치 (예외 처리 포함)
                if (currentUnit == null)
                {
                    Debug.LogError($"유닛 {currentID} 프리팹이 빠졌다");
                    continue;
                }
                
                Instantiate(currentUnit, position, Quaternion.identity, transform);
            }
        }
    }
}
