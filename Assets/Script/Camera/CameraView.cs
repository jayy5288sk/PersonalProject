using System;
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
    
    private float _screenMoveSpeed => _cameraMoveSpeed * Time.deltaTime;

    private void OnEnable() => BindEvents();
    private void OnDisable() => UnbindEvents();

    private void Update()
    {
        if (!_isMapGenerated)
        {
            return;
        }
        
        CameraMove();
    }

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
        
        _isMapGenerated = true;
    }

    private void CameraMove()
    {
        Vector2 cameraPosMove = _cameraMove.action.ReadValue<Vector2>();

        // Pivot 기준으로 입력을 월드 방향으로 변환
        Vector3 moveDirection = (transform.right * cameraPosMove.x) + 
                                (transform.forward * cameraPosMove.y);

        Vector3 currentPosition = transform.position;

        // 이동
        currentPosition += moveDirection * _screenMoveSpeed;

        // 제한
        currentPosition.x = Mathf.Clamp(currentPosition.x, _minCameraPos.x, _maxCameraPos.x);
        currentPosition.z = Mathf.Clamp(currentPosition.z, _minCameraPos.y, _maxCameraPos.y);

        transform.position = currentPosition;
    }
}
