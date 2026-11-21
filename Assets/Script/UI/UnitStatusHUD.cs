using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛 상태 창. 선택한 유닛(없으면 마우스가 가리키는 유닛)의 HP·EN·MV와 이동·공격 완료 여부를 표시
/// </summary>
public class UnitStatusHUD : MonoBehaviour
{
    private const string READY_TEXT = "READY";
    private const string DONE_TEXT = "DONE";
    private static readonly Vector2Int NO_SECTION = new(-1, -1);   // TileSelector가 맵 밖일 때 보내는 값

    [SerializeField] private TileSelector _tileSelector;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private GameObject _statusPanel;

    [Header("Gauge")]
    [SerializeField] private Image _hpFill;
    [SerializeField] private TextMeshProUGUI _hpValueText;
    [SerializeField] private Image _enFill;
    [SerializeField] private TextMeshProUGUI _enValueText;
    [SerializeField] private TextMeshProUGUI _moveValueText;

    [Header("Action")]
    [SerializeField] private Image _moveLamp;
    [SerializeField] private TextMeshProUGUI _moveStatusText;
    [SerializeField] private Image _attackLamp;
    [SerializeField] private TextMeshProUGUI _attackStatusText;
    [SerializeField] private Color _readyColor = new(0.45f, 1f, 0.85f, 1f);
    [SerializeField] private Color _doneColor = new(0.4f, 0.4f, 0.4f, 1f);

    private Vector2Int _lastSection = NO_SECTION;

    private void Start() => Refresh();

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _tileSelector.OnPointedSectionChanged += HandlePointedSectionChanged;
        _stateManager.OnStateChanged += HandleStateChanged;
        _phaseManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void UnbindEvents()
    {
        _tileSelector.OnPointedSectionChanged -= HandlePointedSectionChanged;
        _stateManager.OnStateChanged -= HandleStateChanged;
        _phaseManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    private void HandlePointedSectionChanged(Vector2Int section)
    {
        _lastSection = section;
        Refresh();
    }

    // 이동·공격이 끝나 상태가 바뀌면 같은 유닛이어도 다시 그림
    private void HandleStateChanged(StateType state) => Refresh();

    // 새 페이즈에서 행동 완료가 초기화되므로 다시 그림
    private void HandlePhaseChanged(PhaseType phase) => Refresh();

    private void Refresh()
    {
        if (!TryGetDisplayUnit(out MapUnit unit))
        {
            _statusPanel.SetActive(false);
            return;
        }

        ShowGauge(unit);
        ShowAction(unit);
        _statusPanel.SetActive(true);
    }

    // 선택한 유닛이 우선. 공격·대기 후에는 선택이 풀리므로 마우스가 가리키는 유닛으로 확인
    private bool TryGetDisplayUnit(out MapUnit unit)
    {
        unit = _stateManager.SelectedUnit;
        if (unit != null)
        {
            return true;
        }

        return _lastSection != NO_SECTION &&
               _unitGenerator.TryGetUnitAtSection(_lastSection, out unit);
    }

    private void ShowGauge(MapUnit unit)
    {
        UnitData data = unit.SelectUnitData;

        _hpFill.fillAmount = (float)unit.CurrentHp / data.MaxHealth;
        _hpValueText.text = $"{unit.CurrentHp} / {data.MaxHealth}";

        _enFill.fillAmount = (float)unit.CurrentEn / data.MaxEnergy;
        _enValueText.text = $"{unit.CurrentEn} / {data.MaxEnergy}";

        _moveValueText.text = $"{data.MovementPoints}";
    }

    // 이동 여부는 선택 중에만 알 수 있음. 행동을 끝내면 이동도 불가하므로 둘 다 완료로 표시
    private void ShowAction(MapUnit unit)
    {
        bool isActionDone = unit.IsUnitActionDone;
        bool isSelectedAndMoved = unit == _stateManager.SelectedUnit && _stateManager.IsSelectedUnitMoved;

        SetActionState(_moveLamp, _moveStatusText, isActionDone || isSelectedAndMoved);
        SetActionState(_attackLamp, _attackStatusText, isActionDone);
    }

    private void SetActionState(Image lamp, TextMeshProUGUI statusText, bool isDone)
    {
        lamp.color = isDone ? _doneColor : _readyColor;
        statusText.text = isDone ? DONE_TEXT : READY_TEXT;
    }
}
