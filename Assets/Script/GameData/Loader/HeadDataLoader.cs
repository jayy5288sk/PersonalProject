using System;
using System.Collections;
using UnityEngine;

public class HeadDataLoader : MonoBehaviour
{
    [SerializeField] private UnitDataLoader _unitDataLoader;
    [SerializeField] private WeaponDataLoader _weaponDataLoader;
    [SerializeField] private TerrainDataLoader _terrainDataLoader;
    [SerializeField] private MapDesignDataLoader _mapDesignDataLoader;
    [SerializeField] private MapGenerator _mapGenerator;
    
    private void Awake() => LoadData();

    private void LoadData()
    {
        StartCoroutine(LoadExternalDataRoutine());
    }

    public IEnumerator LoadExternalDataRoutine()
    {
        Coroutine unitRoutine = StartCoroutine(_unitDataLoader.LoadUnitTable());
        Coroutine weaponRoutine = StartCoroutine(_weaponDataLoader.LoadWeaponTable());
        Coroutine terrainRoutine = StartCoroutine(_terrainDataLoader.LoadTerrainTable());
        Coroutine mapRoutine = StartCoroutine(_mapDesignDataLoader.LoadMapTable());
        
        yield return unitRoutine;
        yield return weaponRoutine;
        yield return terrainRoutine;
        yield return mapRoutine;
        
        _mapGenerator.GenerateMap(_mapDesignDataLoader.MapArray);
    }
}
