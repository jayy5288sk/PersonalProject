using UnityEngine;

public class CommandSelect : StateBase
{
    private readonly StateContext _stateContext;
    private readonly UnitSelectDisplay _unitSelectDisplay;

    private StateBase _moveState;
    private StateBase _attackState;
    private StateBase _unitSelectState;

    public CommandSelect(StateContext stateContext, UnitSelectDisplay unitSelectDisplay)
    {
        _stateContext = stateContext;
        _unitSelectDisplay = unitSelectDisplay;
    }

    // 다음 상태가 모두 StateBase 타입이므로, 호출 시 이름 붙인 인수로 순서 실수를 방지
    public void SetStateBase(StateBase moveState, StateBase attackState, StateBase unitSelectState)
    {
        _moveState = moveState;
        _attackState = attackState;
        _unitSelectState = unitSelectState;
    }

    public override StateType StateName()
    {
        return StateType.CommandSelect;
    }

    public override void StateEnter()
    {
        // 처음 선택, 이동 후, 이동 취소 후 모두 현재 위치로 표시 갱신
        _unitSelectDisplay.ShowUnitIndicators(_stateContext.SelectedUnit);
    }

    public override StateBase SectionClick(Vector2Int section)
    {
        // 이동 후에는 공격/대기로 행동을 끝내야 하므로 맵 클릭 무시
        if (_stateContext.IsUnitMoved)
        {
            return null;
        }

        // 이동 전 맵 클릭: 선택 취소
        return _unitSelectState;
    }

    public override StateBase PressButton(CommandType type)
    {
        switch (type)
        {
            case CommandType.Move:
                // 이동은 한 번만. HUD 버튼 비활성화와 별개로 로직에서도 막음
                if (_stateContext.IsUnitMoved)
                {
                    return null;
                }
                return _moveState;

            case CommandType.Attack:
                return _attackState;

            case CommandType.Wait:
                _stateContext.SelectedUnit.CompleteUnitAction();
                return _unitSelectState;

            default:
                return null;
        }
    }
}
