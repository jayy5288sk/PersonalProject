using System.Collections.Generic;
using UnityEngine;

public class AttackTargetSelect : StateBase
{
    private readonly StateContext _stateContext;
    private readonly AttackRangeCalculator _attackRangeCalculator;
    private readonly MapGenerator _mapGenerator;
    private readonly AttackRangeDisplay _attackRangeDisplay;

    private StateBase _doneState;
    private StateBase _cancelState;
    
    private HashSet<Vector2Int> _attackSections = new();
    private List<Vector3> _highlightPosition = new();
    
    public AttackTargetSelect(StateContext stateContext,
                              AttackRangeCalculator calculator,
                              MapGenerator mapGenerator,
                              AttackRangeDisplay display)
    {
        _stateContext = stateContext;
        _attackRangeCalculator = calculator;
        _mapGenerator = mapGenerator;
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
        // 임시: 6단계 확인용 탈출 경로. 어느 칸을 클릭해도 무장 선택
        // F-06에서 공격 대상 판별·공격 처리로 교체
        Debug.Log($"{_stateContext.SelectedWeapon.WeaponName}으로 {section} 클릭");
        return _cancelState;
    }
}
