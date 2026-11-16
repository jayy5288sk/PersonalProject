using System.Collections.Generic;
using UnityEngine;

public class MovementRangeCalculator
{
    // 상, 하, 좌, 우 4방향.
    // 칸 y(행)는 아래로 갈수록 커지므로 Vector2Int.up은 화면에서 아래 칸이다.
    private static readonly Vector2Int[] DIRECTIONS =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    /// <summary>
    /// 출발 칸에서 이동력 안에 갈 수 있는 칸과 그 칸까지 드는 누적 비용을 돌려준다.
    /// </summary>
    public Dictionary<Vector2Int, int> GetReachableSections(Vector2Int startSection, int movementPoints, string teamSide,
                                                            IReadOnlyDictionary<Vector2Int, TerrainData> terrainBySection,
                                                            IReadOnlyDictionary<Vector2Int, MapUnit> unitsBySection)
    {
        Dictionary<Vector2Int, int> reachableSections = new();
        List<Vector2Int> neighbours = new();
        
        reachableSections[startSection] = 0;
        neighbours.Add(startSection);

        while (neighbours.Count > 0)
        {
            int cheapestIndex = 0; // 가장 비용이 싼 지점

            // 이웃한 곳에서 이동력이 적게 드는 곳을 찾기
            for (int i = 1; i < neighbours.Count; i++)
            {
                if (reachableSections[neighbours[i]] < reachableSections[neighbours[cheapestIndex]])
                {
                    cheapestIndex = i;
                }
            }
            
            // 해당하는 곳을 담고 난 다음 후보 목록에서 지우기
            Vector2Int currentSection = neighbours[cheapestIndex];
            neighbours.RemoveAt(cheapestIndex);

            // 해당 칸에 이웃하는 4방향 확인
            foreach (Vector2Int direction in DIRECTIONS)
            {
                Vector2Int nextSection = currentSection + direction; // 이웃 칸
                
                terrainBySection.TryGetValue(nextSection, out TerrainData terrainData);
                // 1. 지형 데이터가 없을 경우
                if (terrainData == null) 
                {
                    continue;
                }
                // 2. 이동 불가인 지형일 경우
                if (!terrainData.IsMoveAvailable)
                {
                    continue;
                }
                // 3. 유닛 유무 및 유닛 진영 확인
                unitsBySection.TryGetValue(nextSection, out MapUnit unit);
                if (unit && unit.SelectUnitData.TeamSide != teamSide)
                {
                    continue;
                }
                // 4. 필요 이동력 비교
                int expansePoint = reachableSections[currentSection] + 
                                       terrainData.RequireMove; // 소모 이동력 + 지형 소모 이동력
                if (expansePoint > movementPoints)
                {
                    continue;
                }
                // 5. 비용 비교
                bool isRecorded = reachableSections.TryGetValue(nextSection, out int costMovement);
                if (isRecorded && costMovement <= expansePoint)
                {
                    continue;
                }
                // 6. 후보 추가 및 비용 갱신
                reachableSections[nextSection] = expansePoint;
                if (!neighbours.Contains(nextSection))
                {
                    neighbours.Add(nextSection);
                }
            }
        }
        
        // 시작점 제외
        reachableSections.Remove(startSection);
        // 같은 진영 유닛이 서 있는 칸은 지나갈 수만 있고 멈출 수 없으므로 결과에서 제외
        foreach (KeyValuePair<Vector2Int, MapUnit> unitPair in unitsBySection)
        {
            if (unitPair.Value.SelectUnitData.TeamSide == teamSide)
            {
                reachableSections.Remove(unitPair.Key);
            }
        }
        
        return reachableSections;
    }
}
