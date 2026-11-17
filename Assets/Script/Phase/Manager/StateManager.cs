using System;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private MoveRangeDisplay _moveRangeDisplay;
    [SerializeField] private UnitSelectDisplay _unitSelectDisplay;
    
    private UnitSelectWait _unitSelectWait;
    private MoveSectionSelect _moveSectionSelect;
    private StateBase _currentState;
    private readonly StateContext _stateContext = new();
    
    public event Action<StateType> OnStateChanged;

    private void Awake() => Init();

    private void Start() => ChangeNextState(_unitSelectWait);
    
    public void SectionClicked(Vector2Int section)
    {
        StateBase currentState = _currentState.SectionClick(section);
        if (currentState == null)
        {
            return;
        }
        
        ChangeNextState(currentState);
    }

    public void ButtonInputReceiver(CommandType command)
    {
        StateBase currentState = _currentState.PressButton(command);
        if (currentState == null)
        {
            return;
        }
        
        ChangeNextState(currentState);
    }
    
    private void ChangeNextState(StateBase nextState)
    {
        _currentState?.StateExit(); // null이어도 Pass
        _currentState = nextState;
        _currentState.StateEnter();
        // 상태 변경 채널에 구독자가 있으면, 새 상태의 이름표를 실어서 방송
        OnStateChanged?.Invoke(_currentState.StateName());
    }

    private void Init()
    {
        MovementRangeCalculator calculator = new MovementRangeCalculator();
        _unitSelectWait = new UnitSelectWait(_stateContext, _unitGenerator, _unitSelectDisplay);
        _moveSectionSelect = new MoveSectionSelect(_stateContext, calculator,
                                                   _mapGenerator, _unitGenerator,
                                                   _moveRangeDisplay);
        _unitSelectWait.SetStateBase(_moveSectionSelect);
        _moveSectionSelect.SetStateBase(_unitSelectWait);
    }
}
