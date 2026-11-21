using System;
using System.Collections.Generic;
using UnityEngine;

public class MapUnit : MonoBehaviour, IDamageable, IEnergyUsable
{
    [SerializeField] private UnitData _selectUnitData;
    [SerializeField] private Vector2Int _sectionUnitPos;

    private UnitActionTint _actionTint;

    private readonly List<WeaponData> _weaponDataList = new();
    
    public UnitData SelectUnitData => _selectUnitData;
    public Vector2Int SectionUnitPos => _sectionUnitPos;
    public bool IsDefeated => CurrentHp <= 0; // 체력이 0이면 유닛 비활성
    public bool CanUseWeapon(WeaponData weapon) 
        => CurrentEn >= weapon.RequireEn; // 잔존 에너지가 무장 소모 에너지보다 많아야 무장 사용 가능.
    public IReadOnlyList<WeaponData> WeaponDataList => _weaponDataList;
    
    public EnemyBehaviorType BehaviorType { get; private set; } // 적 AI만 사용
    public bool IsUnitActionDone { get; private set; }
    public int CurrentHp { get; private set; }
    public int CurrentEn { get; private set; }

    private void Awake() => Init();
    
    public void SetUnitData(UnitData unitData, Vector2Int sectionUnitPos)
    {
        _selectUnitData = unitData;
        _sectionUnitPos = sectionUnitPos;
    }

    public void SetBehaviorType(EnemyBehaviorType behaviorType)
    {
        BehaviorType = behaviorType;
    }

    public void CompleteUnitAction()
    {
        IsUnitActionDone = true;
        _actionTint.SetActionDone(true);
    }

    public void ResetUnitAction()
    {
        IsUnitActionDone = false;
        _actionTint.SetActionDone(false);
    }

    public void TakeDamage(float damage)
    {
        CurrentHp = Mathf.Max(0, (int)Math.Round(CurrentHp - damage));
    }

    public void ConsumeEnergy(float amount)
    {
        CurrentEn = Mathf.Max(0, (int)Math.Round(CurrentEn - amount));
    }

    private void Init()
    {
        _actionTint = GetComponent<UnitActionTint>();
    }
    
    // 시작 시, List 및 체력, 에너지 초기화
    public void InitBattleStatus(List<WeaponData> weapons)
    {
        _weaponDataList.Clear();
        _weaponDataList.AddRange(weapons);
        CurrentHp = _selectUnitData.MaxHealth;
        CurrentEn = _selectUnitData.MaxEnergy;
    }
}
