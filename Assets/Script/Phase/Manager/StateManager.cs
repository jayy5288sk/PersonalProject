using System;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private MoveRangeDisplay _moveRangeDisplay;
    [SerializeField] private UnitSelectDisplay _unitSelectDisplay;
    
    private UnitSelectWait _unitSelectWait;
    private CommandSelect _commandSelect;
    private MoveSectionSelect _moveSectionSelect;
    private AttackTargetSelect _attackTargetSelect;
    private StateBase _currentState;
    private readonly StateContext _stateContext = new();
    private bool _isInputLocked; // 적 페이즈 중 플레이어 입력 차단
    
    public event Action<StateType> OnStateChanged;

    // HUD 버튼 활성화용. 컨텍스트 자체는 공개하지 않고 필요한 값만 읽기 전용으로 공개
    public bool IsSelectedUnitMoved => _stateContext.IsUnitMoved;

    private void Awake() => Init();

    private void Start() => ChangeNextState(_unitSelectWait);
    
    public void SectionClicked(Vector2Int section)
    {
        if (_isInputLocked)
        {
            return;
        }

        StateBase currentState = _currentState.SectionClick(section);
        if (currentState == null)
        {
            return;
        }
        
        ChangeNextState(currentState);
    }

    public void ButtonInputReceiver(CommandType command)
    {
        if (_isInputLocked)
        {
            return;
        }

        StateBase currentState = _currentState.PressButton(command);
        if (currentState == null)
        {
            return;
        }

        ChangeNextState(currentState);
    }

    /// <summary>
    /// 어느 상태에 있든 유닛 선택 대기로 되돌림. 현재 상태의 나가기와 대기 상태의 들어가기가 정리를 맡음
    /// </summary>
    public void ResetControlState()
    {
        ChangeNextState(_unitSelectWait);
    }

    public void SetInputLock(bool isLocked)
    {
        _isInputLocked = isLocked;
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
        _commandSelect = new CommandSelect(_stateContext, _unitSelectDisplay);
        _moveSectionSelect = new MoveSectionSelect(_stateContext, calculator,
                                                   _mapGenerator, _unitGenerator,
                                                   _moveRangeDisplay);
        _attackTargetSelect = new AttackTargetSelect(_stateContext);

        _unitSelectWait.SetStateBase(_commandSelect);
        _commandSelect.SetStateBase(moveState: _moveSectionSelect,
                                    attackState: _attackTargetSelect,
                                    unitSelectState: _unitSelectWait);
        _moveSectionSelect.SetStateBase(_commandSelect);
        _attackTargetSelect.SetStateBase(_commandSelect); // 임시 탈출 경로용
    }
}
