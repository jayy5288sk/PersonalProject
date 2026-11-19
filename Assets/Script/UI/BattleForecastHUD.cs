using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BattleForecastHUD : MonoBehaviour
{
    private const string DEFEAT_MARK = "  DEFEAT";
    private static readonly Vector2Int NO_SECTION = new(-1, -1);   // TileSelector가 맵 밖일 때 보내는 값

    [SerializeField] private TileSelector _tileSelector;
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private GameObject _forecastPanel;
    [SerializeField] private TextMeshProUGUI _forecastText;

    private readonly List<MapUnit> _forecastTargets = new();
    private readonly StringBuilder _forecastBuilder = new();
    private Vector2Int _lastSection = NO_SECTION;

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _tileSelector.OnPointedSectionChanged += HandlePointedSectionChanged;
        _stateManager.OnStateChanged += HandleStateChanged;
    }

    private void UnbindEvents()
    {
        _tileSelector.OnPointedSectionChanged -= HandlePointedSectionChanged;
        _stateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandlePointedSectionChanged(Vector2Int section)
    {
        _lastSection = section;
        Refresh();
    }

    // 공격 대상 선택에 들어오면 바로 표시, 벗어나면 TryGetForecastTargets가 false라 자동으로 꺼짐
    private void HandleStateChanged(StateType state) => Refresh();

    // 대상 수집·데미지 계산은 실제 공격과 같은 코드(AttackTargetSelect, BattleCalculator)를 사용
    private void Refresh()
    {
        if (_lastSection == NO_SECTION ||
            !_stateManager.TryGetForecastTargets(_lastSection, _forecastTargets) ||
            _forecastTargets.Count == 0)
        {
            _forecastPanel.SetActive(false);
            return;
        }

        MapUnit attacker = _stateManager.SelectedUnit;
        WeaponData weapon = _stateManager.SelectedWeapon;

        _forecastBuilder.Clear();
        for (int i = 0; i < _forecastTargets.Count; i++)
        {
            MapUnit target = _forecastTargets[i];
            int damage = BattleCalculator.CalculateDamage(attacker, weapon, target);
            int afterHp = Mathf.Max(0, target.CurrentHp - damage);

            if (i > 0)
            {
                _forecastBuilder.Append('\n');
            }

            _forecastBuilder.Append(target.SelectUnitData.UnitName)
                            .Append("  HP ").Append(target.CurrentHp)
                            .Append(" → ").Append(afterHp)
                            .Append(" (-").Append(damage).Append(')');

            if (afterHp == 0)
            {
                _forecastBuilder.Append(DEFEAT_MARK);
            }
        }

        _forecastText.SetText(_forecastBuilder);
        _forecastPanel.SetActive(true);
    }
}
