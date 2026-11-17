using UnityEngine;
using UnityEngine.UI;

public class CommandMenuHUD : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private GameObject _commandPanel;
    [SerializeField] private Button _moveButton;
    [SerializeField] private Button _attackButton;
    [SerializeField] private Button _waitButton;
    [SerializeField] private Button _turnEndButton;

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _stateManager.OnStateChanged += HandleStateChanged;
        _phaseManager.OnPhaseChanged += HandlePhaseChanged;

        _moveButton.onClick.AddListener(HandleMoveClicked);
        _attackButton.onClick.AddListener(HandleAttackClicked);
        _waitButton.onClick.AddListener(HandleWaitClicked);
        _turnEndButton.onClick.AddListener(HandleTurnEndClicked);
    }

    private void UnbindEvents()
    {
        _stateManager.OnStateChanged -= HandleStateChanged;
        _phaseManager.OnPhaseChanged -= HandlePhaseChanged;

        _moveButton.onClick.RemoveListener(HandleMoveClicked);
        _attackButton.onClick.RemoveListener(HandleAttackClicked);
        _waitButton.onClick.RemoveListener(HandleWaitClicked);
        _turnEndButton.onClick.RemoveListener(HandleTurnEndClicked);
    }

    // 턴 종료 버튼은 아군 페이즈에서만 누를 수 있음
    private void HandlePhaseChanged(PhaseType phase)
    {
        _turnEndButton.interactable = phase == PhaseType.Ally;
    }

    // 커맨드 선택 상태일 때만 메뉴 표시, 이동한 유닛은 이동 버튼 비활성화
    private void HandleStateChanged(StateType state)
    {
        bool isCommandSelect = state == StateType.CommandSelect;
        _commandPanel.SetActive(isCommandSelect);

        if (!isCommandSelect)
        {
            return;
        }

        _moveButton.interactable = !_stateManager.IsSelectedUnitMoved;
    }

    private void HandleMoveClicked() => _stateManager.ButtonInputReceiver(CommandType.Move);

    private void HandleAttackClicked() => _stateManager.ButtonInputReceiver(CommandType.Attack);

    private void HandleWaitClicked() => _stateManager.ButtonInputReceiver(CommandType.Wait);

    private void HandleTurnEndClicked() => _phaseManager.RequestTurnEnd();
}
