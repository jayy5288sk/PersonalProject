using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Scriptable Object/Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private string _weaponName;
    [SerializeField] private int _attackDamage;
    [SerializeField] private int _minRange;
    [SerializeField] private int _maxRange;
    [SerializeField] private string _rangeShape;
    [SerializeField] private int _requireEn;

    public int ID => _id;
    public string WeaponName => _weaponName;
    public int AttackDamage => _attackDamage;
    public int MinRange => _minRange;
    public int MaxRange => _maxRange;
    public string RangeShape => _rangeShape;
    public int RequireEn => _requireEn;

    public void SetData(int id, string weaponName, int attackDamage, int minRange,
                        int maxRange, string rangeShape, int requireEn)
    {
        _id = id;
        _weaponName = weaponName;
        _attackDamage = attackDamage;
        _minRange = minRange;
        _maxRange = maxRange;
        _rangeShape = rangeShape;
        _requireEn = requireEn;
    }
}
