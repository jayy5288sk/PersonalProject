## 문서명: `261120 Feature MakeUI`

## 1. 구현 일자
- 일자: 2026-11-20 (금요일)

## 2. Core Contents
- 구현 방향: `MainMenuController` 스테이지 버튼 → `StageSelection.SelectedStageIndex` → `SceneManager.LoadScene(SceneIndex.BATTLE)` → `HeadDataLoader`가 `_stages[번호]`(`StageData`)의 맵·스폰 주소를 로더에 전달
- 구현 방향: 결과 패널 → 다음 스테이지(번호 순환) / 재도전 / 메인 메뉴, 일시정지 패널 → 재개 / 메인 메뉴. 시간 흐름은 `TimeManager`만 다루고 `OnPauseChanged`로 알림
- 목표: 메인 메뉴 → 스테이지 선택 → 전투 → 결과 → 다음 판 / 메뉴가 끊기지 않고 이어짐. 코드 수정 없이 `StageData` 에셋만으로 스테이지 교체 (F-03). 전투 중 일시정지
- 구조: 씬을 넘어 남겨야 하는 값(고른 스테이지)만 static에 둔다. 남으면 안 되는 값(`Time.timeScale`)은 씬 이동 전에 되돌린다

## 3. F-03 스테이지 데이터 분리
- What : `StageData` (SO) — 스테이지 이름, 맵 TSV 주소, 스폰 TSV 주소. 에셋 `Stage1`, `Stage2`
- What : `MapDesignDataLoader.LoadMapTable(string)`, `UnitSpawnDataLoader.LoadUnitSpawnTable(string)` — 주소를 매개변수로 받음. 로더 안의 주소 상수·Inspector 필드 제거
- What : `HeadDataLoader` — `_stages[StageSelection.SelectedStageIndex]`의 주소를 두 로더에 전달. `StageCount` 공개
- Where: `Assets/Script/GameData/Data/StageData.cs`, `Assets/ScriptableObject/Stage/`, `Assets/Script/GameData/Loader/`, `Assets/Prefab/GameData/DataLoader.prefab`
- Tune : 스테이지 순서와 목록 (`DataLoader.prefab`의 `_stages`), 각 에셋의 시트 주소

## 3. F-12 메인 메뉴, 스테이지 선택
- What : `StageSelection.SelectedStageIndex` (static) — 메인 메뉴에서 고른 번호를 전투 씬으로 전달. 기본값 0이라 전투 씬을 바로 실행하면 스테이지 1
- What : `SceneIndex` — `MAIN_MENU = 0`, `BATTLE = 1` (Build Profiles Scene List 순서와 일치)
- What : `MainMenuController` — 메인 패널 ↔ 스테이지 선택 패널 전환, 스테이지 버튼은 번호 기록 후 전투 씬 로드, 종료(에디터에서는 Play 중지). 버튼은 `AddListener`로 연결
- Where: `Assets/Script/Stage/`, `Assets/Script/Scene/MainMenuController.cs`, `Assets/Scenes/MainMenuScene.unity`, `Assets/Prefab/UI/MainMenu/`

## 3. F-11 결과 패널 버튼
- What : 다음 스테이지 — `(SelectedStageIndex + 1) % StageCount`로 마지막 다음은 스테이지 1. 재도전 — 번호 그대로 전투 씬 다시 로드. 메인 메뉴 — 메인 메뉴 씬 로드
- What : 세 버튼 모두 `LoadSceneWithTimeReset` → `TimeManager.Resume()` 후 로드
- Where: `Assets/Script/UI/BattleResultHUD.cs`

## 3. F-10 일시정지
- What : `TimeManager` — `Pause()` / `Resume()` / `TogglePause()`, `IsPaused`, `OnPauseChanged(bool)`. `OnBattleResult` 수신 후에는 `Pause()` 무시
- What : `PauseHUD` — Esc(`GameMap/Pause`)와 일시정지 버튼 입력을 `TimeManager`에 전달, `OnPauseChanged`로 패널 표시. 메인 메뉴 버튼은 `Resume()` 후 로드
- What : 패널 배경 Image는 Raycast를 받아 칸 클릭·뒤쪽 버튼을 막는다
- Where: `Assets/Script/Battle/TimeManager.cs`, `Assets/Script/UI/PauseHUD.cs`, `Assets/GameInputActions.inputactions`

## 3. F-10, F-07 인게임 UI 정리
- What : `HoverInfoHUD` — 문자열 한 줄 대신 지형 이름, 이동 비용, 유닛 이름, 진영, HP, EN 칸을 따로 둠. 유닛이 없으면 유닛 텍스트 4개를 개별로 끔
- What : `BattleForecastHUD` — 라벨 `공격자 → 피격자들`, 설명 `HP 현재 → 이후 (-데미지)`. 대상이 2명 이상이면 설명 줄 앞에 이름
- What : `TurnPhaseHUD` 턴·페이즈 텍스트 분리, `WeaponListHUD` 무장 정보창 항목별 텍스트
- Where: `Assets/Script/UI/`, `Assets/Prefab/UI/InGame/`

## 4. 선택 이유
- 스테이지 정보를 SO 에셋으로: F-03 "내부 코드 없이 스테이지 교체"를 만족한다. 주소는 에셋 한 곳에만 둔다.
- 로더가 주소를 매개변수로 받음: 로더는 어떤 스테이지인지 몰라도 되고, 선택 판단은 `HeadDataLoader` 한 곳에만 있다. 로더마다 `StageData`를 들면 같은 선택 로직이 두 곳에 생긴다.
- 씬 간 전달을 static으로: 씬을 바꾸면 오브젝트는 모두 사라진다. `DontDestroyOnLoad`는 메뉴 복귀 시 중복 생성 처리가 필요하고, `PlayerPrefs`는 설정 저장 용도와 섞인다. 값 하나라 static이 가장 단순하다.
- `StageCount`를 `HeadDataLoader`에서: 결과 HUD에 목록을 또 연결하면 두 곳을 같이 관리해야 한다. 개수만 물어본다.
- 다음 스테이지를 `%` 순환으로: 이슈 #22 결정. 마지막 스테이지에서 `+1`만 하면 배열 범위를 넘는다.
- `TimeManager` 분리: `timeScale`과 "결과 후 일시정지 금지"는 화면이 아닌 게임 규칙이다. 나중에 사운드가 `PauseHUD`가 아닌 `OnPauseChanged`를 구독하면 된다. 이름을 `GameManager`처럼 넓게 짓지 않아 다른 책임이 몰리지 않게 한다.
- 씬 이동 전 `Resume()`: `timeScale`은 씬을 바꿔도 남는다. 0인 채로 넘어가면 다음 판의 적 대기(`WaitForSeconds`)와 카메라(`deltaTime`)가 멈춘다.
- 버튼을 `AddListener`로: Inspector `On Click`은 메서드 이름이 씬에 문자열로 저장되어, 이름을 바꾸면 에러 없이 연결만 끊긴다. 기존 HUD와 방식을 맞춘다.
- 다른 프리팹 참조(`_headDataLoader`, `_timeManager`, `_battleResultJudge`)는 씬 인스턴스에서 연결: 프리팹 파일끼리는 서로를 참조할 수 없다. Apply to Prefab 하지 않는다.
- 호버 정보창 유닛 텍스트를 그룹 없이 개별로 끔: UI가 항목 추가 시 확장되는 구조라 그룹 오브젝트를 두면 오히려 배치가 불편하다. 꺼진 자식은 Layout Group 배치에서 빠진다.
- 예상 결과 패널은 고정 위치 하나: 범위 공격은 대상이 여럿이라 대상 칸마다 띄우면 패널 풀·위치 추적이 필요하다. 라벨·설명 분리만 적용했다.

## 5. Features 목록
| ID   | 기능            | 사용처                                        | 핵심 위치                     |
|------|-----------------|-----------------------------------------------|-------------------------------|
| F-03 | 스테이지 데이터    | `StageData` 에셋 주소로 맵·스폰 교체, 코드 수정 없음 | `Assets/Script/GameData/`, `Assets/ScriptableObject/Stage/` |
| F-12 | 스테이지 선택      | 메인 메뉴 → 번호 기록 → 전투 씬 로드              | `Assets/Script/Stage/`, `Assets/Script/Scene/` |
| F-12 | 메인 메뉴         | 패널 전환, 게임 종료                              | `Assets/Script/Scene/`        |
| F-11 | 결과 버튼         | 다음 스테이지(순환), 재도전, 메인 메뉴             | `Assets/Script/UI/`           |
| F-10 | 일시정지          | Esc·버튼으로 시간 정지, 재개, 메인 메뉴            | `Assets/Script/Battle/`, `Assets/Script/UI/` |
| F-10 | 호버 정보창 정리    | 항목별 텍스트, 유닛 없으면 유닛 항목 숨김           | `Assets/Script/UI/`           |
| F-07 | 예상 결과 정리     | 라벨 공격자 → 피격자, 설명 HP 변화                 | `Assets/Script/UI/`           |

## TODO
- 환경설정 패널과 BGM·SE 볼륨 + `PlayerPrefs` 저장 (F-13, 이슈 #22 DoD 3·9), 패널 3개가 되면 `ShowPanel(target)`로 통합, 통합 테스트 (DoD 10~12), `MainMenuController.PressedFirstButton`/`PressedSecondButton` 이름을 `Handle~Clicked`로 통일, `SHD_UI_TMP_SDF_Mobile.shader` include 수정은 `Assets/Imports/`(gitignore)라 로컬에만 있음
