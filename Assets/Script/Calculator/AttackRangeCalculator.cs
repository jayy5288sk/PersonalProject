using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackRangeCalculator
{
    // Pierce 용도. MovementRangeCalculator 참고.
    private static readonly Vector2Int[] DIRECTIONS =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };
    
    public void GetAttackableSections(Vector2Int center, WeaponData weapon,
                                      Func<Vector2Int, bool> isInsideMap, // 칸을 주면 T/F
                                      HashSet<Vector2Int> result) // result에 담음 
    {
        result.Clear(); // 먼저 비우고 시작
        
        int maxRange = weapon.MaxRange; // 무기 최대 사거리
        int minRange = weapon.MinRange; // 무기 최소 사거리
        
        switch (weapon.RangeShape)
        {
            case AttackShape.Single:
            case AttackShape.Area:
            {
                // 거리 절대값 계산
                for (int x = -maxRange; x <= maxRange; x++)
                {
                    for (int y = -maxRange; y <= maxRange; y++)
                    {
                        int distance = Mathf.Abs(x) + Mathf.Abs(y);
                        Vector2Int section = center + new Vector2Int(x, y);
                        
                        bool isRange = distance >= minRange && distance <= maxRange;
                        bool isInside = isInsideMap(section);

                        if (!isInside) // 구성된 맵 안의 지형인가?
                        {
                            continue;
                        }
                        if (!isRange) // 설정된 사거리 안에 있는가?
                        {
                            continue;
                        }
                        
                        result.Add(section);
                    }
                }
                
                break;
            }
            case AttackShape.Pierce:
            {
                foreach (Vector2Int direction in DIRECTIONS)
                {
                    for (int i = minRange; i <= maxRange; i++)
                    {
                        Vector2Int section = center + direction * i;
                        
                        bool isInside = isInsideMap(section); // 구성된 맵 안의 지형인가?
                        if (!isInside)
                        {
                            continue;
                        }
                        
                        result.Add(section);
                    }
                }
                
                break;
            }
        }
    }

    public void ApplyAttackSection(Vector2Int center, WeaponData weapon,
                                   Vector2Int clicked, HashSet<Vector2Int> attackable,
                                   List<Vector2Int> result)
    {
        result.Clear(); // 먼저 비우고 시작

        switch (weapon.RangeShape)
        {
            // 클릭한 한 칸
            case AttackShape.Single:
            {
                result.Add(clicked); 
                break;
            }
            // 공격 가능한 칸 전부
            case AttackShape.Area:
            {
                result.AddRange(attackable); 
                break;
            }
            case AttackShape.Pierce:
            {
                // 클릭한 칸으로 방향을 구하기
                Vector2Int diff = clicked - center;
                Vector2Int direction = new Vector2Int(Math.Sign(diff.x), 
                                                      Math.Sign(diff.y));

                for (int i = weapon.MinRange; i <= weapon.MaxRange; i++)
                {
                    Vector2Int section = center + direction * i;

                    // attackable은 이미 맵 안 + 사거리 안인 칸만 담고 있다
                    if (!attackable.Contains(section))
                    {
                        continue;
                    }

                    result.Add(section);
                }

                break;
            }
        }
    }
}
