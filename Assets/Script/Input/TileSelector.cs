using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private InputActionReference _terrainOverlap;
    [SerializeField] private InputActionReference _terrainClick;
    [SerializeField] private float _groundHeight = 0.75f; // 타일 윗면 높이
    [SerializeField] private float _highlightOffset = 0.01f; // 바닥에서 하이라이트를 띄우는 거리
    [SerializeField] private GameObject _highlightPrefab;
    
    private Camera _camera;
    private bool _isMapGenerated;
    private bool _isMapInside;
    private Vector2Int _currentSection = new(-1, -1); // 맵 어느 구역인지(열, 행)
    private Plane _groundCheckPlane; // 마우스 Ray 계산용 바닥 평면
    private GameObject _groundHighlight; // 지정된 프리팹(_highlightPrefab) 복사본 용도

    private void Awake() => Init();

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();
    
    private void Update()
    {}

    private void BindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted += Foo;
    }

    private void UnbindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted -= Foo;
    }

    private void Foo()
    {
        //_groundCheckPlane = new Plane(Vector3.up, );
        _isMapGenerated = true;
    }

    private void Init()
    {
        _camera = Camera.main;
        
        _groundHighlight = Instantiate(_highlightPrefab, 
                                       _highlightPrefab.transform.position, 
                                       _highlightPrefab.transform.rotation);
        
        _groundHighlight.SetActive(false);
    }
}