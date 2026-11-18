using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponListHUD : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private GameObject _weaponPanel;
    [SerializeField] private Button[] _weaponButtons;
   //[SerializeField] private TextMeshProUGUI[] _weaponTexts;

    private void OnEnable() => BindEvents();
    private void OnDisable() => UnBindEvents();

    private void BindEvents()
    {
        _stateManager.OnStateChanged += StateChanged;
        _weaponButtons[0].onClick.AddListener(BasicWeaponClicked);
        _weaponButtons[1].onClick.AddListener(ExpandedWeaponClicked);
    }

    private void UnBindEvents()
    {
        _stateManager.OnStateChanged -= StateChanged;
        _weaponButtons[0].onClick.RemoveListener(BasicWeaponClicked);
        _weaponButtons[1].onClick.RemoveListener(ExpandedWeaponClicked);
    }

    private void StateChanged(StateType state)
    {
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

            WeaponData weapon = weapons[i];
            _weaponButtons[i].interactable = _stateManager.CanSelectedUnitUseWeapon(i);
        }
    }
}
