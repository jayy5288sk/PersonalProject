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
        
        yield return unitRoutine;
    }
}
