using UnityEngine;

public abstract class StateBase
{
    public abstract StateType StateName();
    
    public virtual void StateEnter()
    {
        return;
    }

    public virtual void StateExit()
    {
        return;
    }

    public abstract StateBase SectionClick(Vector2Int section);
    
    public virtual StateBase PressButton(CommandType type)
    {
        return null;
    }

    public virtual StateBase SelectWeapon(int weaponIndex)
    {
        return null;
    }
}
