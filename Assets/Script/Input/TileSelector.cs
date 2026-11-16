using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TileSelector : MonoBehaviour
{
    private const string ALLY_TEAMSIDE = "Ally";
    
    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private MoveRangeDisplay _moveRangeDisplay;
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
    private MovementRangeCalculator _movementRangeCalculator;
    private MapUnit _selectedUnit;
    private Dictionary<Vector2Int, int> _destinationData;

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

        // 선택 상태: 선택 갈래만 실행하고 대기 갈래로 넘어가지 않음
        if (_selectedUnit != null)
        {
            HandleSelectedStateClick(_currentSection);
            return;
        }

        TrySelectUnit(_currentSection);
    }

    // 선택 상태의 클릭 처리. 숫자 칸이면 이동, 그 외 칸이면 취소
    private void HandleSelectedStateClick(Vector2Int section)
    {
        if (_destinationData.ContainsKey(section))
        {
            _unitGenerator.MoveUnitToSection(_selectedUnit, section);
        }

        // 이동, 취소 공통: 표시 끄기 → 대기 상태로 되돌리기
        _moveRangeDisplay.ClearHighlights();
        _selectedUnit = null;
        _destinationData = null;
    }

    private void TrySelectUnit(Vector2Int section)
    {
        // 유닛 유무 및 아군 판별
        bool isUnitExist = _unitGenerator.TryGetUnitAtSection(section, out MapUnit unit);
        if (!isUnitExist || unit.SelectUnitData.TeamSide != ALLY_TEAMSIDE)
        {
            return;
        }

        // 필요값 주입 및 결과값 저장
        _destinationData = _movementRangeCalculator.GetReachableSections
        (
            unit.SectionUnitPos,
            unit.SelectUnitData.MovementPoints,
            unit.SelectUnitData.TeamSide,
            _mapGenerator.TerrainBySection,
            _unitGenerator.UnitsBySection
        );

        // 변환 및 정보 목록 등록
        List<(Vector3 position, int cost)> highLightList = new();
        foreach (KeyValuePair<Vector2Int, int> pair in _destinationData)
        {
            Vector3 position = SectionToHighlightWorldPosition(pair.Key);
            highLightList.Add((position, pair.Value));
        }
        
        // 결과값 보여주기
        _moveRangeDisplay.ShowMoveRangeHighlight(highLightList);
        
        _selectedUnit = unit;
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
        _groundHighlight.transform.position = SectionToHighlightWorldPosition(section);
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
    private Vector3 SectionToHighlightWorldPosition(Vector2Int section)
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
        
        _movementRangeCalculator = new MovementRangeCalculator();
    }
}
