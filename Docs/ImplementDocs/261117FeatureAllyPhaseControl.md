## 문서명: `261117 Feature AllyPhaseControl`

## 1. 구현 일자
- 일자: 2026-11-17 (화요일) ~ 2026-11-18 (수요일)

## 2. Core Contents
- 구현 방향: `TileSelector` (칸 클릭) / `CommandMenuHUD` (버튼) → `StateManager` (조작 단계 상태 4개) ↔ `PhaseManager` (아군·적 페이즈) → `UnitSelectDisplay`, `MapUnit` (선택 표시, 행동 완료)
- 목표: 아군 클릭 → 선택 표시 + 커맨드 메뉴(이동/공격/대기) → 이동 후에는 공격/대기만 → 대기 시 행동 완료(어둡게, 재선택 불가) → 아군 전원 완료 또는 턴 종료 시 적 페이즈(임시로 즉시 복귀) → 아군 페이즈 시작 시 행동 초기화
- 구조: 페이즈(바깥) + 조작 단계(안쪽) 두 겹 상태. 상태끼리 공유하는 값은 `StateContext` 하나(선택한 유닛, 이동 여부)

## 3. F-04 조작 단계 상태 머신
- What : 상태는 `StateBase`를 상속한 순수 C# 클래스. 칸 클릭(`SectionClick`)과 버튼(`PressButton`)이 **다음 상태를 반환**하고, 전환이 없으면 `null`
- What : 전환은 `ChangeNextState` 한 곳에서만: 나가기 → 교체 → 들어가기 → `OnStateChanged(StateType)` 발행. 게임 시작 전환도 이 메서드로 한다
- What : `Awake`에서 상태를 모두 만든 뒤 연결 메서드(`SetStateBase`)로 다음 상태를 넣고, `Start`에서 유닛 선택 대기로 시작
- Where: `Assets/Script/Phase/Manager/StateManager.cs`, `Assets/Script/Phase/StateBase/StateBase.cs`, `Assets/Script/Phase/Context/StateContext.cs`, `Assets/Script/Phase/Enum/` (`StateType`, `CommandType`)

## 3. F-04 조작 단계 상태 4개
- What : `UnitSelectWait` — 들어가기: 선택 표시 끄기, 컨텍스트 비우기. 클릭: 행동 가능한 아군만 → 커맨드 선택
- What : `CommandSelect` — 들어가기: 선택한 유닛의 현재 위치로 선택 표시. `Move`(이동 전만) → 이동 칸 선택, `Attack` → 공격 대상 선택, `Wait` → 행동 완료 후 유닛 선택 대기. 맵 클릭: 이동 전 취소, 이동 후 무시
- What : `MoveSectionSelect` — 들어가기: 이동 범위 계산·표시. 이동과 취소 모두 커맨드 선택으로. 나가기: 표시 끄기
- What : `AttackTargetSelect` — 전환까지만 구현. **임시**: 아무 칸 클릭 시 커맨드 선택으로 복귀 (F-06에서 교체)
- Where: `Assets/Script/Phase/State/`

## 3. F-04 고른 유닛 표시, 행동 완료
- What : 바닥 테두리 + 머리 위 마커 한 벌을 `Awake`에서 만들어 끄고, 선택할 때 위치만 옮겨 켠다
- What : 행동 완료 = `IsUnitActionDone` + `material.color`를 원래 색 × 밝기(r, g, b만). 되돌리기 = 원래 색
- Where: `Assets/Script/Unit/UnitSelectDisplay.cs`, `Assets/Script/Unit/MapUnit.cs`, `Assets/Prefab/Unit/TempModel/SelectedUnitQuad.prefab`, `Assets/Prefab/Unit/TempModel/SelectedUnitPoint.prefab`
- Tune : 띄우는 높이 `_groundQuadHeight`, `_headMarkerHeight` (`UnitSelectDisplay` Inspector), 어둡기 `ACTION_DONE_BRIGHTNESS` (`MapUnit.cs`)

## 3. F-04 아군·적 페이즈 전환
- What : 페이즈는 `PhaseBase`를 상속한 `AllyPhase`, `EnemyPhase`. `PhaseEnter`가 다음 페이즈를 반환하면 `ChangePhase`가 현재 전환을 이벤트까지 끝낸 뒤 이어서 전환한다
- What : 첫 아군 페이즈는 `UnitGenerator.OnUnitPlacementCompleted`에서 시작
- What : 아군 페이즈 들어가기: 모든 아군 `ResetUnitAction` + `StateManager.ResetControlState`
- What : 적 페이즈 들어가기: 입력 잠금(`SetInputLock`) 후 **임시로** 즉시 아군 페이즈 반환 (F-08에서 적 AI 연결). 나가기: 잠금 해제
- What : `OnStateChanged`로 `UnitSelectStandBy`가 올 때마다 아군 전원 행동 완료인지 확인. 아군이 0명이면 완료로 보지 않는다
- Where: `Assets/Script/Phase/Manager/PhaseManager.cs`, `Assets/Script/Phase/PhaseState/`, `Assets/Script/Phase/StateBase/PhaseBase.cs`, `Assets/Script/ResourceGenerator/UnitGenerator.cs` (`OnUnitPlacementCompleted`, `GetUnitsByTeam`), `Assets/Script/Unit/TeamSide.cs`

## 3. F-04, F-10 커맨드 메뉴, 턴 종료 버튼 (최소 UI)
- What : `OnStateChanged` 구독: 커맨드 선택일 때만 패널 표시, 이동한 유닛은 이동 버튼 비활성. `OnPhaseChanged` 구독: 아군 페이즈에서만 턴 종료 버튼 활성
- What : 버튼은 `onClick.AddListener`로 연결해 `CommandType`을 `StateManager.ButtonInputReceiver`에 전달
- What : `TileSelector`는 마우스가 UI 위에 있으면 맵 클릭을 무시한다. UI 판정은 `Update`에서 미리 계산
- Where: `Assets/Script/UI/CommandMenuHUD.cs` (Canvas에 부착), `Assets/Prefab/UI/CommandUI.prefab`, `Assets/Script/Input/TileSelector.cs`

## 3. 좌표 변환 이전
- What : 칸 ↔ 월드 좌표 변환(`ReturnTerrainPosition`, `WorldToSection`, `IsInsideMap`)과 바닥 높이(`GroundHeight`)를 `TileSelector`에서 `MapGenerator`로 옮겼다. 바닥 위치만 돌려주고, 띄우는 높이는 쓰는 쪽이 더한다
- Where: `Assets/Script/ResourceGenerator/MapGenerator.cs`
- Tune : 바닥 높이 `_groundHeight` (`MapGenerator` Inspector)

## 4. 선택 이유
- 페이즈와 조작 단계를 두 겹으로: 바뀌는 시점이 다르다(페이즈는 턴 단위, 조작 단계는 클릭 단위). 한 겹이면 적 페이즈에도 조작 상태가 섞인다.
- 상태를 순수 C# 클래스로: `MonoBehaviour`는 `new`와 생성자를 쓸 수 없다. 상태마다 필요한 도구만 생성자로 받아 의존이 드러난다.
- 상태는 다음 상태만 반환하고 전환은 관리자 혼자: 나가기 → 들어가기 → 이벤트 순서를 한 곳에서 보장한다.
- 관리자가 생성과 연결을 맡음: 상태끼리 서로를 만들지 않는다. 상태를 필드로 들고 있어 턴 종료 시 `ResetControlState`로 바로 되돌릴 수 있다.
- 이벤트 하나 + `StateType`: HUD, 페이즈 관리자, 로그가 같은 채널 하나를 구독한다.
- 정리는 유닛 선택 대기의 들어가기 한 곳에서: 이동, 취소, 대기, 턴 종료 어떤 길로 와도 표시와 컨텍스트가 정리된다. 표시 켜기는 커맨드 선택의 들어가기에서 하므로 이동 후 위치 갱신도 함께 된다.
- 이동 취소 없음 + 이동 후 맵 클릭 무시: 이동한 유닛은 공격이나 대기로 끝내야 행동 완료 누락이 없다. 이동 후 `Move`는 HUD와 상태 양쪽에서 막는다(화면만 막으면 다른 입력 경로에서 뚫림).
- 이동 칸 선택의 취소도 커맨드 선택으로: 다음 상태가 하나로 유지되어 연결이 단순하다.
- 커맨드 선택의 연결은 이름 붙인 인수로: 다음 상태 3개가 모두 `StateBase`라 순서가 틀려도 컴파일러가 잡지 못한다.
- `material` + 원래 색 저장: `material`은 유닛별 복사본이라 그 유닛만 바뀐다. 원래 색 기준으로 계산해 여러 번 불러도 결과가 같다.
- HUD에는 `StateContext` 대신 `IsSelectedUnitMoved`만 공개: 쓰기 메서드가 바깥에 노출되지 않는다.
- 버튼은 `AddListener`: Inspector OnClick으로는 `enum`을 넘길 수 없고, 코드 연결이 타입 안전하다.
- UI 판정을 `Update`에서: 입력 콜백 안에서 `IsPointerOverGameObject`를 부르면 Input System 경고가 난다(DoD 10).
- 페이즈 들어가기가 다음 페이즈를 반환: 들어가기 안에서 직접 전환하면 전환이 끝나기 전에 다시 불려 이벤트 순서가 뒤집힌다.
- 첫 페이즈는 배치 완료 이벤트에서: 데이터 로딩이 코루틴이라 `Start` 시점에는 아군이 없다.
- 아군 목록은 필요할 때 유닛 사전에서 고름: 격파로 유닛이 줄어도 사전 한 곳만 관리하면 된다. 결과 리스트는 호출하는 쪽이 재사용한다.
- 입력 잠금은 `StateManager`에: 칸 클릭과 버튼 두 입구를 한 곳에서 막는다.

## 5. Features 목록
| ID   | 기능             | 사용처                                      | 핵심 위치                          |
|------|------------------|---------------------------------------------|------------------------------------|
| F-04 | 조작 단계 상태 머신 | 유닛 선택 → 커맨드 → 이동·공격 대상 상태 전환   | `Assets/Script/Phase/`             |
| F-04 | 고른 유닛 표시 2종  | 선택한 유닛의 칸 주위 테두리, 머리 위 마커      | `Assets/Script/Unit/`              |
| F-04 | 행동 완료 처리     | 대기 시 어둡게, 재선택 불가, 아군 페이즈에 초기화 | `Assets/Script/Unit/`              |
| F-04 | 아군·적 페이즈 전환 | 아군 전원 행동 완료 또는 턴 종료 시 적 페이즈     | `Assets/Script/Phase/`             |
| F-10 | 커맨드 메뉴·턴 종료 | 커맨드 선택 시 메뉴, 이동 후 이동 버튼 비활성     | `Assets/Script/UI/`                |

---

## TODO
- 공격 대상 선택 실제 처리와 임시 탈출 경로 교체 (F-06), 적 AI 연결과 적 페이즈 즉시 복귀 교체 (F-08), 아군 0명일 때 패배 처리 (F-09), 메뉴·버튼 외형과 페이즈 전환 중앙 UI (F-10), 선택 중 다른 아군 클릭 시 바로 전환, `TempLog` 출시 전 제거, `MoveSectionSelect` 하이라이트 리스트 재사용, 이름 정리 (`ButtonInputReceiver`, `AlreadyMovedUnit`, `UnitSelectStandBy`/`UnitSelectWait`), 프리팹 원본(`StateManager`, `UnitSelectDisplay`, `PhaseManager`, `CommandUI`)의 씬 참조 칸이 비어 있음 (다른 씬 배치 시 재연결), `TileSelector`의 `(-1, -1)` 상수화
