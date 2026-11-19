using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 공격 실행 공용 처리.
/// 즉, 아군 공격(AttackTargetSelect)과 적 AI(EnemyAIController)가 같은 규칙을 쓴다
/// </summary>
public static class AttackExecutor
{
    /// <summary>
    /// 맞는 칸 중 공격자와 다른 진영의 유닛만 result에 채움
    /// </summary>
    public static void CollectTargets(MapUnit attacker, List<Vector2Int> hitSections,
                                      UnitGenerator unitGenerator, List<MapUnit> result)
    {
        result.Clear();
        foreach (Vector2Int hit in hitSections)
        {
            if (unitGenerator.TryGetUnitAtSection(hit, out MapUnit unit) &&
                unit.SelectUnitData.TeamSide != attacker.SelectUnitData.TeamSide)
            {
                result.Add(unit);
            }
        }
    }

    /// <summary>
    /// 대상마다 데미지 적용 → HP 0이면 제거, 마지막에 EN 1회 소모
    /// </summary>
    public static void Execute(MapUnit attacker, WeaponData weapon, 
                               List<MapUnit> targets,
                               UnitGenerator unitGenerator)
    {
        foreach (MapUnit target in targets)
        {
            int damage = BattleCalculator.CalculateDamage(attacker, weapon, target);
            target.TakeDamage(damage);
            Debug.Log($"{attacker.name} → {target.name} : {damage} (남은 HP {target.CurrentHp})");

            if (target.IsDefeated)
            {
                Debug.Log($"{target.name} 격파");
                unitGenerator.RemoveUnit(target);
            }
        }

        // 맞힌 적의 수와 상관없이 공격 1회당 1번만 소모
        attacker.ConsumeEnergy(weapon.RequireEn);
        Debug.Log($"{attacker.name} EN 소모 : {weapon.RequireEn} (남은 EN {attacker.CurrentEn})");
    }
}
