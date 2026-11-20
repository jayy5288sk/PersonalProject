using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class BattleForecastHUD : MonoBehaviour
{
    private const string DEFEAT_MARK = "  DEFEAT";
    private const string ARROW = " → ";
    private const string TARGET_SEPARATOR = ", ";
    private const string NAME_GAP = "  ";
    private static readonly Vector2Int NO_SECTION = new(-1, -1);   // TileSelector가 맵 밖일 때 보내는 값

    [SerializeField] private TileSelector _tileSelector;
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private GameObject _forecastPanel;
    [SerializeField] private TextMeshProUGUI _labelText;   // 공격자 → 피격자
    [SerializeField] private TextMeshProUGUI _descText;    // 피격자가 받는 데미지

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

        RefreshLabel(attacker);
        RefreshDesc(attacker, _stateManager.SelectedWeapon);

        _forecastPanel.SetActive(true);
    }

    // 공격자 → 피격자1, 피격자2
    private void RefreshLabel(MapUnit attacker)
    {
        _forecastBuilder.Clear();
        _forecastBuilder.Append(attacker.SelectUnitData.UnitName).Append(ARROW);

        for (int i = 0; i < _forecastTargets.Count; i++)
        {
            if (i > 0)
            {
                _forecastBuilder.Append(TARGET_SEPARATOR);
            }

            _forecastBuilder.Append(_forecastTargets[i].SelectUnitData.UnitName);
        }

        _labelText.SetText(_forecastBuilder);
    }

    // 대상이 여럿이면 어느 줄이 누구인지 알 수 있도록 줄 앞에 이름을 붙임
    private void RefreshDesc(MapUnit attacker, WeaponData weapon)
    {
        bool hasMultipleTargets = _forecastTargets.Count > 1;

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

            if (hasMultipleTargets)
            {
                _forecastBuilder.Append(target.SelectUnitData.UnitName).Append(NAME_GAP);
            }

            _forecastBuilder.Append("HP ").Append(target.CurrentHp)
                            .Append(ARROW).Append(afterHp)
                            .Append(" (-").Append(damage).Append(')');

            if (afterHp == 0)
            {
                _forecastBuilder.Append(DEFEAT_MARK);
            }
        }

        _descText.SetText(_forecastBuilder);
    }
}
