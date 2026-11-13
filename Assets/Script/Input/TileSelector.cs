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
    {
        // 맵이 준비되기 전에는 계산할 바닥 평면이 없음
        if (!_isMapGenerated)
        {
            return;
        }

        UpdatePointedSection();
    }

    private void BindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted += HandleMapGenerated;

        _terrainOverlap.action.Enable();
        _terrainClick.action.Enable();
        _terrainClick.action.performed += HandleTileClick;
    }

    private void UnbindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted -= HandleMapGenerated;

        _terrainClick.action.performed -= HandleTileClick;
        _terrainClick.action.Disable();
        _terrainOverlap.action.Disable();
    }

    private void HandleMapGenerated()
    {
        // 평면은 끝없이 넓으므로 x, z는 아무 값이어도 같은 평면
        Vector3 groundPoint = new Vector3(0f, _groundHeight, 0f);
        _groundCheckPlane = new Plane(Vector3.up, groundPoint);
        _isMapGenerated = true;
    }

    // 클릭 시 Update에서 계산해 둔 현재 칸을 그대로 사용
    private void HandleTileClick(InputAction.CallbackContext context)
    {
        if (!_isMapInside)
        {
            return;
        }

        Debug.Log($"클릭한 곳: 행 {_currentSection.y + 1}, 열 {_currentSection.x + 1}");
    }

    // 마우스가 가리키는 칸을 계산하고, 칸이 바뀌었을 때만 하이라이트를 갱신
    private void UpdatePointedSection()
    {
        if (!TryGetPointedGroundPosition(out Vector3 groundPosition))
        {
            ClearPointedSection();
            return;
        }

        Vector2Int section = WorldToSection(groundPosition);

        if (!IsInsideMap(section))
        {
            ClearPointedSection();
            return;
        }

        // 이전 프레임과 같은 칸이면 갱신하지 않음
        if (section == _currentSection)
        {
            return;
        }

        _currentSection = section;
        _isMapInside = true;
        _groundHighlight.transform.position = SectionToHighlightPosition(section);
        _groundHighlight.SetActive(true);
    }

    // 마우스 화면 좌표 → 바닥 평면 위의 월드 좌표
    private bool TryGetPointedGroundPosition(out Vector3 groundPosition)
    {
        Vector2 screenPosition = _terrainOverlap.action.ReadValue<Vector2>();
        Ray ray = _camera.ScreenPointToRay(screenPosition);

        if (_groundCheckPlane.Raycast(ray, out float distance))
        {
            groundPosition = ray.GetPoint(distance);
            return true;
        }

        groundPosition = Vector3.zero;
        return false;
    }

    // 월드 좌표 → (열, 행). 칸 중심이 정수 좌표이므로 반올림
    private Vector2Int WorldToSection(Vector3 worldPosition)
    {
        int column = Mathf.RoundToInt(worldPosition.x / MapGenerator.TERRAIN_SIZE);
        int row = _mapGenerator.RowCount - 1 - Mathf.RoundToInt
                  (
                      worldPosition.z / MapGenerator.TERRAIN_SIZE
                  );

        return new Vector2Int(column, row);
    }

    private bool IsInsideMap(Vector2Int section)
    {
        bool isColumnInside = section.x >= 0 && section.x < _mapGenerator.ColumnCount;
        bool isRowInside = section.y >= 0 && section.y < _mapGenerator.RowCount;

        return isColumnInside && isRowInside;
    }

    // (열, 행) → 하이라이트를 놓을 월드 좌표
    private Vector3 SectionToHighlightPosition(Vector2Int section)
    {
        float x = section.x * MapGenerator.TERRAIN_SIZE;
        float y = _groundHeight + _highlightOffset;
        float z = (_mapGenerator.RowCount - 1 - section.y) * MapGenerator.TERRAIN_SIZE;

        return new Vector3(x, y, z);
    }

    // 맵 밖을 가리키게 된 순간에만 숨김 처리
    private void ClearPointedSection()
    {
        if (!_isMapInside)
        {
            return;
        }

        _isMapInside = false;
        _currentSection = new Vector2Int(-1, -1);
        _groundHighlight.SetActive(false);
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
