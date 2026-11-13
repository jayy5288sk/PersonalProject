using UnityEngine;
using UnityEngine.InputSystem;

public class CameraView : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private InputActionReference _cameraMove;
    [SerializeField] private float _cameraMoveSpeed = 1f;
    
    private Vector2 _minCameraPos;
    private Vector2 _maxCameraPos;
    private bool _isMapGenerated;

    private void OnEnable() => BindEvents();
    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted += HandleMapGenerated;
        
        _cameraMove.action.Enable();
    }

    private void UnbindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted -= HandleMapGenerated;
        
        _cameraMove.action.Disable();
    }

    private void HandleMapGenerated()
    {
        float x = (_mapGenerator.ColumnCount - 1) * MapGenerator.TERRAIN_SIZE;
        float z = (_mapGenerator.RowCount - 1) * MapGenerator.TERRAIN_SIZE;
        
        _minCameraPos = new Vector2(0, 0);
        _maxCameraPos = new Vector2(x, z);
        
        Debug.Log($"{_maxCameraPos.x}, {_maxCameraPos.y}");
        _isMapGenerated = true;
    }
}
