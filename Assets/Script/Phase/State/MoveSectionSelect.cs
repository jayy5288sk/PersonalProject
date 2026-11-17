using System.Collections.Generic;
using UnityEngine;

public class MoveSectionSelect : StateBase
{
    private readonly StateContext _stateContext;
    private readonly MovementRangeCalculator _movementRangeCalculator;
    private readonly MapGenerator _mapGenerator;
    private readonly UnitGenerator _unitGenerator;
    private readonly MoveRangeDisplay _moveRangeDisplay;
    
    private StateBase _nextState;
    private Dictionary<Vector2Int, int> _availableSections = new();

    public MoveSectionSelect(StateContext stateContext,
                             MovementRangeCalculator movementRangeCalculator,
                             MapGenerator mapGenerator,
                             UnitGenerator unitGenerator,
                             MoveRangeDisplay moveRangeDisplay)
    {
        _stateContext = stateContext;
        _movementRangeCalculator = movementRangeCalculator;
        _mapGenerator = mapGenerator;
        _unitGenerator = unitGenerator;
        _moveRangeDisplay = moveRangeDisplay;
    }
    
    public void SetStateBase(StateBase stateBase)
    {
        _nextState = stateBase;
    }
    
    public override StateType StateName()
    {
        return StateType.MoveSectionSelect;
    }

    public override void StateEnter()
    {
        MapUnit mapUnit = _stateContext.SelectedUnit;
        
        // 필요값 주입 및 결과값 저장
        _availableSections = _movementRangeCalculator.GetReachableSections
        (
            mapUnit.SectionUnitPos,
            mapUnit.SelectUnitData.MovementPoints,
            mapUnit.SelectUnitData.TeamSide,
            _mapGenerator.TerrainBySection,
            _unitGenerator.UnitsBySection
        );

        // 변환 및 정보 목록 등록
        List<(Vector3 position, int cost)> highLightList = new();
        foreach (KeyValuePair<Vector2Int, int> pair in _availableSections)
        {
            Vector3 position = _mapGenerator.ReturnTerrainPosition(pair.Key);
            highLightList.Add((position, pair.Value));
        }
        
        // 결과값 보여주기
        _moveRangeDisplay.ShowMoveRangeHighlight(highLightList);
    }

    public override StateBase SectionClick(Vector2Int section)
    {
        MapUnit mapUnit = _stateContext.SelectedUnit;
        bool isExistSection = _availableSections.ContainsKey(section);
        
        // 이동 가능 칸인지 판별
        if (isExistSection)
        {
            _unitGenerator.MoveUnitToSection(mapUnit, section);
            _stateContext.AlreadyMovedUnit();
        }
        // UnitSelectWait가 들어올 때마다 컨텍스트를 비움. StateEnter() 참조.
        
        return _nextState;
    }

    public override void StateExit()
    {
        // 이동, 취소: 표시 끄기 → 목록 비우기
        _moveRangeDisplay.ClearHighlights();
        _availableSections.Clear();
    }
}
