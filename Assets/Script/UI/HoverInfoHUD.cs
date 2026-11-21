using TMPro;
using UnityEngine;

public class HoverInfoHUD : MonoBehaviour
{
    private const string IMPASSABLE_TEXT = "Impassable";
    private static readonly Vector2Int NO_SECTION = new(-1, -1);   // TileSelector가 맵 밖일 때 보내는 값

    [SerializeField] private TileSelector _tileSelector;
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private GameObject _hoverInfoPanel;
    [SerializeField] private TextMeshProUGUI _tileNameText;
    [SerializeField] private TextMeshProUGUI _moveCostText;
    [SerializeField] private TextMeshProUGUI _unitNameText;
    [SerializeField] private TextMeshProUGUI _teamText;
    [SerializeField] private TextMeshProUGUI _hpText;
    [SerializeField] private TextMeshProUGUI _enText;

    private Vector2Int _lastSection = NO_SECTION;

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

    // 같은 칸에 마우스를 둔 채 공격·이동이 끝나도 최신 정보로 다시 그림
    private void HandleStateChanged(StateType state) => Refresh();

    private void HandlePhaseChanged(PhaseType phase) => Refresh();

    private void Refresh()
    {
        if (_lastSection == NO_SECTION ||
            !_mapGenerator.TerrainBySection.TryGetValue(_lastSection, out TerrainData terrain))
        {
            _hoverInfoPanel.SetActive(false);
            return;
        }

        _tileNameText.text = terrain.TileName;
        _moveCostText.text = terrain.IsMoveAvailable ? $"{terrain.RequireMove}" : IMPASSABLE_TEXT;

        bool hasUnit = _unitGenerator.TryGetUnitAtSection(_lastSection, out MapUnit unit);
        SetUnitInfoActive(hasUnit);
        if (hasUnit)
        {
            UnitData data = unit.SelectUnitData;
            _unitNameText.text = data.UnitName;
            _teamText.text = data.TeamSide;
            _hpText.text = $"HP {unit.CurrentHp} / {data.MaxHealth}";
            _enText.text = $"EN {unit.CurrentEn} / {data.MaxEnergy}";
        }

        _hoverInfoPanel.SetActive(true);
    }

    // 유닛 정보 텍스트를 개별로 켜고 끔. 꺼진 텍스트는 Layout Group 배치에서 빠짐
    private void SetUnitInfoActive(bool isActive)
    {
        _unitNameText.gameObject.SetActive(isActive);
        _teamText.gameObject.SetActive(isActive);
        _hpText.gameObject.SetActive(isActive);
        _enText.gameObject.SetActive(isActive);
    }
}
