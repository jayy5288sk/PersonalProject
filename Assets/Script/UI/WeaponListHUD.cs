using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponListHUD : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private GameObject _weaponPanel;
    [SerializeField] private Button[] _weaponButtons;
    [SerializeField] private TextMeshProUGUI[] _weaponTexts;     // _weaponButtons와 같은 순서
    [SerializeField] private WeaponDataHover[] _weaponHovers;    // _weaponButtons와 같은 순서
    [SerializeField] private GameObject _weaponInfoPanel;
    [SerializeField] private TextMeshProUGUI _weaponInfoText;

    private void OnEnable() => BindEvents();
    private void OnDisable() => UnBindEvents();

    private void BindEvents()
    {
        _stateManager.OnStateChanged += StateChanged;
        _weaponButtons[0].onClick.AddListener(BasicWeaponClicked);
        _weaponButtons[1].onClick.AddListener(ExpandedWeaponClicked);

        foreach (WeaponDataHover hover in _weaponHovers)
        {
            hover.OnHoverEnter += HandleWeaponHoverEnter;
            hover.OnHoverExit += HandleWeaponHoverExit;
        }
    }

    private void UnBindEvents()
    {
        _stateManager.OnStateChanged -= StateChanged;
        _weaponButtons[0].onClick.RemoveListener(BasicWeaponClicked);
        _weaponButtons[1].onClick.RemoveListener(ExpandedWeaponClicked);

        foreach (WeaponDataHover hover in _weaponHovers)
        {
            hover.OnHoverEnter -= HandleWeaponHoverEnter;
            hover.OnHoverExit -= HandleWeaponHoverExit;
        }
    }

    private void StateChanged(StateType state)
    {
        // 버튼이 사라질 때는 OnPointerExit가 오지 않을 수 있으므로 상태가 바뀌면 정보창을 항상 닫음
        _weaponInfoPanel.SetActive(false);

        bool isWeaponSelected = state == StateType.WeaponSelect;
        _weaponPanel.SetActive(isWeaponSelected);
        if (!isWeaponSelected)
        {
            return;
        }

        RefreshButtons();
    }

    private void BasicWeaponClicked()
    {
        _stateManager.WeaponInputReceiver(0);
    }

    private void ExpandedWeaponClicked()
    {
        _stateManager.WeaponInputReceiver(1);
    }

    // 버튼에는 이름만 표시. 수치는 호버 정보창에서 표시
    private void RefreshButtons()
    {
        IReadOnlyList<WeaponData> weapons = _stateManager.SelectedUnitWeapon;
        if (weapons == null)
        {
            return;
        }

        for (int i = 0; i < _weaponButtons.Length; i++)
        {
            bool isUsed = i < weapons.Count;
            _weaponButtons[i].gameObject.SetActive(isUsed);

            if(!isUsed)
            {
                continue;
            }

            _weaponTexts[i].text = weapons[i].WeaponName;
            _weaponButtons[i].interactable = _stateManager.CanSelectedUnitUseWeapon(i);
        }
    }

    // EN 부족으로 비활성인 버튼도 호버는 받으므로 수치 확인 가능
    private void HandleWeaponHoverEnter(int weaponIndex)
    {
        IReadOnlyList<WeaponData> weapons = _stateManager.SelectedUnitWeapon;
        if (weapons == null || weaponIndex >= weapons.Count)
        {
            return;
        }

        WeaponData weapon = weapons[weaponIndex];
        _weaponInfoText.text = $"{weapon.WeaponName}\n" +
                               $"RNG {weapon.MinRange}~{weapon.MaxRange}  ATK {weapon.AttackDamage}\n" +
                               $"EN {weapon.RequireEn}  Shape {weapon.RangeShape}";
        _weaponInfoPanel.SetActive(true);
    }

    private void HandleWeaponHoverExit()
    {
        _weaponInfoPanel.SetActive(false);
    }
}
