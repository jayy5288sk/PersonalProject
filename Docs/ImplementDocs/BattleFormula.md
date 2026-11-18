## 문서명: `Battle Formula`

## 1. 구현 일자
- 일자: 2026-11-19 (목요일), 계산식 v1 (구현 상세: `Docs\ImplementDocs\261118FeatureWeaponAttack.md`)

## 2. Core Contents
- 구현 방향: 무기 TSV 시트 (공격력, 사거리, 범위 모양, EN 소모량) + 유닛 TSV 시트 (최대 HP·EN, 무장 ID) → `AttackRangeCalculator` (어느 칸을 때리나) → `BattleCalculator` (얼마나 아픈가) → `MapUnit` (HP·EN 반영)
- 목표: 전투 규칙(데미지, 범위, EN, HP)을 한 문서에 모아, 실제 공격·예상 전투 결과 UI·적 AI가 같은 규칙을 따르게 한다.
- 수치의 원본은 시트다. 이 문서는 식과 규칙만 적는다.

## 3. F-07 데미지 계산식
- What : **v1: `데미지 = 무장 공격력(AttackDamage)`**. 공격자·방어자 능력치, 지형, 명중·회피, 치명타는 반영하지 않는다
- What : 범위 무장으로 여러 적을 맞히면 **적마다** 같은 식으로 따로 계산한다
- What : 계산은 `BattleCalculator.CalculateDamage(attacker, weapon, defender)` 한 곳에서만 한다. 공격자·방어자는 보정식(v2 이후)을 위해 미리 받는다
- What : 예시 (가정 수치): 공격력 10 무장으로 HP 200인 적 공격 → 데미지 10, 남은 HP 190
- Where: `Assets/Script/Calculator/BattleCalculator.cs`
- Tune : 무장별 `AttackDamage` (무기 TSV 시트)

## 3. F-06, F-24 공격 범위
- What : 칸 거리 = 맨해튼 거리 `|dx| + |dy|` (대각선 이동 없음, 4방향 기준)
- What : 사거리 조건: `MinRange ≤ 칸 거리 ≤ MaxRange`. 맵 밖 칸은 제외. **지형과 유닛은 공격을 막지 않는다**
- What : 범위 모양 3종 (`AttackShape`)

  | 모양 | 고를 수 있는 칸 | 맞는 칸 |
  |---|---|---|
  | `Single` (단일) | 사거리 안 마름모 | 클릭한 칸 1개 |
  | `Area` (마름모) | 사거리 안 마름모 | 고를 수 있는 칸 **전부** (자기 주변 전원) |
  | `Pierce` (직선) | 상하좌우 4방향 직선의 `MinRange`~`MaxRange` 칸 | 클릭한 방향의 직선 전체 |

- What : 모양 그림 (`MinRange` 1, `MaxRange` 2, `U` = 공격자, `■` = 고를 수 있는 칸)
  ```
  Single / Area          Pierce
  . . ■ . .              . . ■ . .
  . ■ ■ ■ .              . . ■ . .
  ■ ■ U ■ ■              ■ ■ U ■ ■
  . ■ ■ ■ .              . . ■ . .
  . . ■ . .              . . ■ . .
  ```
  `MinRange`가 2 이상이면 가운데가 빈 고리 모양이 된다
- What : 맞는 칸 중 **공격자와 다른 진영**의 유닛만 대상. 빈 칸·같은 진영 칸은 맞아도 영향 없음
- Where: `Assets/Script/Calculator/AttackRangeCalculator.cs`, `Assets/Script/Unit/Enum/AttackShape.cs`, `Assets/Script/GameData/Data/WeaponData.cs` (`RangeShape` 문자열 변환)
- Tune : 무장별 `MinRange`, `MaxRange`, 범위 모양 문자열 `Single` / `Area` / `Pierce` (무기 TSV 시트)

## 3. F-06, F-07 EN 규칙
- What : 사용 조건: `CurrentEn ≥ RequireEn`일 때만 그 무장을 고를 수 있다. 부족하면 버튼 비활성 + 상태에서도 선택 거부
- What : 소모: 공격 **1회당 `RequireEn` 1번**. 맞힌 적의 수, 맞힌 적이 0명인지와 무관(단, 적이 없는 칸 클릭은 공격으로 치지 않아 소모 없음)
- What : 회복: 없음. 페이즈가 바뀌어도 행동 완료만 초기화되고 EN은 유지
- What : 예시 (가정 수치): EN 80, `RequireEn` 5 무장으로 적 2명 공격 → 남은 EN 75
- Where: `Assets/Script/Unit/MapUnit.cs` (`CanUseWeapon`, `ConsumeEnergy`)
- Tune : 무장별 `RequireEn` (무기 TSV 시트), 유닛별 최대 EN (유닛 TSV 시트)

## 3. F-07 HP 규칙
- What : 시작 값: 스폰 시 `CurrentHp = MaxHealth`, `CurrentEn = MaxEnergy`
- What : 감소: `CurrentHp = max(0, 반올림(CurrentHp - 데미지))`. EN도 같은 방식(`ConsumeEnergy`)
- What : 반올림: `Math.Round` 기본값(은행원 반올림, .5는 가까운 짝수로). v1은 정수 데미지만 있어 영향 없음
- What : HP 0: 0 아래로 내려가지 않는다. HP 0이 되면(`IsDefeated`) `AttackExecutor`가 `UnitGenerator.RemoveUnit`으로 맵에서 제거한다 (F-09 최소 구현)
- What : 회복: 없음
- Where: `Assets/Script/Unit/MapUnit.cs` (`TakeDamage`, `InitBattleStatus`, `IsDefeated`), `Assets/Script/Interface/`
- Tune : 유닛별 최대 HP (유닛 TSV 시트)

## 3. 데이터 출처
- What : 유닛 시트의 기본·확장 무장 열 = **무장 ID(`int`)**. 스폰 시 ID로 무기 데이터를 찾아 연결한다
- What : 무기 시트 열: ID, 이름, 공격력, 최소 사거리, 최대 사거리, 범위 모양, EN 소모량. 숫자 열은 `int.TryParse`로 읽는다
- Where: `Assets/Script/GameData/Loader/WeaponDataLoader.cs`, `Assets/Script/GameData/Loader/UnitDataLoader.cs`, `Assets/Script/ResourceGenerator/UnitGenerator.cs`
- Tune : 시트 URL (각 Loader Inspector)

## 4. 선택 이유
- 데미지 = 공격력 (v1): 일정상 가장 단순하고, 규칙이 하나뿐이라 문서화·검증 부담이 적다. 보정은 식을 한 곳에 모아 둔 뒤 추가한다.
- 계산을 한 곳에: 실제 공격, 예상 전투 결과 UI, 적 AI 점수가 같은 숫자를 써야 한다. 식이 여러 곳에 있으면 바뀔 때 어긋난다.
- 맨해튼 거리: 이동이 4방향이므로 공격 거리도 같은 기준으로 맞춘다. 칸 수를 세듯 직관적이다.
- 지형·유닛이 공격을 막지 않음: 시야·차폐 규칙 없이 사거리만으로 범위를 판정해 계산을 단순하게 유지한다.
- `Area` = 자기 주변 전원: 무기 데이터에 별도 폭발 반경 열이 없어, 사거리를 그대로 효과 범위로 쓴다.
- 진영은 "공격자와 다른가": 같은 규칙으로 적 AI가 아군을 공격할 수 있다.
- EN은 공격 1회당 1번: 범위 무장이 맞힌 적 수만큼 EN을 잃으면 범위 무장을 쓸 이유가 사라진다.
- HP·EN을 `float`로 받아 반올림: 이후 배율 보정(예: 지형 0.8배)이 들어와도 호출부를 바꾸지 않는다.
- 0 아래로 내려가지 않음: 음수 HP·EN이 UI와 판정에 섞이지 않게 한다.

## 5. Features 목록
| ID   | 기능          | 사용처                                      | 핵심 위치                      |
|------|---------------|---------------------------------------------|--------------------------------|
| F-07 | 데미지 계산식  | 실제 공격, 예상 전투 결과, 적 AI 점수의 공통 식 | `Assets/Script/Calculator/`    |
| F-06 | 공격 범위 규칙 | 사거리·모양 3종으로 고를 칸, 맞는 칸 판정       | `Assets/Script/Calculator/`    |
| F-07 | EN·HP 규칙    | 사용 조건, 1회 1번 소모, 0 하한, 회복 없음      | `Assets/Script/Unit/`          |

---

## TODO
- 방어·지형 보정식 v2 (F-17 숲 회피 보너스), ~~HP 0 격파 처리 (F-09)~~, 예상 전투 결과 표시 (F-07, 11/19 UI), ~~적 AI 점수 계산에 `BattleCalculator` 재사용 (F-08)~~, 시트에 소수 공격력이 들어오면 `int.TryParse` 실패로 그 행이 적용되지 않음 (시트 형식 확인 필요), EN·HP 회복 규칙 필요 여부
