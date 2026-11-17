using UnityEngine;

public class StateContext
{
    public MapUnit SelectedUnit { get; private set; }
    public bool IsUnitMoved { get; private set; }
    
    public void PutUnit(MapUnit unit)
    {
        IsUnitMoved = false;
        SelectedUnit = unit;
    }
    
    public void MovedUnit()
    {
        IsUnitMoved = true;
    }
    
    public void EmptyStateContext()
    {
        SelectedUnit = null;
        IsUnitMoved = false;
    }
}
