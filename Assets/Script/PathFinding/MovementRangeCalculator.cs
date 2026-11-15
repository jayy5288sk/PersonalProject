using System.Collections.Generic;
using UnityEngine;

public class MovementRangeCalculator
{
    // 상, 하, 좌, 우 4방향. 칸 y(행)는 아래로 갈수록 커지므로 Vector2Int.up은 화면에서 아래 칸이다.
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
            int cheapestIndex = 0;

            for (int i = 1; i < neighbours.Count; i++)
            {
                if (reachableSections[neighbours[i]] < reachableSections[neighbours[cheapestIndex]])
                {
                    cheapestIndex = i;
                }
            }
            
            Vector2Int currentSection = neighbours[cheapestIndex];
            neighbours.RemoveAt(cheapestIndex);
        }
        
        return reachableSections;
    }
}
