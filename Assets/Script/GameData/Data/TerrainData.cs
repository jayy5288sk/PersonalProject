using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTerrainData", menuName = "Scriptable Object/Terrain/Terrain Data")]
public class TerrainData : ScriptableObject
{
    [SerializeField] private int _id;
    [SerializeField] private string _tileName;
    [SerializeField] private bool _isMoveAvailable;
    [SerializeField] private int _requireMove;
    
    [SerializeField] private GameObject _tilePrefab;
    
    public int ID => _id;
    public string TileName => _tileName;
    public bool IsMoveAvailable => _isMoveAvailable;
    public int RequireMove => _requireMove;
    public GameObject TilePrefab => _tilePrefab;
    
    public void SetTerrainData(int id, string tileName, bool isMoveAvailable, int requireMove)
    {
        _id = id;
        _tileName = tileName;
        _isMoveAvailable = isMoveAvailable;
        _requireMove = requireMove;
    }
}
