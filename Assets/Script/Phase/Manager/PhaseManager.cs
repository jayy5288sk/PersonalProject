using System;
using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private EnemyAIController _enemyAIController;

    private AllyPhase _allyPhase;
    private EnemyPhase _enemyPhase;
    private PhaseBase _currentPhase;
    private readonly List<MapUnit> _allyUnits = new();

    public event Action<PhaseType> OnPhaseChanged;

    private void Awake() => Init();

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    /// <summary>
    /// 턴 종료 버튼. 아군 페이즈일 때만 적 페이즈로 넘김
    /// </summary>
    public void RequestTurnEnd()
    {
        if (_currentPhase != _allyPhase)
        {
            return;
        }

        ChangePhase(_enemyPhase);
    }

    private void BindEvents()
    {
        _unitGenerator.OnUnitPlacementCompleted += HandleUnitPlacementCompleted;
        _stateManager.OnStateChanged += HandleStateChanged;
        _enemyAIController.OnEnemyTurnCompleted += HandleEnemyTurnCompleted;
    }

    private void UnbindEvents()
    {
        _unitGenerator.OnUnitPlacementCompleted -= HandleUnitPlacementCompleted;
        _stateManager.OnStateChanged -= HandleStateChanged;
        _enemyAIController.OnEnemyTurnCompleted -= HandleEnemyTurnCompleted;
    }

    // 적 전원 행동 완료 시 아군 페이즈로 복귀
    private void HandleEnemyTurnCompleted()
    {
        if (_currentPhase != _enemyPhase)
        {
            return;
        }

        ChangePhase(_allyPhase);
    }

    // 유닛 배치가 끝나야 아군 목록이 채워지므로, 첫 아군 페이즈는 배치 완료 시점에 시작
    private void HandleUnitPlacementCompleted()
    {
        ChangePhase(_allyPhase);
    }

    // 유닛 선택 대기로 돌아올 때마다 아군 전원 행동 완료인지 확인
    private void HandleStateChanged(StateType state)
    {
        if (_currentPhase != _allyPhase || state != StateType.UnitSelectStandBy)
        {
            return;
        }

        if (IsAllAllyActionDone())
        {
            ChangePhase(_enemyPhase);
        }
    }

    private bool IsAllAllyActionDone()
    {
        _unitGenerator.GetUnitsByTeam(TeamSide.ALLY, _allyUnits);

        // 아군이 없으면 "전원 완료"로 보지 않음 (아군 ↔ 적 무한 전환 방지)
        if (_allyUnits.Count == 0)
        {
            return false;
        }

        foreach (MapUnit unit in _allyUnits)
        {
            if (!unit.IsUnitActionDone)
            {
                return false;
            }
        }

        return true;
    }

    // 들어가기가 다음 페이즈를 반환하면, 현재 전환을 이벤트까지 끝낸 뒤 이어서 전환
    private void ChangePhase(PhaseBase nextPhase)
    {
        while (nextPhase != null)
        {
            _currentPhase?.PhaseExit();
            _currentPhase = nextPhase;
            nextPhase = _currentPhase.PhaseEnter();
            OnPhaseChanged?.Invoke(_currentPhase.PhaseName());
        }
    }

    private void Init()
    {
        _allyPhase = new AllyPhase(_unitGenerator, _stateManager);
        _enemyPhase = new EnemyPhase(_stateManager, _enemyAIController);
    }
}
