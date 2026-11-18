using System.Collections.Generic;
using UnityEngine;

public class AttackTargetSelect : StateBase
{
    private readonly StateContext _stateContext;
    private readonly AttackRangeCalculator _attackRangeCalculator;
    private readonly MapGenerator _mapGenerator;
    private readonly UnitGenerator _unitGenerator;
    private readonly AttackRangeDisplay _attackRangeDisplay;

    private StateBase _doneState;
    private StateBase _cancelState;

    private HashSet<Vector2Int> _attackSections = new();
    private List<Vector3> _highlightPosition = new();
    private List<Vector2Int> _hitSections = new();
    private List<MapUnit> _targets = new();

    public AttackTargetSelect(StateContext stateContext,
                              AttackRangeCalculator calculator,
                              MapGenerator mapGenerator,
                              UnitGenerator unitGenerator,
                              AttackRangeDisplay display)
    {
        _stateContext = stateContext;
        _attackRangeCalculator = calculator;
        _mapGenerator = mapGenerator;
        _unitGenerator = unitGenerator;
        _attackRangeDisplay = display;
    }

    public void SetStateBase(StateBase done, StateBase cancel)
    {
        _doneState = done;
        _cancelState = cancel;
    }

    public override StateType StateName()
    {
        return StateType.AttackTargetSelect;
    }

    public override void StateEnter()
    {
        MapUnit mapUnit = _stateContext.SelectedUnit;

        _attackRangeCalculator.GetAttackableSections(mapUnit.SectionUnitPos,
                                                     _stateContext.SelectedWeapon,
                                                     _mapGenerator.IsInsideMap,
                                                     _attackSections);
        _highlightPosition.Clear();
        foreach (Vector2Int section in _attackSections)
        {
            _highlightPosition.Add(_mapGenerator.ReturnTerrainPosition(section));
        }
        
        _attackRangeDisplay.ShowAttackRangeHighlight(_highlightPosition);
    }

    public override void StateExit()
    {
        _attackRangeDisplay.ClearHighlights();
        _attackSections.Clear();
    }

    public override StateBase SectionClick(Vector2Int section)
    {
        // 범위 밖 클릭 시, 무장 다시 고르기
        if (!_attackSections.Contains(section))
        {
            return _cancelState;
        }

        MapUnit attacker = _stateContext.SelectedUnit;
        WeaponData weapon = _stateContext.SelectedWeapon;

        // 맞는 칸을 구하고, 그 칸에 있는 공격자와 다른 진영의 유닛만 모은다
        _attackRangeCalculator.ApplyAttackSection(attacker.SectionUnitPos, weapon, section,
                                                  _attackSections, _hitSections);
        _targets.Clear();
        foreach (Vector2Int hit in _hitSections)
        {
            if (_unitGenerator.TryGetUnitAtSection(hit, out MapUnit unit) &&
                unit.SelectUnitData.TeamSide != attacker.SelectUnitData.TeamSide)
            {
                _targets.Add(unit);
            }
        }

        // 범위 안이지만 맞을 적이 없음(빈 칸, 아군 칸) → 무시
        if (_targets.Count == 0)
        {
            return null;
        }

        foreach (MapUnit target in _targets)
        {
            int damage = BattleCalculator.CalculateDamage(attacker, weapon, target);
            target.TakeDamage(damage);
            Debug.Log($"{attacker.name} → {target.name} : {damage} (남은 HP {target.CurrentHp})");
        }
        
        // 맞힌 적의 수와 상관없이 공격 1회당 1번만 소모
        attacker.ConsumeEnergy(weapon.RequireEn);
        Debug.Log($"{attacker.name} EN 소모 : {weapon.RequireEn} (남은 EN {attacker.CurrentEn})");

        attacker.CompleteUnitAction();
        return _doneState;
    }
}
