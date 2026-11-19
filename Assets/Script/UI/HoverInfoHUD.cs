using System.Text;
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
    [SerializeField] private TextMeshProUGUI _hoverInfoText;

    private readonly StringBuilder _infoBuilder = new();
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

        _infoBuilder.Clear();
        _infoBuilder.Append(terrain.TileName).Append("  ");
        if (terrain.IsMoveAvailable)
        {
            _infoBuilder.Append("MOVE ").Append(terrain.RequireMove);
        }
        else
        {
            _infoBuilder.Append(IMPASSABLE_TEXT);
        }

        if (_unitGenerator.TryGetUnitAtSection(_lastSection, out MapUnit unit))
        {
            UnitData data = unit.SelectUnitData;
            _infoBuilder.Append('\n').Append(data.UnitName).Append(" [").Append(data.TeamSide).Append(']');
            _infoBuilder.Append('\n').Append("HP ").Append(unit.CurrentHp).Append(" / ").Append(data.MaxHealth);
            _infoBuilder.Append("   EN ").Append(unit.CurrentEn).Append(" / ").Append(data.MaxEnergy);
        }

        _hoverInfoText.SetText(_infoBuilder);
        _hoverInfoPanel.SetActive(true);
    }
}
