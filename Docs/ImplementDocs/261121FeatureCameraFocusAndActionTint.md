## 문서명: `261122 Feature CameraFocusAndActionTint`

## 1. 구현 일자
- 일자: 2026-11-22 (일요일)

## 2. Core Contents
- 구현 방향: `PhaseManager.OnPhaseChanged(Ally)` · `EnemyAIController.OnEnemyActionStarted(MapUnit)` → `CameraView.FocusOn()` → `SmoothDamp`로 대상 칸까지 자동 이동. WASD 입력이 있으면 자동 이동 취소
- 구현 방향: 적 턴 시작 시 적 목록을 이동력 내림차순으로 정렬 → 적마다 `OnEnemyActionStarted` → 대기 → 행동
- 구현 방향: `MapUnit.CompleteUnitAction()` / `ResetUnitAction()` → `UnitActionTint.SetActionDone(bool)` → 몸체 Renderer의 `_BaseColor`를 `MaterialPropertyBlock`으로 덮어쓰기 / 지우기
- 목표: 페이즈 시작과 적 행동 때 플레이어가 카메라를 찾아 옮기지 않아도 됨. 여러 Renderer로 된 메크 모델에서도 행동 완료 유닛이 어둡게 보임

## 3. F-02 카메라 자동 시점 이동
- What : 아군 페이즈 시작 — 이동력이 가장 높은 아군(같으면 먼저 찾은 유닛)을 비춤. 이후 시점은 플레이어가 조작
- What : 적 행동 시작 — 행동할 적을 비춤
- What : `FocusOn()` — 높이는 유지하고 X · Z만 대상 위치로, `ClampToMap()`으로 맵 범위 안에 맞춘 지점을 목표로 설정
- What : `Update()` — 입력이 있으면 `_isFocusing = false` 후 수동 이동, 입력이 없을 때만 `MoveToFocus()`. 목표와의 거리 제곱이 `FOCUS_ARRIVE_SQR_DISTANCE` 미만이면 도착 처리
- What : 수동 · 자동 이동이 같은 `ClampToMap()`을 사용
- Where: `Assets/Script/Camera/CameraView.cs`, `Assets/Prefab/Cameras/InGameCameraPivot.prefab`
- Tune : 자동 이동 시간 `_focusSmoothTime`(Inspector), 도착 판정 거리 `FOCUS_ARRIVE_SQR_DISTANCE`

## 3. F-08 적 행동 순서
- What : `EnemyAIController` — `GetUnitsByTeam(ENEMY)`로 복사한 목록을 `CompareByMovementDescending`(이동력 내림차순)으로 정렬 후 순회
- What : `OnEnemyActionStarted(MapUnit)` — 적마다 행동 대기(`_actionWait`) 전에 발생. 카메라가 먼저 이동하고 기존 대기 시간 뒤에 행동
- Where: `Assets/Script/AI/EnemyAIController.cs`
- Tune : 적 행동 간격 `_actionDelay`(Inspector)

## 3. F-04 행동 완료 표시
- What : `UnitActionTint` — `Awake`에서 자식까지 모든 Renderer를 모으되 `ParticleSystemRenderer` · `TrailRenderer`(VFX)는 제외
- What : `SetActionDone(true)` — 머티리얼마다 원래 `_BaseColor` × 밝기(알파는 유지)를 PropertyBlock에 넣음. `false` — 빈 블록을 넣어 덮어쓴 값을 지움
- What : `MapUnit` — `Renderer` · 원래 색 보관을 없애고 `GetComponent<UnitActionTint>()`만 가져옴(`InitColor`)
- Where: `Assets/Script/Unit/UnitActionTint.cs`, `Assets/Script/Unit/MapUnit.cs`, 유닛 프리팹(`UnitData.UnitPrefab`, `UnitActionTint` 부착)
- Tune : 행동 완료 밝기 `_actionDoneBrightness`(프리팹 Inspector, 0 ~ 1)

## 4. 선택 이유
- 자동 이동을 이벤트 구독으로: 카메라가 페이즈와 AI를 호출받는 쪽이 되면 `PhaseManager` · `EnemyAIController`가 카메라를 알아야 한다. 이벤트로 두면 카메라를 빼도 전투 코드는 그대로다.
- 아군 페이즈는 시작 때만 비춤: 유닛 선택마다 카메라가 움직이면 플레이어가 맞춰 둔 시점을 빼앗는다.
- 입력이 자동 이동보다 우선: 자동 이동 중에도 플레이어가 바로 시점을 잡을 수 있어야 한다.
- `SmoothDamp` 사용: 순간 이동은 어디로 옮겨졌는지 알기 어렵다. `deltaTime` 기반이라 일시정지(`timeScale = 0`) 중에는 함께 멈춘다.
- 높이 유지 · 범위 제한: 피벗 높이와 자식 카메라 기울기가 쿼터뷰를 만든다. 맵 가장자리 유닛을 비춰도 수동 이동과 같은 범위를 벗어나지 않는다.
- 적 정렬을 복사한 목록에서: 원본 유닛 사전 순서는 건드리지 않고, 정렬 기준은 비교 메서드 하나(`CompareByMovementDescending`)라 기준을 바꿀 때 그곳만 고친다.
- 이벤트를 대기 전에 발생: 카메라가 도착하는 동안 기존 `_actionDelay`가 지나므로 대기를 새로 만들지 않는다.
- `MaterialPropertyBlock` 사용: `renderer.material`에 접근하면 머티리얼 사본이 유닛마다 생긴다. 블록은 사본 없이 Renderer별 값만 덮어쓰고, 비우면 원래 색으로 돌아가 원래 색을 따로 보관하지 않아도 된다.
- 머티리얼마다 각자 색에 밝기를 곱함: 메크 모델은 부위별 머티리얼 색이 달라 한 색으로 칠하면 모양이 사라진다.
- 색 처리를 `MapUnit`에서 분리: `MapUnit`은 전투 상태(HP · EN · 행동 완료)만 다루고, 표현 방식이 바뀌어도 `UnitActionTint`만 고친다.

## 5. Features 목록
| ID   | 기능            | 사용처                                         | 핵심 위치                    |
|------|-----------------|------------------------------------------------|------------------------------|
| F-02 | 카메라 자동 이동  | 아군 페이즈 시작 · 적 행동 시 대상 유닛 비춤      | `Assets/Script/Camera/`      |
| F-08 | 적 행동 순서     | 적 턴에서 이동력 높은 적부터 행동                 | `Assets/Script/AI/`          |
| F-04 | 행동 완료 표시    | 행동 끝난 유닛 어둡게, 다음 페이즈에 원래 색       | `Assets/Script/Unit/`        |
