## 문서명: `261118 Feature WeaponAttack`

## 1. 구현 일자
- 일자: 2026-11-18 (수요일) ~ 2026-11-19 (목요일)

## 2. Core Contents
- 구현 방향: 유닛 시트 무장 ID → `UnitGenerator` (스폰 시 `WeaponData` 연결) → `MapUnit` (무장 목록, 현재 HP·EN) / `CommandSelect` → `WeaponSelect` (`WeaponListHUD`) → `AttackTargetSelect` (`AttackRangeCalculator`, `AttackRangeDisplay`, `BattleCalculator`) → `UnitSelectWait`
- 목표: 공격 커맨드 → 무장 목록(EN 부족 무장은 선택 불가) → 무장 모양·사거리대로 빨간 범위 표시 → 범위 안 적 클릭 시 HP를 공격력만큼, EN을 요구량만큼 감소 → 행동 완료 후 유닛 선택 대기
- 구조: 데이터(무장 연결) / 계산(범위, 데미지) / 상태(무장 선택, 공격 대상 선택) / 화면(HUD, 범위 표시)을 분리. 계산 부품은 상태·화면을 모른다

## 3. F-01 유닛·무장 연결
- What : 유닛 시트의 기본·확장 무장 열을 무장 이름에서 **무장 ID(`int`)**로 변경. 스폰 시 `AddWeaponById`로 `WeaponDataLoader.TryGetWeaponById`(Dictionary)에서 찾아 연결. 못 찾으면 에러 로그 후 그 무장만 건너뜀
- What : `MapUnit.InitBattleStatus`가 무장 목록(`WeaponDataList`)을 채우고 `CurrentHp`, `CurrentEn`을 최댓값으로 초기화. 데미지·EN 소모는 `IDamageable.TakeDamage(float)`, `IEnergyUsable.ConsumeEnergy(float)`
- What : 무장 사용 가능 판정은 `MapUnit.CanUseWeapon` 한 곳(`CurrentEn >= RequireEn`). `WeaponSelect`와 HUD(`StateManager.CanSelectedUnitUseWeapon`)가 함께 쓴다
- Where: `Assets/Script/ResourceGenerator/UnitGenerator.cs`, `Assets/Script/Unit/MapUnit.cs`, `Assets/Script/GameData/Loader/WeaponDataLoader.cs`, `Assets/Script/GameData/Data/UnitData.cs`, `Assets/Script/Interface/`
- Tune : 유닛의 최대 HP·EN, 무장 ID (유닛 TSV 시트), 무장 수치 (무기 TSV 시트)

## 3. F-06 공격 범위 계산
- What : `WeaponData.RangeShape`가 시트 문자열을 `AttackShape`(`Single`, `Area`, `Pierce`)로 변환. 알 수 없는 값은 에러 로그 후 `Single`
- What : `GetAttackableSections` — 고를 수 있는 칸, `ApplyAttackSection` — 클릭했을 때 맞는 칸. 사거리 식과 모양별 칸 규칙은 `Docs\ImplementDocs\BattleFormula.md` 참고
- What : 맵 안 판정은 `Func<Vector2Int, bool>`(`MapGenerator.IsInsideMap`)로 받고, 결과는 호출하는 쪽 컬렉션에 채운다
- Where: `Assets/Script/Calculator/AttackRangeCalculator.cs`, `Assets/Script/Unit/Enum/AttackShape.cs`, `Assets/Script/GameData/Data/WeaponData.cs`
- Tune : 무장별 `MinRange`, `MaxRange`, 범위 모양 (무기 TSV 시트)

## 3. F-06 무장 선택 / 공격 대상 선택 상태
- What : `WeaponSelect` — 무장 버튼(`SelectWeapon(index)`)으로 무장을 고른다. 번호가 범위 밖이거나 EN이 부족하면 무시(`null`). 고르면 `StateContext.PutWeaponToUnit` 후 공격 대상 선택으로. 맵 클릭은 커맨드 선택으로 취소
- What : `AttackTargetSelect` — 들어가기: 공격 범위 계산·표시. 나가기: 표시 끄기. 클릭: 범위 밖 = 무장 선택으로 취소, 범위 안이지만 적 없음 = 무시(`null`), 적 있음 = 공격 후 유닛 선택 대기
- What : 무장 입력 통로는 `StateManager.WeaponInputReceiver(int)` → `StateBase.SelectWeapon(int)`. `CommandType`과 별도
- Where: `Assets/Script/Unit/WeaponSelect.cs`, `Assets/Script/Phase/State/AttackTargetSelect.cs`, `Assets/Script/Phase/Manager/StateManager.cs`, `Assets/Script/Phase/StateBase/StateBase.cs`, `Assets/Script/Phase/Context/StateContext.cs`, `Assets/Script/Phase/Enum/StateType.cs`

## 3. F-06 무장 목록 HUD, 공격 범위 표시 (최소 UI)
- What : `WeaponListHUD` — `OnStateChanged` 구독. `WeaponSelect`일 때만 패널을 켜고, 유닛의 무장 수만큼 버튼 표시, EN 부족 무장은 비활성. 씬에 고정 버튼 2개(기본·확장)를 `AddListener`로 연결
- What : `AttackRangeDisplay` — 빨간 칸(`AttackRangeHighlight`)을 필요한 개수만큼만 복제해 재사용(오브젝트 풀). `MoveRangeDisplay`와 같은 구조
- Where: `Assets/Script/UI/WeaponListHUD.cs`, `Assets/Script/Input/AttackRangeDisplay.cs`, `Assets/Script/Input/AttackRangeHighlight.cs`, `Assets/Prefab/Unit/AttackRangeDisplay.prefab`, `Assets/Prefab/Unit/TempModel/AttackRangeQuad.prefab`, `Material/AttackRange.mat`
- Tune : 띄우는 높이 `_prefabHeight` (`AttackRangeDisplay` Inspector)

## 3. F-07 공격 실행
- What : 계산 규칙(데미지식, 범위, EN, HP, 반올림)은 `Docs\ImplementDocs\BattleFormula.md` 참고
- What : 처리 순서: 맞는 칸 구하기 → 공격자와 다른 진영 유닛 모으기 → 대상마다 `BattleCalculator.CalculateDamage` → `TakeDamage` → `ConsumeEnergy` 1번 → `CompleteUnitAction` → 유닛 선택 대기 반환
- What : 아군 전원 완료 판정은 기존 `PhaseManager`가 `OnStateChanged`로 그대로 처리
- Where: `Assets/Script/Calculator/BattleCalculator.cs`, `Assets/Script/Phase/State/AttackTargetSelect.cs`, `Assets/Script/Unit/MapUnit.cs`

## 4. 선택 이유
- 무장 열을 ID로: 이름 검색은 순회가 필요하고 이름이 바뀌면 끊긴다. `TryGetUnitData`와 같은 Dictionary 방식으로 통일.
- 사용 가능 판정을 `MapUnit.CanUseWeapon` 한 곳에: HUD(버튼 비활성)와 상태(선택 거부)가 같은 식을 쓴다. 화면만 막으면 다른 입력 경로에서 뚫린다.
- 상태를 무장 선택 / 공격 대상 선택 둘로: 입력 종류(버튼 / 맵 클릭)와 취소 목적지가 다르다.
- 공격 대상 선택의 취소는 무장 선택으로: 한 걸음만 뒤로 가서 다른 무장으로 바로 바꿔 고를 수 있다.
- 무장 입력을 `CommandType`과 분리: 유닛마다 무장 개수가 다르고 번호를 넘겨야 한다.
- 범위 계산기가 맵 판정을 `Func`로 받음: `MapGenerator`·유닛·상태를 모르는 순수 계산으로 남아 적 AI(F-08)가 그대로 재사용한다.
- 고를 수 있는 칸 / 맞는 칸을 두 메서드로: `Area`, `Pierce`는 클릭한 칸과 맞는 칸이 다르다. `Pierce`의 맞는 칸은 `attackable.Contains`로 걸러 매개변수가 늘지 않는다.
- 범위 안 빈 칸 클릭은 무시: 범위 무장으로 노리다 잘못 눌러도 무장 선택부터 다시 하지 않는다. 범위 밖 클릭만 취소.
- EN 소모를 데미지 반복문 밖에: 범위 무장의 EN 중복 소모 방지.
- 행동 완료를 상태 반환 전에: 전환 직후 `PhaseManager`가 전원 완료를 확인하므로, 순서가 바뀌면 마지막 유닛이 미완료로 보인다.
- 데미지 계산을 `static` 클래스 한 곳에: 실제 공격, 예상 데미지 UI, 적 AI 점수가 같은 식을 쓴다. 기억할 값이 없는 순수 계산이라 객체·생성자 연결이 필요 없다. 공격자·방어자는 지형 보정(F-17)을 위해 미리 받는다.
- 무장 버튼은 씬 고정 2개: 무장 칸이 기본·확장 두 개로 고정이라 프리팹 복제·리스너 재연결이 필요 없다.
- 결과 컬렉션(`_attackSections`, `_hitSections`, `_targets`, `_highlightPosition`)을 필드로 두고 비워서 재사용: 클릭마다 새로 만들지 않는다.

## 5. Features 목록
| ID   | 기능             | 사용처                                        | 핵심 위치                          |
|------|------------------|-----------------------------------------------|------------------------------------|
| F-01 | 유닛·무장 연결     | 스폰 시 무장 ID로 무장 목록, 현재 HP·EN 설정       | `Assets/Script/ResourceGenerator/`, `Assets/Script/Unit/` |
| F-06 | 공격 범위 계산     | 모양 3종·사거리로 고를 칸, 맞는 칸 계산 (적 AI 재사용) | `Assets/Script/Calculator/`        |
| F-06 | 무장 선택 상태     | 무장 버튼 선택, EN 부족 거부, 맵 클릭 시 취소       | `Assets/Script/Unit/`              |
| F-06 | 공격 대상 선택 상태 | 범위 표시, 범위 밖 취소, 빈 칸 무시, 적 공격        | `Assets/Script/Phase/State/`       |
| F-06 | 무장 목록·범위 표시 | 무장 선택 중 버튼 패널, 공격 범위 빨간 칸          | `Assets/Script/UI/`, `Assets/Script/Input/` |
| F-07 | 공격 실행         | 범위 안 적에 데미지·EN 반영 후 행동 완료 (규칙: `BattleFormula.md`) | `Assets/Script/Phase/State/`, `Assets/Script/Calculator/` |
| F-24 | 범위 모양 3종      | 단일·마름모·직선 무장 구분                       | `Assets/Script/Calculator/`        |

---

## TODO
- ~~HP 0 유닛 격파·제거 (F-09)~~, 예상 전투 결과 UI와 무장 설명 툴팁 (11/19 UI), ~~적 AI에서 범위·데미지 계산기 재사용 (F-08)~~, 지형·방어 보정 계산식 (F-17), 확인용 로그(`WeaponSelect` 들어가기, 공격·EN 로그) 출시 전 정리, 시트 `AttackDamage`는 `float`·코드는 `int`, `WeaponSelect.cs` 위치(`Unit/`) 정리 여부, `WeaponListHUD`의 주석 처리된 `_weaponTexts` 정리
