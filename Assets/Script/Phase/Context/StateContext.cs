public class StateContext
{
    public MapUnit SelectedUnit { get; private set; }
    public bool IsUnitMoved { get; private set; }
    public WeaponData SelectedWeapon { get; private set; }

    public void PutWeaponToUnit(WeaponData weapon) => SelectedWeapon = weapon;
    
    public void PutUnit(MapUnit unit)
    {
        IsUnitMoved = false;
        SelectedUnit = unit;
    }
    
    public void AlreadyMovedUnit()
    {
        IsUnitMoved = true;
    }
    
    public void EmptyStateContext()
    {
        SelectedUnit = null;
        SelectedWeapon = null;
        IsUnitMoved = false;
    }
}
