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
    
    [SerializeField] private UnitSpawnDataLoader _unitSpawnDataLoader;
    [SerializeField] private UnitGenerator _unitGenerator;
    
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
        Coroutine unitSpawnRoutine = StartCoroutine(_unitSpawnDataLoader.LoadUnitSpawnTable());
        
        yield return unitRoutine;
        yield return weaponRoutine;
        yield return terrainRoutine;
        yield return mapRoutine;
        yield return unitSpawnRoutine;
        
        _mapGenerator.GenerateMap(_mapDesignDataLoader.MapArray);
        _unitGenerator.PlacementUnit(_unitSpawnDataLoader.UnitSpawnArray, 
                                     _mapDesignDataLoader.MapArray.Count);
    }
}
