using System;
using UnityEngine;

[Serializable]
public class WeaponData
{
    [SerializeField] private int _id;
    [SerializeField] private string _weaponName;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _minRange;
    [SerializeField] private float _maxRange;
    [SerializeField] private string _rangeShape;
    [SerializeField] private int _requireEn;

    public int ID => _id;
    public string WeaponName => _weaponName;
    public float AttackDamage => _attackDamage;
    public float MinRange => _minRange;
    public float MaxRange => _maxRange;
    public string RangeShape => _rangeShape;
    public int RequireEn => _requireEn;

    public WeaponData(int id, string weaponName, float attackDamage, float minRange,
                      float maxRange, string rangeShape, int requireEn)
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
