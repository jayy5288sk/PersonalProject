// 수정: 사용하지 않던 System 대신 List용 Generic 사용
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraView : MonoBehaviour
{
    // 추가: 0.05 거리 이내면 자동 이동 도착으로 처리
    private const float FOCUS_ARRIVE_SQR_DISTANCE = 0.0025f;

    [SerializeField] private MapGenerator _mapGenerator;
    [SerializeField] private InputActionReference _cameraMove;
    [SerializeField] private float _cameraMoveSpeed = 1f;

    // 추가: 자동 시점 이동에 필요한 참조와 이동 시간(초)
    [SerializeField] private UnitGenerator _unitGenerator;
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private EnemyAIController _enemyAIController;
    [SerializeField] private float _focusSmoothTime = 0.3f;

    private Vector2 _minCameraPos;
    private Vector2 _maxCameraPos;
    private bool _isMapGenerated;

    // 추가: 자동 시점 이동 상태와 아군 목록 재사용
    private readonly List<MapUnit> _allyUnits = new();
    private Vector3 _focusTarget;
    private Vector3 _focusVelocity;
    private bool _isFocusing;

    private float _screenMoveSpeed => _cameraMoveSpeed * Time.deltaTime;

    private void OnEnable() => BindEvents();
    private void OnDisable() => UnbindEvents();

    // 수정: 입력이 있으면 자동 이동을 취소하고 수동 이동, 입력이 없을 때만 자동 이동
    private void Update()
    {
        if (!_isMapGenerated)
        {
            return;
        }

        Vector2 moveInput = _cameraMove.action.ReadValue<Vector2>();
        if (moveInput != Vector2.zero)
        {
            _isFocusing = false;
            CameraMove(moveInput);
            return;
        }

        if (_isFocusing)
        {
            MoveToFocus();
        }
    }

    // 수정: 페이즈 변경, 적 행동 시작 이벤트 구독 추가
    private void BindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted += HandleMapGenerated;
        _phaseManager.OnPhaseChanged += HandlePhaseChanged;
        _enemyAIController.OnEnemyActionStarted += HandleEnemyActionStarted;

        _cameraMove.action.Enable();
    }

    // 수정: 페이즈 변경, 적 행동 시작 이벤트 구독 해제 추가
    private void UnbindEvents()
    {
        _mapGenerator.OnMapGenerateCompleted -= HandleMapGenerated;
        _phaseManager.OnPhaseChanged -= HandlePhaseChanged;
        _enemyAIController.OnEnemyActionStarted -= HandleEnemyActionStarted;

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

    // 추가: 아군 페이즈 시작 때만 이동력이 가장 높은 아군을 비춤. 이후 시점은 플레이어가 조작
    private void HandlePhaseChanged(PhaseType phase)
    {
        if (phase != PhaseType.Ally)
        {
            return;
        }

        if (TryGetFastestAlly(out MapUnit ally))
        {
            FocusOn(ally.transform.position);
        }
    }

    // 추가: 적 페이즈에는 행동을 시작하는 적을 비춤
    private void HandleEnemyActionStarted(MapUnit enemy)
    {
        FocusOn(enemy.transform.position);
    }

    // 추가: 이동력이 가장 높은 아군 찾기. 이동력이 같으면 먼저 찾은 유닛
    private bool TryGetFastestAlly(out MapUnit fastest)
    {
        _unitGenerator.GetUnitsByTeam(TeamSide.ALLY, _allyUnits);

        fastest = null;
        foreach (MapUnit unit in _allyUnits)
        {
            if (fastest == null ||
                unit.SelectUnitData.MovementPoints > fastest.SelectUnitData.MovementPoints)
            {
                fastest = unit;
            }
        }

        return fastest != null;
    }

    // 추가: 높이는 유지하고 맵 범위 안으로 제한한 지점을 자동 이동 목표로 설정
    private void FocusOn(Vector3 worldPosition)
    {
        Vector3 target = transform.position;
        target.x = worldPosition.x;
        target.z = worldPosition.z;

        _focusTarget = ClampToMap(target);
        _focusVelocity = Vector3.zero;
        _isFocusing = true;
    }

    // 수정: 입력값을 매개변수로 받고, 범위 제한은 ClampToMap으로 분리
    private void CameraMove(Vector2 moveInput)
    {
        // Pivot 기준으로 입력을 월드 방향으로 변환
        Vector3 moveDirection = (transform.right * moveInput.x) +
                                (transform.forward * moveInput.y);

        transform.position = ClampToMap(transform.position + moveDirection * _screenMoveSpeed);
    }

    // 추가: 목표 지점까지 부드럽게 이동. 일시정지 중에는 deltaTime이 0이라 함께 멈춤
    private void MoveToFocus()
    {
        transform.position = Vector3.SmoothDamp(transform.position, _focusTarget,
                                                ref _focusVelocity, _focusSmoothTime);

        if ((transform.position - _focusTarget).sqrMagnitude < FOCUS_ARRIVE_SQR_DISTANCE)
        {
            transform.position = _focusTarget;
            _isFocusing = false;
        }
    }

    // 추가: 수동, 자동 이동이 같은 맵 범위 제한을 사용
    private Vector3 ClampToMap(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, _minCameraPos.x, _maxCameraPos.x);
        position.z = Mathf.Clamp(position.z, _minCameraPos.y, _maxCameraPos.y);
        return position;
    }
}
