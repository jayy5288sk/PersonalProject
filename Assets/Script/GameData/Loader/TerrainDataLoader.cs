using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class TerrainDataLoader : MonoBehaviour
{
    [SerializeField] private string _terrainTsvUrl;

    // 데이터 열의 개수(ID, 지형 이름, 이동 가능 여부, 소모 이동값)
    private const int COLUMN_COUNT = 4;
    
    [SerializeField] private List<TerrainData> _terrains = new();
    private Dictionary<int, TerrainData> _terrainDataById = new();

    /// <summary>
    /// Dictionary인 _terrainDataByID에 등록
    /// </summary>
    private void Awake()
    {
        foreach (var terrain in _terrains)
        {
            if (terrain == null) 
            {
                continue;
            }

            if (_terrainDataById.ContainsKey(terrain.ID))
            {
                Debug.Log("무기 ID 중복");
                continue;
            }
            
            _terrainDataById.Add(terrain.ID, terrain);
        }
    }
    
    
}
