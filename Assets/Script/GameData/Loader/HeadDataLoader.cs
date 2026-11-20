using System;
using System.Collections;
using System.Collections.Generic; // TEMP: DebugAttackRange 삭제 시 함께 삭제
using UnityEngine;

public class HeadDataLoader : MonoBehaviour
{
    [SerializeField] private StageData[] _stages;
    
    [SerializeField] private UnitDataLoader _unitDataLoader;
    [SerializeField] private WeaponDataLoader _weaponDataLoader;
    [SerializeField] private TerrainDataLoader _terrainDataLoader;
    
    [SerializeField] private MapDesignDataLoader _mapDesignDataLoader;
    [SerializeField] private MapGenerator _mapGenerator;
    
    [SerializeField] private UnitSpawnDataLoader _unitSpawnDataLoader;
    [SerializeField] private UnitGenerator _unitGenerator;
    
    public int StageCount => _stages.Length; // 총 스테이지 수
    
    private void Awake() => LoadData();

    private void LoadData()
    {
        StartCoroutine(LoadExternalDataRoutine());
    }

    public IEnumerator LoadExternalDataRoutine()
    {
        StageData stage = _stages[StageSelection.SelectedStageIndex];
        
        Coroutine unitRoutine = StartCoroutine(_unitDataLoader.LoadUnitTable());
        Coroutine weaponRoutine = StartCoroutine(_weaponDataLoader.LoadWeaponTable());
        Coroutine terrainRoutine = StartCoroutine(_terrainDataLoader.LoadTerrainTable());
        Coroutine mapRoutine = StartCoroutine(_mapDesignDataLoader.LoadMapTable(stage.MapTsvUrl));
        Coroutine unitSpawnRoutine = StartCoroutine(_unitSpawnDataLoader.LoadUnitSpawnTable(stage.UnitSpawnTsvUrl));
        
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
