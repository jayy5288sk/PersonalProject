using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectWait : StateBase
{
    private const string ALLY_TEAMSIDE = "Ally";

    private readonly StateContext _stateContext;
    private readonly UnitGenerator _unitGenerator;

    private StateBase _stateBase;

    public UnitSelectWait(StateContext stateContext, UnitGenerator unitGenerator)
    {
        _stateContext = stateContext;
        _unitGenerator = unitGenerator;
    }
    
    public void SetStateBase(StateBase stateBase)
    {
        _stateBase = stateBase;
    }

    public override StateType StateName()
    {
        return StateType.UnitSelectStandBy;
    }
    
    public override StateBase SectionClick(Vector2Int section)
    {
        // 유닛 유무 및 아군 판별
        bool isUnitExist = _unitGenerator.TryGetUnitAtSection(section, out MapUnit unit);
        if (!isUnitExist || unit.SelectUnitData.TeamSide != ALLY_TEAMSIDE)
        {
            return null;
        }
        
        _stateContext.PutUnit(unit);
        return _stateBase;
    }
}
