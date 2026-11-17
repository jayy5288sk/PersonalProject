using System.Collections.Generic;

public class AllyPhase : PhaseBase
{
    private readonly UnitGenerator _unitGenerator;
    private readonly StateManager _stateManager;

    private readonly List<MapUnit> _allyUnits = new();

    public AllyPhase(UnitGenerator unitGenerator, StateManager stateManager)
    {
        _unitGenerator = unitGenerator;
        _stateManager = stateManager;
    }

    public override PhaseType PhaseName()
    {
        return PhaseType.Ally;
    }

    public override PhaseBase PhaseEnter()
    {
        // 모든 아군 행동 가능으로 되돌리기
        _unitGenerator.GetUnitsByTeam(TeamSide.ALLY, _allyUnits);
        foreach (MapUnit unit in _allyUnits)
        {
            unit.ResetUnitAction();
        }

        // 선택 중 턴이 끝났어도 표시·메뉴가 남지 않도록 조작 단계 초기화
        _stateManager.ResetControlState();
        return null;
    }
}
