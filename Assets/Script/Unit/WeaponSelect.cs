using UnityEngine;

public class WeaponSelect : StateBase
{
    private readonly StateContext _stateContext;

    private StateBase _targetState;
    private StateBase _cancelState;

    public WeaponSelect(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    public void SetStateBase(StateBase targetState, StateBase cancelState)
    {
        _targetState = targetState;
        _cancelState = cancelState;
    }

    public override StateType StateName()
    {
        return StateType.WeaponSelect;
    }

    public override void StateEnter()
    {
        Debug.Log($"유닛 {_stateContext.SelectedUnit.name}의 무장 선택");
    }

    public override StateBase SelectWeapon(int weaponIndex)
    {
        MapUnit unit = _stateContext.SelectedUnit;
        if (weaponIndex < 0 || weaponIndex >= unit.WeaponDataList.Count)
        {
            return null;
        }

        WeaponData weapon = unit.WeaponDataList[weaponIndex];
        if (!unit.CanUseWeapon(weapon))
        {
            return null;
        }
        
        _stateContext.PutWeaponToUnit(weapon);
        return _targetState;
    }

    public override StateBase SectionClick(Vector2Int section)
    {
        return _cancelState;
    }
}
