using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleResultJudge : MonoBehaviour
{
    [SerializeField] private UnitGenerator _unitGenerator;
    
    private readonly List<MapUnit> _unitsByTeam = new();
    private bool _isGameOver;
    
    public int EnemyKillCount { get; private set; }
    public int AllyDeathCount { get; private set; }
    
    public event Action<BattleResult> OnBattleResult;
    
    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _unitGenerator.OnUnitDeath += Foo;
    }

    private void UnbindEvents()
    {
        _unitGenerator.OnUnitDeath -= Foo;
    }

    private void Foo(MapUnit unit)
    {
        if (_isGameOver) 
        {
            return;
        }

        if (unit.SelectUnitData.TeamSide == TeamSide.ENEMY)
        {
            EnemyKillCount++;
        }
        else if (unit.SelectUnitData.TeamSide == TeamSide.ALLY)
        {
            AllyDeathCount++;
        }
        
        _unitGenerator.GetUnitsByTeam(TeamSide.ENEMY, _unitsByTeam);
        if (_unitsByTeam.Count == 0)
        {
            EndBattle(BattleResult.Victory);
            return;
        }
        _unitGenerator.GetUnitsByTeam(TeamSide.ALLY, _unitsByTeam);
        if (_unitsByTeam.Count == 0)
        {
            EndBattle(BattleResult.Defeat);
        }
    }

    private void EndBattle(BattleResult result)
    {
        _isGameOver = true;
        OnBattleResult?.Invoke(result);
    }
}
