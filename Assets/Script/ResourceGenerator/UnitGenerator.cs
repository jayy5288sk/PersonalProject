using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class UnitGenerator : MonoBehaviour
{
    private const float UNIT_SPAWNPOINT_SIZE = 1f; // MapGenerator의 TERRAIN_SIZE와 같아야 한다.
    private const float UNIT_SPAWN_Y_POSITION = 1f; // 임시 상수. 유닛 배치 시 높이.
    
    [SerializeField] private UnitDataLoader _unitDataLoader;

    private Dictionary<Vector2Int, MapUnit> _unitsBySection = new();
    public IReadOnlyDictionary<Vector2Int, MapUnit> UnitsBySection => _unitsBySection;
    private int _mapRowCount;

    public event Action OnUnitPlacementCompleted;

    public void PlacementUnit(JArray spawnArray, int unitSpawnArrayRow)
    {
        // 스폰 데이터를 불러오지 못한 경우. 원인 로그는 UnitSpawnDataLoader 쪽에서 출력
        if (spawnArray == null)
        {
            return;
        }
        
        _mapRowCount = unitSpawnArrayRow;

        int spawnRowCount = spawnArray.Count;
        int columnCount = ((JArray)spawnArray[0]).Count;

        for (int i = 0; i < spawnRowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                int currentID = spawnArray[i][j].Value<int>(); // 배치될 유닛의 ID
                GameObject currentUnit; // 배치될 유닛
                GameObject mapSpawnUnit; // 정보를 받을 유닛
                
                if (_unitDataLoader.TryGetUnitData(currentID, out UnitData unitData))
                {
                    currentUnit = unitData.UnitPrefab;
                }
                else
                {
                    continue;
                }
                
                // 유닛 설치 위치
                Vector2Int unitSpawnPos = new Vector2Int(j, i);
                Vector3 position = SectionToUnitPosition(unitSpawnPos);
                
                // 위치에 설치 (예외 처리 포함)
                if (currentUnit == null)
                {
                    Debug.LogError($"ID {currentID}번 유닛 프리팹이 빠졌다");
                    continue;
                }
                
                mapSpawnUnit = Instantiate(currentUnit, position, Quaternion.identity, transform);
                
                MapUnit tempUnit = mapSpawnUnit.AddComponent<MapUnit>();
                tempUnit.SetUnitData(unitData, unitSpawnPos);
                _unitsBySection.Add(unitSpawnPos, tempUnit);
            }
        }

        OnUnitPlacementCompleted?.Invoke();
    }

    /// <summary>
    /// 해당 진영의 유닛을 result에 채움. 호출하는 쪽의 리스트를 재사용
    /// </summary>
    public void GetUnitsByTeam(string teamSide, List<MapUnit> result)
    {
        result.Clear();
        foreach (MapUnit unit in _unitsBySection.Values)
        {
            if (unit.SelectUnitData.TeamSide == teamSide)
            {
                result.Add(unit);
            }
        }
    }
    
    /// <summary>
    /// 구역에 해당하는 유닛을 가져오기 위한 정보를 주는 함수
    /// </summary>
    public bool TryGetUnitAtSection(Vector2Int section, out MapUnit mapUnit)
    {
        if (_unitsBySection.TryGetValue(section, out mapUnit))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private Vector3 SectionToUnitPosition(Vector2Int section)
    {
        float x = section.x * UNIT_SPAWNPOINT_SIZE; 
        float y = UNIT_SPAWN_Y_POSITION;
        float z = (_mapRowCount - 1 - section.y) * UNIT_SPAWNPOINT_SIZE; 

        return new Vector3(x, y, z);
    }

    public void MoveUnitToSection(MapUnit mapUnit, Vector2Int section)
    {
        Vector2Int prevPos = mapUnit.SectionUnitPos;
        _unitsBySection.Remove(prevPos);
        _unitsBySection.Add(section, mapUnit);
        mapUnit.SetUnitData(mapUnit.SelectUnitData, section);
        mapUnit.transform.position = SectionToUnitPosition(section);
    }
}
