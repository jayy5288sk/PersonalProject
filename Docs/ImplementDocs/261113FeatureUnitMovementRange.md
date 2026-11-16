## 문서명: `261113 Feature UnitMovementRange`

## 1. 구현 일자
- 일자: 2026-11-13 (금요일) ~ 2026-11-16 (월요일)

## 2. Core Contents
- 구현 방향: `MapGenerator` (지형 사전) + `UnitGenerator` (유닛 사전, `MapUnit`) → `TileSelector` (클릭 흐름) → `MovementRangeCalculator` (다익스트라) → `MoveRangeDisplay` (숫자 하이라이트) → `UnitGenerator.MoveUnitToSection` (이동)
- 목표: 아군 유닛을 클릭하면 갈 수 있는 칸마다 그 칸까지 드는 누적 이동력을 숫자로 표시하고, 숫자 칸을 클릭하면 그 칸으로 순간이동한다. 그 외 칸을 클릭하면 취소한다.
- 칸 좌표: `Vector2Int(열, 행)`, 0부터 시작. `TileSelector`의 `WorldToSection`과 같은 기준

## 3. F-05 유닛 칸 정보 (`MapUnit`, 유닛 사전)
- What : 유닛 생성 직후 `AddComponent<MapUnit>()`으로 `UnitData`와 현재 칸을 붙이고, `칸 → MapUnit` 사전에 등록한다
- What : 이동은 `MoveUnitToSection` 한 곳에서 사전 갱신, `MapUnit` 칸 갱신, 오브젝트 위치 이동을 한 번에 처리한다
- Where: `Assets/Script/Unit/MapUnit.cs`, `Assets/Script/ResourceGenerator/UnitGenerator.cs` (`UnitsBySection`, `TryGetUnitAtSection`, `MoveUnitToSection`)

## 3. F-05 지형 사전
- What : 지형 데이터를 찾은 칸만 `칸 → TerrainData` 사전에 등록한다. 사전에 없는 칸(맵 밖, Default 타일)은 진입 불가로 처리한다
- Where: `Assets/Script/ResourceGenerator/MapGenerator.cs` (`TerrainBySection`)
- Tune : 지형별 `RequireMove`, `IsMoveAvailable` (지형 TSV 시트)

## 3. F-05 이동 범위 계산 (다익스트라)
- What : 출발 칸, 이동력, 진영, 지형 사전, 유닛 사전을 받아 `칸 → 누적 비용` 사전을 돌려준다
- What : 비용 기준은 **들어가는 칸**의 `RequireMove`. 이동력을 넘으면 진입 불가
- What : 진입 불가 지형과 다른 진영 유닛 칸은 막는다. 같은 진영 유닛 칸은 지나갈 수 있지만 결과에서 뺀다. 출발 칸도 결과에서 뺀다
- Where: `Assets/Script/PathFinding/MovementRangeCalculator.cs` (순수 C# 클래스)
- Tune : 유닛 이동력 `MovementPoints` (유닛 TSV 시트)

## 3. F-05 숫자 하이라이트 표시
- What : `(월드 위치, 비용)` 튜플 목록을 받아 칸마다 누적 비용 숫자를 띄운다. 오브젝트는 끄기만 하고 재사용한다(오브젝트 풀)
- Where: `Assets/Script/Input/MoveRangeDisplay.cs` (개수, 재사용 관리), `Assets/Script/Input/MoveRangeHighlight.cs` (하이라이트 하나의 위치, 숫자, 켜기/끄기)
- Where: `Assets/Prefab/Map/MoveRangeIndicator.prefab` (TextMeshPro 3D), `Assets/Prefab/Map/MoveRangeDisplay.prefab`
- Tune : 숫자를 띄우는 높이 `_prefabHeight` (`MoveRangeDisplay` Inspector), 글자 크기 (`MoveRangeIndicator.prefab`)

## 3. F-05 클릭 흐름 (대기 / 선택)
- What : `_selectedUnit`이 비어 있으면 대기, 들어 있으면 선택 상태
- What : 대기 상태에서 아군 클릭 → 계산, 표시, 선택. 빈 칸과 적 클릭은 무시
- What : 선택 상태에서 숫자 칸 클릭 → 이동 후 대기. 그 외 칸(빈 칸, 자기 자신, 다른 아군, 적) 클릭 → 취소 후 대기. 맵 밖 클릭은 두 상태 모두 무시
- Where: `Assets/Script/Input/TileSelector.cs` (`HandleTileClick`, `TrySelectUnit`, `HandleSelectedStateClick`)

## 4. 선택 이유
- 칸 → 데이터 사전 2개(지형, 유닛): 칸 하나를 조회할 때 반복문 없이 바로 찾는다. 지형은 바뀌지 않고 유닛은 이동, 격파로 바뀌므로 소유자를 나눴다.
- `IReadOnlyDictionary`로 공개: 바깥에서 `Add`, `Remove`를 못 하게 막는다. 필드를 그대로 돌려주므로 유닛이 이동하면 계산기도 바로 바뀐 사전을 본다.
- 계산기를 순수 C# 클래스로, 입력 5개를 모두 매개변수로: MonoBehaviour와 씬에 묶이지 않아 적 AI(F-08)에서도 그대로 다시 쓸 수 있다. 값을 빠뜨리면 컴파일 오류가 난다.
- 결과 사전을 비용 표로 함께 사용: 사전 하나로 계산하고, 마무리에서 출발 칸과 아군 칸만 지운다.
- 이동 처리를 `MoveUnitToSection` 한 곳에: 사전, `MapUnit`, 위치 중 하나라도 빠지면 화면과 계산이 어긋난다. 나중에 한 칸씩 이동 연출(P2)로 바꿀 때도 이곳만 고친다.
- 상태 변수 없이 `_selectedUnit`으로 상태 판단: 같은 사실을 두 변수에 나눠 담으면 서로 어긋날 수 있다. 이동, 취소 때 `_destinationData`도 함께 비워 대기 상태 = 선택 유닛도 결과도 없음으로 맞췄다.
- 선택 상태의 클릭은 선택 갈래만 실행: 다른 아군을 클릭했을 때 바로 그 아군으로 바꾸는 기능은 F-04 상태 머신에서 다룬다.
- 숫자 하이라이트 오브젝트 풀: 자주 열고 닫아도 생성, 파괴가 반복되지 않는다. 표시 코드를 분리해 공격 범위(F-06)에서도 다시 쓸 수 있다.

## 5. Features 목록
| ID   | 기능             | 사용처                                      | 핵심 위치                          |
|------|------------------|---------------------------------------------|------------------------------------|
| F-05 | 칸별 지형·유닛 사전 | 칸 좌표로 지형, 유닛을 바로 조회             | `Assets/Script/ResourceGenerator/` |
| F-05 | 이동 범위 계산     | 다익스트라로 갈 수 있는 칸과 누적 비용 계산    | `Assets/Script/PathFinding/`       |
| F-05 | 이동 범위 표시     | 갈 수 있는 칸마다 누적 비용 숫자 표시          | `Assets/Script/Input/`             |
| F-05 | 유닛 선택·이동·취소 | 아군 클릭 → 숫자 칸 클릭 시 순간이동, 그 외 취소 | `Assets/Script/Input/`             |

---

## TODO
- 한 칸씩 이동 연출과 경로 기록 (P2), 대기/선택 상태를 F-04 상태 머신으로 이전, 선택 중 다른 아군 클릭 시 바로 전환 (F-04), `GenerateMap`·`PlacementUnit` 재호출 시 사전 키 중복 (맵 재시작 기능 때 사전 비우기), 이동력 10 이상일 때 숫자 글자 크기 조정, 격파 시 유닛 사전에서 제거 (F-07)
