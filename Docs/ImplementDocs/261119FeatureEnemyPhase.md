## 문서명: `261119 Feature EnemyPhase`

## 1. 구현 일자
- 일자: 2026-11-19 (목요일)

## 2. Core Contents
- 구현 방향: 스폰 TSV 시트 (`ID:행동코드`) → `UnitSpawnDataLoader` → `UnitGenerator` (`MapUnit.BehaviorType`) / `PhaseManager` → `EnemyPhase` → `EnemyAIController` (`MovementRangeCalculator`, `AttackRangeCalculator`, `BattleCalculator`, `AttackExecutor`) → `OnEnemyTurnCompleted` → `AllyPhase`
- 목표: 적 페이즈에 적이 한 기씩 대기 후 행동. (서는 칸 × 무장 × 대상) 조합 중 최고점으로 이동·공격, 조합이 없으면 돌격형은 접근·거점형은 대기. HP 0 유닛은 맵에서 제거
- 구조: 적 AI는 아군과 같은 계산기·공격 실행(`AttackExecutor`)을 쓰고, 상태머신(`StateManager`)은 거치지 않는다

## 3. F-08 행동 유형 (스폰 데이터)
- What : 스폰 칸 형식 `유닛ID` 또는 `유닛ID:행동코드`. `A` = 돌격형(`Assault`), `D` = 거점형(`Defend`). 생략 시 돌격형, 알 수 없는 코드는 에러 로그 후 돌격형
- What : 로더가 칸마다 `{ Id, Behavior }` JObject로 보관 (키 상수 `SPAWN_ID_KEY`, `SPAWN_BEHAVIOR_KEY`). `UnitGenerator`가 스폰 시 `MapUnit.SetBehaviorType`으로 전달
- Where: `Assets/Script/GameData/Loader/UnitSpawnDataLoader.cs`, `Assets/Script/ResourceGenerator/UnitGenerator.cs`, `Assets/Script/Unit/MapUnit.cs`, `Assets/Script/Unit/Enum/EnemyBehaviorType.cs`
- Tune : 적 칸의 행동코드 (스폰 TSV 시트)

## 3. F-08 적 턴 흐름
- What : `EnemyPhase.PhaseEnter` — 입력 잠금 → `EnemyAIController.StartEnemyTurn()` → `null` (머무름)
- What : 코루틴: 적 목록 복사(`TeamSide.ENEMY`) → 적마다 [대기 → 행동 → `CompleteUnitAction`] → 마지막 대기 → 적 전원 `ResetUnitAction` → `OnEnemyTurnCompleted`
- What : `PhaseManager`가 완료 이벤트를 받아, 현재 적 페이즈일 때만 아군 페이즈로 전환
- Where: `Assets/Script/AI/EnemyAIController.cs`, `Assets/Script/Phase/PhaseState/EnemyPhase.cs`, `Assets/Script/Phase/Manager/PhaseManager.cs`, `Assets/Script/Unit/TeamSide.cs`
- Tune : `_actionDelay` (`EnemyAIController` Inspector)

## 3. F-08 조합 점수와 행동 결정
- What : 서는 칸 후보 = 제자리 + 이동 가능 칸. 칸마다 EN으로 쓸 수 있는 무장 → 고를 수 있는 칸 중 상대 진영 유닛 칸을 클릭 후보로 → `ApplyAttackSection` → `CollectTargets`
- What : 점수 = Σ min(데미지, 대상 현재 HP) + 격파 수 × `_defeatScoreBonus`. 0점보다 큰 최고점 조합 선택, 동점이면 먼저 찾은 쪽(제자리 우선)
- What : 조합 있음 → 이동 후 공격 (돌격형·거점형 공통). 조합 없음 → 돌격형만 접근 이동, 거점형은 제자리
- What : 접근 이동: 아군마다 아군 칸에서 출발해 이동력 제한 없이 `GetReachableSections`(적 진영 규칙) → 이동 가능 칸 중 그 값이 가장 작은 칸으로 이동. 어느 아군에도 닿지 않으면 제자리
- Where: `Assets/Script/AI/EnemyAIController.cs`
- Tune : `_defeatScoreBonus` (`EnemyAIController` Inspector)

## 3. F-07, F-09 공격 실행 공통화, 격파
- What : `AttackExecutor.CollectTargets` — 맞는 칸 중 공격자와 다른 진영 유닛 모으기. `Execute` — 대상마다 데미지 → HP 0이면 `UnitGenerator.RemoveUnit` → EN 1회 소모
- What : `RemoveUnit` — 유닛 사전에서 제거 후 오브젝트 파괴. 이후 선택·공격·이동 차단 대상에서 빠진다
- What : `AttackTargetSelect`(아군)와 `EnemyAIController`(적)가 같은 메서드를 쓴다
- Where: `Assets/Script/Battle/AttackExecutor.cs`, `Assets/Script/Phase/State/AttackTargetSelect.cs`, `Assets/Script/ResourceGenerator/UnitGenerator.cs`

## 4. 선택 이유
- 행동 유형을 스폰 데이터에: 같은 기체도 배치마다 성향을 다르게 줄 수 있다. 시트를 따로 두면 두 시트의 좌표를 맞춰야 한다.
- 칸을 `{ Id, Behavior }` JObject로: 키 상수로 읽어, 칸에 값이 늘어도 읽는 쪽이 순서에 묶이지 않는다.
- 거점형 = "조합 없으면 대기": "이동+공격 범위 안에 아군 있음"은 "공격 조합 있음"과 같아 별도 판정이 필요 없다.
- 코루틴은 MonoBehaviour(`EnemyAIController`)에: 페이즈는 순수 C# 클래스라 코루틴을 돌릴 수 없고, `PhaseManager`에 넣으면 전환 관리와 AI 로직이 섞인다.
- 완료를 이벤트로, 첫 대기 뒤에: `StartCoroutine`은 첫 `yield`까지 바로 실행된다. 대기 없이 알리면 `ChangePhase` 반복 도중 다시 전환되어 이벤트 순서가 뒤집힌다.
- 적 목록을 먼저 복사: 이동·격파로 유닛 사전이 바뀌므로 사전을 직접 순회하지 않는다.
- 점수에 min(데미지, HP): 남은 HP보다 큰 데미지는 의미가 없다. 격파 보너스로 막타를 우선한다.
- 접근 거리를 아군 칸에서 출발한 다익스트라로: 벽·진입 불가 지형을 돌아가는 실제 거리를 쓴다. 계산기를 고치지 않고 출발점과 이동력만 바꿔 재사용한다.
- 공격 실행을 `AttackExecutor` 한 곳에: 격파 처리를 아군·적 두 곳에서 따로 관리하지 않는다. 기억할 값이 없어 `static`.
- `TeamSide.ENEMY` 상수: "아군이 아님"으로 판별하면 진영 값 오타가 적으로 처리되어 문제가 숨는다.

## 5. Features 목록
| ID   | 기능            | 사용처                                       | 핵심 위치                     |
|------|-----------------|----------------------------------------------|-------------------------------|
| F-08 | 적 행동 유형      | 스폰 칸 `ID:A`/`ID:D`로 돌격형·거점형 지정        | `Assets/Script/GameData/Loader/`, `Assets/Script/Unit/` |
| F-08 | 적 턴 흐름        | 적 한 기씩 대기 후 행동, 완료 시 아군 페이즈 복귀   | `Assets/Script/AI/`, `Assets/Script/Phase/` |
| F-08 | 적 AI 점수화      | 서는 칸×무장×대상 조합 중 최고점으로 이동·공격     | `Assets/Script/AI/`           |
| F-08 | 접근 이동         | 공격 불가 시 돌격형이 가장 가까운 아군 쪽으로 이동  | `Assets/Script/AI/`           |
| F-09 | 격파 처리         | HP 0 유닛을 사전에서 제거하고 오브젝트 파괴        | `Assets/Script/Battle/`, `Assets/Script/ResourceGenerator/` |

## TODO
- 아군 전멸 시 패배 처리 (F-09), 한 칸씩 이동 연출 (P2), 적 클릭 시 범위 표시 (F-22), 접근 이동이 아군 수만큼 다익스트라를 돌림 (아군이 많아지면 확인), 확인용 로그(`AttackExecutor`) 출시 전 정리, 유닛 시트의 적 진영 값이 `"Enemy"`인지 확인
