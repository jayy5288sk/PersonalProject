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

    /// <summary>
    /// 범위 밖이면 false. 범위 안이면 맞을 적(공격자와 다른 진영)을 result에 채우고 true.
    /// 실제 공격과 예상 전투 결과가 같은 대상 규칙을 쓰도록 한 곳에 둔다
    /// </summary>
    public bool TryCollectTargetsAt(Vector2Int section, List<MapUnit> result)
    {
        result.Clear();
        if (!_attackSections.Contains(section))
        {
            return false;
        }

        MapUnit attacker = _stateContext.SelectedUnit;

        // 맞는 칸을 구하고, 그 칸에 있는 공격자와 다른 진영의 유닛만 모은다
        _attackRangeCalculator.ApplyAttackSection(attacker.SectionUnitPos,
                                                  _stateContext.SelectedWeapon, section,
                                                  _attackSections, _hitSections);
        AttackExecutor.CollectTargets(attacker, _hitSections, _unitGenerator, result);
        return true;
    }

    public override StateBase SectionClick(Vector2Int section)
    {
        // 범위 밖 클릭 시, 무장 다시 고르기
        if (!TryCollectTargetsAt(section, _targets))
        {
            return _cancelState;
        }

        // 범위 안이지만 맞을 적이 없을 때(빈 칸, 아군 칸) '무시'
        if (_targets.Count == 0)
        {
            return null;
        }

        MapUnit attacker = _stateContext.SelectedUnit;
        AttackExecutor.Execute(attacker, _stateContext.SelectedWeapon, _targets, _unitGenerator);

        // 행동 완료 및 유닛 선택 대기
        attacker.CompleteUnitAction();
        return _doneState;
    }
}
