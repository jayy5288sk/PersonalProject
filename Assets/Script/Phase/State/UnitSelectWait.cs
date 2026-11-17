using UnityEngine;

public class UnitSelectWait : StateBase
{
    private readonly StateContext _stateContext;
    private readonly UnitGenerator _unitGenerator;
    private readonly UnitSelectDisplay _unitSelectDisplay;

    private StateBase _nextState;

    public UnitSelectWait(StateContext stateContext, UnitGenerator unitGenerator, 
                          UnitSelectDisplay unitSelectDisplay)
    {
        _stateContext = stateContext;
        _unitGenerator = unitGenerator;
        _unitSelectDisplay = unitSelectDisplay;
    }
    
    public void SetStateBase(StateBase stateBase)
    {
        _nextState = stateBase;
    }

    public override StateType StateName()
    {
        return StateType.UnitSelectStandBy;
    }
    
    public override StateBase SectionClick(Vector2Int section)
    {
        // 유닛 유무 및 아군 판별
        bool isUnitExist = _unitGenerator.TryGetUnitAtSection(section, out MapUnit unit);
        if (!isUnitExist || unit.SelectUnitData.TeamSide != TeamSide.ALLY || 
            unit.IsUnitActionDone)
        {
            return null;
        }
        
        // 선택 표시는 CommandSelect.StateEnter()에서 켬
        _stateContext.PutUnit(unit);
        return _nextState;
    }
    
    public override void StateEnter()
    {
        _unitSelectDisplay.HideUnitIndicators();
        _stateContext.EmptyStateContext();
    }
}
