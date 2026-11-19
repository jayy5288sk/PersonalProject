using System;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private MoveRangeDisplay _moveRangeDisplay;
    [SerializeField] private UnitSelectDisplay _unitSelectDisplay;
    [SerializeField] private AttackRangeDisplay _attackRangeDisplay;
    
    private UnitSelectWait _unitSelectWait;
    private CommandSelect _commandSelect;
    private MoveSectionSelect _moveSectionSelect;
    private AttackTargetSelect _attackTargetSelect;
    private WeaponSelect _weaponSelect;
    private StateBase _currentState;
    private readonly StateContext _stateContext = new();
    private bool _isInputLocked; // 적 페이즈 중 플레이어 입력 차단

    public IReadOnlyList<WeaponData> SelectedUnitWeapon 
           => _stateContext.SelectedUnit?.WeaponDataList;
    
    public event Action<StateType> OnStateChanged;

    // HUD 버튼 활성화용. 컨텍스트 자체는 공개하지 않고 필요한 값만 읽기 전용으로 공개
    public bool IsSelectedUnitMoved => _stateContext.IsUnitMoved;
    public MapUnit SelectedUnit => _stateContext.SelectedUnit;          // 예상 전투 결과용
    public WeaponData SelectedWeapon => _stateContext.SelectedWeapon;   // 예상 전투 결과용

    private void Awake() => Init();

    private void Start() => ChangeNextState(_unitSelectWait);
    
    public void SectionClicked(Vector2Int section)
    {
        if (_isInputLocked)
        {
            return;
        }

        StateBase nextState = _currentState.SectionClick(section);
        if (nextState == null)
        {
            return;
        }

        ChangeNextState(nextState);
    }

    public void ButtonInputReceiver(CommandType command)
    {
        if (_isInputLocked)
        {
            return;
        }

        StateBase nextState = _currentState.PressButton(command);
        if (nextState == null)
        {
            return;
        }

        ChangeNextState(nextState);
    }

    public void WeaponInputReceiver(int weaponIndex)
    {
        if (_isInputLocked)
        {
            return;
        }

        StateBase nextState = _currentState.SelectWeapon(weaponIndex);
        if (nextState == null)
        {
            return;
        }

        ChangeNextState(nextState);
    }

    /// <summary>
    /// 어느 상태에 있든 유닛 선택 대기로 되돌림. 현재 상태의 나가기와 대기 상태의 들어가기가 정리를 맡음
    /// 이미 대기 상태면 정리할 것이 없으므로 상태 변경 이벤트를 다시 보내지 않음 (페이즈 전환 중복 방지)
    /// </summary>
    public void ResetControlState()
    {
        if (_currentState == _unitSelectWait)
        {
            return;
        }

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
    
    public bool CanSelectedUnitUseWeapon(int weaponIndex)
    {
        MapUnit unit = _stateContext.SelectedUnit;
        if (unit == null || weaponIndex < 0 || weaponIndex >= unit.WeaponDataList.Count)
        {
            return false;
        }

        return unit.CanUseWeapon(unit.WeaponDataList[weaponIndex]);
    }

    /// <summary>
    /// 공격 대상 선택 상태일 때만 그 칸을 공격하면 맞을 적을 result에 채움. 그 외 상태면 false
    /// </summary>
    public bool TryGetForecastTargets(Vector2Int section, List<MapUnit> result)
    {
        if (_currentState != _attackTargetSelect)
        {
            result.Clear();
            return false;
        }

        return _attackTargetSelect.TryCollectTargetsAt(section, result);
    }

    private void Init()
    {
        MovementRangeCalculator moveCalculator = new MovementRangeCalculator();
        AttackRangeCalculator attackCalculator = new AttackRangeCalculator();
        _unitSelectWait = new UnitSelectWait(_stateContext, _unitGenerator, _unitSelectDisplay);
        _commandSelect = new CommandSelect(_stateContext, _unitSelectDisplay);
        _moveSectionSelect = new MoveSectionSelect(_stateContext, moveCalculator,
                                                   _mapGenerator, _unitGenerator,
                                                   _moveRangeDisplay);
        _attackTargetSelect = new AttackTargetSelect(_stateContext, 
                                                     attackCalculator,
                                                     _mapGenerator,
                                                     _unitGenerator,
                                                     _attackRangeDisplay);
        _weaponSelect = new WeaponSelect(_stateContext);
        
        _unitSelectWait.SetStateBase(_commandSelect);
        _commandSelect.SetStateBase(moveState: _moveSectionSelect,
                                    attackState: _weaponSelect,
                                    unitSelectState: _unitSelectWait);
        _weaponSelect.SetStateBase(targetState: _attackTargetSelect,
                                   cancelState: _commandSelect);
        _moveSectionSelect.SetStateBase(_commandSelect);
        _attackTargetSelect.SetStateBase(done: _unitSelectWait,
                                         cancel: _weaponSelect); 
    }
}
