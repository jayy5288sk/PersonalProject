using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "Scriptable Object/Unit/Unit Data")]
public class UnitData : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private string _unitName;
    [SerializeField] private int _maxHealth;
    [SerializeField] private int _maxEnergy;
    [SerializeField] private int _movementPoints;
    [SerializeField] private string _basicWeapon;
    [SerializeField] private string _expandedWeapon;
    
    public int ID => _id;
    public string UnitName => _unitName;
    public int MaxHealth => _maxHealth;
    public int MaxEnergy => _maxEnergy;
    public int MovementPoints => _movementPoints;
    public string BasicWeapon => _basicWeapon;
    public string ExpandedWeapon => _expandedWeapon;
    
    public void SetData(int id, string unitName, int maxHealth, int maxEnergy, 
                        int movementPoints, string basicWeapon, string expandedWeapon)
    {
        _id = id;
        _unitName = unitName;
        _maxHealth = maxHealth;
        _maxEnergy = maxEnergy;
        _movementPoints = movementPoints;
        _basicWeapon = basicWeapon;
        _expandedWeapon = expandedWeapon;
    }
}