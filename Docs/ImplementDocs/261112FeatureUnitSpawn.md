## 문서명: `261112 Feature UnitSpawn`

## 1. 구현 일자
- 일자: 2026-11-12 (목요일)

## 2. Core Contents
- 구현 방향: 스폰 TSV 시트 (격자형, 칸 = 유닛 ID) → `UnitSpawnDataLoader` (TSV → `JArray`) → `UnitGenerator` (칸 ID → 유닛 SO → 프리팹) → 맵 위에 유닛 생성
- 목표: 코드를 고치지 않고 스폰 시트만 바꿔서 유닛 배치를 바꾼다.
- 설계 원본: `Docs\ImplementDocs\MapDesign.md`

## 3. F-03 유닛 스폰
- What : 스폰 시트를 맵 시트와 같은 격자로 읽어 칸마다 유닛 ID를 보관하고, 맵 생성 직후 해당 칸에 유닛 프리팹을 생성
- What : 시트 작성 규칙 - 테두리 칸은 `0`으로 채운다, `0`은 유닛 ID로 쓰지 않는다 (빈칸 = `0` = 유닛 없음)
- What : 좌표 규칙 - `Docs\Claude\TechSpec.md`의 "칸 인덱스 ↔ 월드 좌표 변환 규칙" 참고. 행 수는 **맵**의 행 수를 사용
- What : 예외 처리
  - 숫자가 아닌 칸: 몇 행 몇 열인지 에러 로그, `INVALID_UNIT_ID`(-1)로 저장
  - 1행과 칸 수가 다른 행: 행 번호 에러 로그
  - 스폰 데이터 없음 (URL 비어 있음, 다운로드 실패, 빈 시트): 로더에서 로그, 생성기는 생성 없이 종료
  - 유닛 SO가 없는 ID: 그 칸 건너뜀
  - SO의 `UnitPrefab`이 비어 있음: 유닛 ID 에러 로그, 그 칸 건너뜀
- Where: `Assets/Script/GameData/Loader/UnitSpawnDataLoader.cs` (스폰 TSV 다운로드·변환·보관)
- Where: `Assets/Script/ResourceGenerator/UnitGenerator.cs` (유닛 생성)
- Where: `Assets/Script/GameData/Loader/UnitDataLoader.cs` (`TryGetUnitData()`: ID로 유닛 SO 조회)
- Where: `Assets/Script/GameData/Data/UnitData.cs` (`UnitPrefab` 프로퍼티)
- Where: `Assets/Script/GameData/Loader/HeadDataLoader.cs` (`GenerateMap()` 다음 줄에서 유닛 생성 호출)
- Tune : 스폰 시트 URL (`UnitSpawnDataLoader` Inspector), 유닛 프리팹 (`Assets/ScriptableObject/Unit/`), 칸 크기 (`UnitGenerator`, `MapGenerator.TERRAIN_SIZE`와 같아야 함), 유닛 높이 (`UnitGenerator`)

## 4. 선택 이유
- 격자형 시트: 맵 시트와 같은 모양이라, 두 시트를 나란히 놓고 배치를 눈으로 맞출 수 있다.
- 위치는 맵 행 수, 반복은 스폰 데이터 크기: Google 시트 TSV는 내용이 있는 범위까지만 내보내서 스폰 시트가 맵보다 작게 내려올 수 있다. 맵 행 수로 계산해야 위치가 어긋나지 않는다.
- 맵 행 수는 값(`int`)으로 전달: 생성기가 맵 로더를 몰라도 되어, "로더는 데이터, 생성기는 배치" 분리를 유지한다.
- 프리팹은 SO에서 연결: 스프레드시트는 Unity 에셋을 가리킬 수 없다.
- 오류 칸은 건너뜀: 맵과 달리 유닛은 대체할 기본값이 없고, 잘못된 유닛을 놓으면 전투 결과가 달라진다.
- 에러 로그는 원인 지점 한 곳에서만: 같은 원인으로 로그가 여러 번 찍히지 않게 한다.

## 5. Features 목록
| ID   | 기능            | 사용처                                  | 핵심 위치                          |
|------|-----------------|-----------------------------------------|------------------------------------|
| F-03 | 스폰 데이터 로드 | 스폰 시트를 칸별 유닛 ID 표로 보관        | `Assets/Script/GameData/Loader/`   |
| F-03 | 유닛 배치        | 맵 생성 직후 칸 ID에 맞는 유닛 생성       | `Assets/Script/ResourceGenerator/` |

---

## TODO
- 유닛 높이 `1.7f` 임시 값 → 타일 윗면 상수 + 프리팹 피벗으로 정리 (실제 모델 교체 시), 유닛 이름·진영 생성 로그, 빈칸(`0`)·`-1` 칸 조회 로그 정리, `PlacementUnit`·`unitSpawnArrayRow` 이름 정리
