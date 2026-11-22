## 문서명: `261122 Feature SoundAndUnitStatus`

## 1. 구현 일자
- 일자: 2026-11-22 (일요일)

## 2. Core Contents
- 구현 방향: 볼륨 Slider → `SoundSettingsHUD.ApplyVolume()` (선형 → dB) → `MainMixer` Exposed Parameter(`BGMVolume`, `SFXVolume`). 값은 `PlayerPrefs`에 저장하고 시작 시 불러옴
- 구현 방향: `TileSelector` · `StateManager` · `PhaseManager` 이벤트 → `UnitStatusHUD.Refresh()` → 선택 유닛(없으면 마우스가 가리키는 유닛)의 HP · EN · MV와 이동 · 공격 완료 여부 표시
- 목표: 메인 메뉴 환경설정에서 BGM · SE 볼륨 조절, 다음 실행에도 유지 (F-13). 전투 중 유닛 상태와 남은 행동을 한눈에 확인

## 3. F-13 환경설정 (BGM · SE 볼륨)
- What : `SoundSettingsHUD` — 열기 · 닫기 버튼으로 설정 패널 표시. Slider 값을 `Mathf.Log10(값) × 20`으로 dB 변환해 Mixer에 적용. 0은 `MIN_VOLUME`(0.0001 = -80dB)으로 올려 음수 무한 방지
- What : 저장 — Slider를 움직일 때 `PlayerPrefs.SetFloat`, 패널을 닫을 때 `PlayerPrefs.Save()`. 시작 시 `SetValueWithoutNotify`로 Slider에 넣고 Mixer에 적용
- What : BGM — 메인 메뉴 `MainMenuBGM` 프리팹과 인게임 씬의 AudioSource(PlayOnAwake, Loop). 둘 다 출력이 `MainMixer`의 `BGM` 그룹
- Where: `Assets/Script/UI/SoundSettingsHUD.cs`, `Assets/Prefab/Sound/MainMixer.mixer`, `Assets/Prefab/UI/MainMenu/Option.prefab`, `Assets/Prefab/UI/MainMenu/MainMenuController.prefab`, `Assets/Prefab/UI/MainMenu/MainMenuBGM.prefab`
- Tune : Exposed Parameter 이름(`BGM_VOLUME_PARAM`, `SFX_VOLUME_PARAM`, 저장 키와 같음), 기본 볼륨 `DEFAULT_VOLUME`, BGM 클립(`Assets/Prefab/Sound/`)

## 3. F-07, F-10 유닛 상태창
- What : `UnitStatusHUD` — HP · EN 게이지(`Image.fillAmount` = 현재 / 최대)와 `현재 / 최대` 텍스트, 이동력 MV
- What : 이동 · 공격 램프 — READY / DONE 텍스트와 색. 공격은 `IsUnitActionDone`, 이동은 `IsUnitActionDone` 또는 (선택 중인 유닛이면서 `IsSelectedUnitMoved`)
- What : 표시 대상 — `StateManager.SelectedUnit`이 우선, 없으면 마지막으로 가리킨 칸의 유닛(`TryGetUnitAtSection`). 둘 다 없으면 패널 끄기
- What : 칸 변경(`OnPointedSectionChanged`), 상태 변경(`OnStateChanged`), 페이즈 변경(`OnPhaseChanged`) 때 다시 그림
- Where: `Assets/Script/UI/UnitStatusHUD.cs`, `Assets/Prefab/UI/InGame/UnitStatus.prefab`
- Tune : READY · DONE 색(`_readyColor`, `_doneColor`, Inspector)

## 4. 선택 이유
- Slider 값을 dB로 변환: Mixer 볼륨은 로그 단위(dB)라 선형 값을 그대로 넣으면 Slider 앞쪽 대부분이 거의 무음으로 들린다. 0은 Log10이 음수 무한이 되므로 최솟값으로 올린다.
- 볼륨을 AudioSource가 아닌 Mixer 그룹에서 조절: BGM · SE 소리가 늘어나도 그룹 출력만 연결하면 된다. 소스마다 볼륨을 찾아 바꿀 필요가 없다.
- Mixer 적용을 `Start`에서: `AudioMixer.SetFloat`은 `Awake`에서 호출하면 적용되지 않는다.
- `PlayerPrefs` 키를 Exposed Parameter 이름과 같게: 같은 값을 가리키는 이름이 두 벌 생기지 않는다.
- `Save()`는 닫을 때만: Slider를 움직이는 동안 매 프레임 디스크에 쓰지 않는다. `SetFloat`만으로도 실행 중에는 값이 유지된다.
- 불러올 때 `SetValueWithoutNotify`: 그냥 `value`에 넣으면 `onValueChanged`가 불려 불러온 값을 다시 저장한다.
- 상태창이 선택 유닛을 우선: 조작 중인 유닛의 정보가 마우스 위치 때문에 바뀌면 안 된다. 공격 · 대기 후에는 선택이 풀리므로 마우스가 가리키는 유닛으로 확인한다.
- 이동 완료는 선택 중에만 판단: 이동 여부는 `StateContext.IsUnitMoved`에만 있고 `MapUnit`에는 없다. 행동을 끝낸 유닛은 이동도 할 수 없으므로 함께 DONE으로 본다.
- 호버 정보창과 분리: 호버 정보창은 지형 + 유닛 요약, 상태창은 선택 유닛의 행동 가능 여부를 다룬다. 한 HUD에 두면 표시 대상 규칙(마우스 vs 선택)이 섞인다.

## 5. Features 목록
| ID   | 기능          | 사용처                                      | 핵심 위치                                  |
|------|---------------|---------------------------------------------|--------------------------------------------|
| F-13 | BGM · SE 볼륨  | 메인 메뉴 환경설정 Slider → Mixer dB, 저장 유지 | `Assets/Script/UI/`, `Assets/Prefab/Sound/` |
| F-13 | BGM 재생       | 메인 메뉴 · 인게임 BGM, Mixer BGM 그룹 출력    | `Assets/Prefab/UI/MainMenu/`, `Assets/Scenes/` |
| F-07 | 유닛 상태창     | 선택 · 가리킨 유닛 HP · EN · MV, 행동 완료 표시 | `Assets/Script/UI/`, `Assets/Prefab/UI/InGame/` |

## TODO
- (미검증) 볼륨 불러오기는 메인 메뉴의 `SoundSettingsHUD`에만 있어, 인게임 씬을 바로 실행하면 저장값이 적용되는지 확인 필요. SFX 그룹으로 출력하는 AudioSource는 아직 없음. 일시정지 중 소리 멈춤(`OnPauseChanged` 구독)은 미구현
