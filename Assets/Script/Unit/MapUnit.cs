using UnityEngine;

public class MapUnit : MonoBehaviour
{
    [SerializeField] private UnitData _selectUnitData;
    [SerializeField] private Vector2Int _sectionUnitPos;
    
    public UnitData SelectUnitData => _selectUnitData;
    public Vector2Int SectionUnitPos => _sectionUnitPos;

    public void SetUnitData(UnitData unitData, Vector2Int sectionUnitPos)
    {
        _selectUnitData = unitData;
        _sectionUnitPos = sectionUnitPos;
    }
}
