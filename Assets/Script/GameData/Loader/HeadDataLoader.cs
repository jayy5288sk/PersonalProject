using System;
using System.Collections;
using UnityEngine;

public class HeadDataLoader : MonoBehaviour
{
    [SerializeField] private UnitDataLoader _unitDataLoader;
    [SerializeField] private WeaponDataLoader _weaponDataLoader;
    
    private void Awake() => LoadData();

    private void LoadData()
    {
        StartCoroutine(LoadExternalDataRoutine());
    }

    public IEnumerator LoadExternalDataRoutine()
    {
        Coroutine unitRoutine = StartCoroutine(_unitDataLoader.LoadUnitTable());
        Coroutine weaponRoutine = StartCoroutine(_weaponDataLoader.LoadWeaponTable());
        
        yield return unitRoutine;
        yield return weaponRoutine;
    }
}
