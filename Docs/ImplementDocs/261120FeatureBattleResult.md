## 문서명: `261120 Feature BattleResult`

## 1. 구현 일자
- 일자: 2026-11-20 (금요일)

## 2. Core Contents
- 구현 방향: `AttackExecutor` 격파 → `UnitGenerator.RemoveUnit` → `OnUnitDeath` → `BattleResultJudge` → `OnBattleResult` → `PhaseManager`(진행 중단) / `BattleResultHUD`(결과 패널)
- 구현 방향: `PhaseManager.OnPhaseChanged` → `TurnPhaseHUD` / `TileSelector.OnPointedSectionChanged` → `HoverInfoHUD`, `BattleForecastHUD` / `WeaponDataHover` → `WeaponListHUD`
- 목표: 적 전멸 시 승리, 아군 전멸 시 패배. 결과 후 게임 진행 중단, 결과 화면에 턴 수·격파 수·손실 수 표시. 인게임 UI 최소 구현 (턴·페이즈, 호버 정보창, 무장 정보, 예상 전투 결과, 턴 종료 버튼 정리)
- 구조: 일이 일어난 곳이 이벤트를 보내고 HUD가 구독한다. HUD 스크립트는 항상 켜진 오브젝트에 두고 패널만 끄고 켠다

## 3. F-09 승패 판정
- What : `RemoveUnit` — 유닛 사전에서 제거 → `OnUnitDeath(MapUnit)` 발송 → `Destroy`. 사전에서 먼저 빼야 받는 쪽이 남은 수를 셀 때 방금 격파된 유닛이 빠져 있다
- What : `BattleResultJudge` — 격파 진영별로 `EnemyKillCount` / `AllyDeathCount` 증가 → 남은 적 0명이면 `Victory`, 남은 아군 0명이면 `Defeat`로 `OnBattleResult` 발송. `_isGameOver`로 결과는 한 번만
- Where: `Assets/Script/Battle/BattleResultJudge.cs`, `Assets/Script/Phase/Enum/BattleResult.cs`, `Assets/Script/ResourceGenerator/UnitGenerator.cs`

## 3. F-09 결과 후 진행 중단
- What : `PhaseManager.SetBattleResult` — `_isGameOver = true` → `SetInputLock(true)` → `EnemyAIController.StopEnemyTurn()` (`StopAllCoroutines`)
- What : `RequestTurnEnd`, `HandleEnemyTurnCompleted`, `HandleStateChanged` 맨 앞에 `_isGameOver ||` 가드
- Where: `Assets/Script/Phase/Manager/PhaseManager.cs`, `Assets/Script/AI/EnemyAIController.cs`

## 3. F-10, F-11 턴 수, 턴·페이즈 표시, 결과 UI
- What : `PhaseManager.TurnCount` — `ChangePhase`에서 아군 페이즈로 바뀔 때 `OnPhaseChanged` **발송 전에** 1 증가
- What : `TurnPhaseHUD` — `OnPhaseChanged` 수신 시 `Turn N · Ally/Enemy Phase`
- What : `BattleResultHUD` — `OnBattleResult` 수신 시 패널 켜기, 제목(`Stage Clear` / `Defeated`)·턴·격파·손실 표시, 다음 스테이지 버튼은 승리 시에만. 재도전 = 현재 씬 다시 불러오기
- Where: `Assets/Script/UI/TurnPhaseHUD.cs`, `Assets/Script/UI/BattleResultHUD.cs`, `Assets/Prefab/UI/ResultPanel.prefab`, `Assets/Prefab/UI/TurnPhaseHUD.prefab`
- Tune : 표시 문구 (각 HUD의 `const`)

## 3. F-10 무장 목록 정리, 무장 정보 호버
- What : 무장 버튼에는 이름만 (`WeaponName`). 버튼에 붙인 `WeaponDataHover`가 `IPointerEnterHandler`/`IPointerExitHandler`로 `OnHoverEnter(무장 순번)` / `OnHoverExit` 발송
- What : `WeaponListHUD`가 수신해 정보창에 이름, 사거리, 공격력, EN, 범위 모양 표시. 상태가 바뀌면 정보창을 항상 닫음 (버튼이 사라질 때는 `OnPointerExit`가 오지 않을 수 있음)
- Where: `Assets/Script/UI/WeaponListHUD.cs`, `Assets/Script/UI/WeaponDataHover.cs`, `Assets/Prefab/UI/WeaponInfoPanel.prefab`
- Tune : 버튼별 `_weaponIndex` (Inspector)

## 3. F-10 호버 정보창
- What : `TileSelector.OnPointedSectionChanged(Vector2Int)` — 가리키는 칸이 바뀔 때만 발송, 맵 밖이면 `(-1, -1)`
- What : `HoverInfoHUD` — 지형(`TileName`, `RequireMove` 또는 `Impassable`) + 유닛(이름, 진영, HP, EN). 칸 변경 외에 `OnStateChanged`·`OnPhaseChanged`에도 마지막 칸으로 다시 그림
- Where: `Assets/Script/Input/TileSelector.cs`, `Assets/Script/UI/HoverInfoHUD.cs`, `Assets/Prefab/UI/HoverInfoPanel.prefab`

## 3. F-07, F-10 예상 전투 결과
- What : `AttackTargetSelect.TryCollectTargetsAt(칸, result)` — 범위 확인 → `ApplyAttackSection` → `CollectTargets`. `SectionClick`도 이 메서드를 사용
- What : `StateManager.TryGetForecastTargets` — 공격 대상 선택 상태일 때만 위 메서드 호출. `SelectedUnit`, `SelectedWeapon` 읽기 전용 공개
- What : `BattleForecastHUD` — 대상마다 `BattleCalculator.CalculateDamage`로 `이름  HP 현재 → 이후 (-데미지)`, 격파 시 `DEFEAT`
- Where: `Assets/Script/Phase/State/AttackTargetSelect.cs`, `Assets/Script/Phase/Manager/StateManager.cs`, `Assets/Script/UI/BattleForecastHUD.cs`, `Assets/Prefab/UI/ForecastPanel.prefab`

## 3. F-10 턴 종료 버튼 정리
- What : 조작 단계 초기화(`ResetControlState`)를 `AllyPhase.PhaseEnter`에서 `PhaseExit`로 이동. 선택 도중 턴 종료 시 적 턴 동안 메뉴·범위 표시가 남던 문제 해결
- What : `ResetControlState`는 이미 대기 상태면 아무것도 하지 않음
- Where: `Assets/Script/Phase/PhaseState/AllyPhase.cs`, `Assets/Script/Phase/Manager/StateManager.cs`

## 4. 선택 이유
- 판정을 `RemoveUnit` 이벤트에: 유닛 수가 줄어드는 곳은 격파뿐이고 아군·적 공격 모두 이곳을 지난다. 매 프레임 확인이나 페이즈 종료 시 확인보다 정확하고 즉시 판정된다.
- `BattleResultJudge` 별도 컴포넌트: `PhaseManager`에 넣으면 페이즈 전환 + 승패 + 통계로 비대해진다. 결과 UI는 이곳만 보면 된다.
- 결과 enum: `OnBattleResult(BattleResult.Defeat)`가 `bool`보다 읽기 쉽다. `new BattleResult()`는 0번 값(`Victory`)이므로 쓰지 않는다.
- `StopEnemyTurn()` 메서드: 바깥에서 `StopAllCoroutines()`를 직접 부르면 누가 적 턴을 끄는지 `EnemyAIController`만 봐서는 보이지 않는다.
- `TurnCount`를 프로퍼티로: 이벤트 형태(`Action<PhaseType>`)를 바꾸지 않아 기존 구독자(`CommandMenuHUD`)를 고치지 않는다. 이벤트보다 먼저 올려 HUD가 새 번호를 읽는다.
- HUD를 켜진 오브젝트에: 꺼진 오브젝트는 `OnEnable`이 불리지 않아 이벤트를 구독하지 못한다.
- 무장 수치를 호버 정보창에: 버튼은 이름만 두어 작게 유지한다. EN 부족으로 비활성인 버튼도 호버는 받으므로 못 쓰는 이유를 볼 수 있다.
- 정보창은 Raycast를 받지 않게(`CanvasGroup.blocksRaycasts` 끔 등): 받으면 정보창이 버튼을 덮어 진입·이탈이 반복되며 깜빡이고, 맵 정보창은 칸 클릭을 막는다.
- 칸 변경 이벤트를 `TileSelector`에: 가리키는 칸은 이미 여기서 계산하고, 칸이 바뀔 때만 실행되는 위치에서 보내 매 프레임 발송하지 않는다. 호버 정보창과 예상 결과가 같은 이벤트를 쓴다.
- 호버 정보창 갱신을 상태·페이즈 이벤트로: `Update()` 매 프레임 갱신은 문자열 할당이 생긴다. 공격·이동은 상태 변경으로 끝나므로 그때만 다시 그린다.
- 예상 결과의 대상 수집을 `SectionClick`과 공유: 예상과 실제가 같은 코드를 지나야 어긋나지 않는다. 결과 리스트는 호출하는 쪽이 넘겨 실제 공격용 `_targets`와 섞이지 않는다.
- `StateManager` 창구: 컨텍스트 자체는 숨기고 필요한 값만 읽기 전용으로 공개하는 기존 원칙을 따른다.
- 초기화를 페이즈 나가기로: 어떤 이유로 아군 페이즈가 끝나든 항상 정리된다. 이미 대기 상태면 건너뛰는 가드가 없으면, 전원 행동 완료로 전환하는 도중 상태 변경 이벤트가 다시 나가 적 페이즈가 두 번 시작된다.

## 5. Features 목록
| ID   | 기능            | 사용처                                        | 핵심 위치                     |
|------|-----------------|-----------------------------------------------|-------------------------------|
| F-09 | 승패 판정         | 격파 시 남은 진영 수로 승리·패배 1회 판정           | `Assets/Script/Battle/`, `Assets/Script/ResourceGenerator/` |
| F-09 | 결과 후 진행 중단   | 입력 잠금, 적 턴 중단, 이후 페이즈 전환 차단        | `Assets/Script/Phase/Manager/`, `Assets/Script/AI/` |
| F-11 | 결과 UI          | 승패 제목, 턴·격파·손실 수, 재도전                 | `Assets/Script/UI/`           |
| F-10 | 턴·페이즈 표시     | 페이즈 전환마다 `Turn N · 페이즈` 갱신             | `Assets/Script/UI/`, `Assets/Script/Phase/Manager/` |
| F-10 | 무장 정보 호버     | 무장 버튼 호버 시 사거리·공격력·EN 표시            | `Assets/Script/UI/`           |
| F-10 | 호버 정보창       | 가리키는 칸의 지형·유닛·HP·EN 표시                | `Assets/Script/UI/`, `Assets/Script/Input/` |
| F-07 | 예상 전투 결과     | 범위 안 대상 호버 시 실제 공격과 같은 계산으로 표시   | `Assets/Script/UI/`, `Assets/Script/Phase/` |
| F-10 | 턴 종료 정리       | 선택 도중 턴 종료 시 메뉴·범위 표시 정리            | `Assets/Script/Phase/`        |

## TODO
- 다음 스테이지·메인 메뉴 버튼 연결 (11/20 메인 메뉴·스테이지 선택), 페이즈 전환 중앙 UI (11/21), 호버 정보창이 적 턴 중 적 이동을 바로 반영하지 않음, 마우스가 UI 위여도 아래 칸 정보 표시, 확인용 `TempLog` 출시 전 정리, `BattleResultJudge.UnitDeath`·`WeaponListHUD.StateChanged`/`UnBindEvents` 이름을 `Handle~`/`Unbind~` 형식으로 통일
