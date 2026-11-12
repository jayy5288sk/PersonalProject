using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private InputActionReference _terrainOverlap;
    [SerializeField] private InputActionReference _terrainClick;
    [SerializeField] private float _groundHeight;
    
    private Camera _camera;
    private bool _isMapGenerated;
    private bool _isMapInside;
    private float _currentSection;
}
