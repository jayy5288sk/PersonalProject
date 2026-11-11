## 문서명: `261110 Feature MapDesign`

## 1. 구현 일자
- 일자: 2026-11-10 (화요일)
- 갱신: 2026-11-11 (수요일), 맵 로더·생성기 구현 내용 반영 (구현 상세: `Docs\ImplementDocs\261110FeatureMapDesign.md`)

## 2. Core Contents
- 구현 방향: 지형 TSV 시트 → JSON (지형 정의) + 지형 SO (Cube 프리팹) → 스테이지별 맵 TSV (칸별 Terrain ID) → `JArray` (행의 배열) → 시트 크기만큼 그리드 생성
- 목표: 지형 규칙, 지형 외형, 맵 배치를 나누어, 스테이지 교체 시 맵 시트만 바꾸면 되도록 한다.

## 3. F-03 맵 외부 데이터
- What : 데이터 구성
  | 데이터 | 형식 | 내용 | 스테이지별 여부 |
  |---|---|---|---|
  | 지형 정의 (TerrainData) | TSV → JSON → SO 적용 | ID, TileName, RequireMove, IsMoveAvailable | 공통 |
  | 지형 외형 | SO | ID별 Cube 프리팹 (시트에서 받지 않음) | 공통 |
  | 맵 디자인 | 스테이지별 TSV → `JArray` 보관 | 셀마다 Terrain ID, 시트 격자 = 맵 격자, 헤더 없음 (A1부터 ID) | 스테이지별 |
  | 스폰 위치 | 맵 시트와 별도 TSV | 유닛 배치 좌표 (미구현) | 스테이지별 |
- What : 맵 크기 - 시트의 행 수와 1행의 칸 수에서 읽는다.
- What : 좌표 규칙 - `Docs\Claude\TechSpec.md`의 "칸 인덱스 ↔ 월드 좌표 변환 규칙" 참고
- What : 이동 판정 - `IsMoveAvailable`로 진입 가능 여부를 먼저 검사하고, 가능할 때만 `RequireMove`만큼 이동력을 소모한다. 진입 불가 지형의 `RequireMove` 값은 사용하지 않는다. (F-05에서 구현)
- What : 예외 처리
  - 숫자가 아닌 칸·빈 칸: 행·열 위치와 값을 에러 로그로 출력, `INVALID_TERRAIN_ID`(-1)로 저장
  - 1행과 칸 수가 다른 행: 행 번호를 에러 로그로 출력
  - 지형 정의에 없는 Terrain ID: 기본 프리팹(`Default.prefab`)으로 대체
- Where: `Assets/Script/GameData/Loader/MapDesignDataLoader.cs`, `Assets/Script/ResourceGenerator/MapGenerator.cs`
- Where: `Assets/Script/GameData/Loader/TerrainDataLoader.cs`, `Assets/Script/GameData/Data/TerrainData.cs`
- Where: `Assets/ScriptableObject/Terrain/` (지형 SO), `Assets/Prefab/Map/MapTiles/` (지형 프리팹)
- Tune : 지형별 이동 비용·진입 가능 여부 (지형 TSV 시트), 지형 외형 (지형 SO), 칸 배치 (맵 TSV 시트), 맵 시트 URL (`MapDesignDataLoader` Inspector), 칸 크기 (`MapGenerator.TERRAIN_SIZE`)

## 4. 선택 이유
- 지형 정의와 외형 분리: 스프레드시트는 Unity 에셋(프리팹)을 참조할 수 없으므로, 수치는 시트에서 받고 외형만 SO에 둔다. SO에는 이동 관련 수치를 두지 않아 값의 출처를 하나로 유지한다.
- 맵에 ID 기입: 지형 이름이 바뀌어도 맵 시트를 고칠 필요가 없다.
- 시트 격자 = 맵 격자: 시트에서 맵 모양을 눈으로 보며 디자인할 수 있다.
- 시트 맨 위 줄 = 화면 위쪽: 시트에서 본 모양 그대로 게임 화면에 나오게 한다.
- 맵 데이터는 `JArray`로 보관: 맵은 값을 넣을 SO가 없으므로, 생성기가 `[행][열]`로 바로 꺼낼 수 있는 형태로 둔다.
- 스테이지별 시트, 스폰 분리: 스테이지 교체 시 해당 스테이지 시트만 바꾸면 되고, 지형 정의는 모든 스테이지가 함께 쓴다.
- 선검증 후 비용: 진입 가능 여부와 비용을 나누어, 이동 판정 흐름을 단순하고 직관적으로 유지한다.
- 미정의 ID는 진입 불가: 잘못된 데이터로 원래 막혀야 할 길이 뚫리는 것을 막는다. (F-05에서 구현)

## 5. Features 목록
| ID   | 기능          | 사용처                                   | 핵심 위치                          |
|------|---------------|------------------------------------------|------------------------------------|
| F-03 | 맵 데이터 로드 | 스테이지 맵 시트를 칸별 지형 ID 표로 보관   | `Assets/Script/GameData/Loader/`   |
| F-03 | 맵 그리드 생성 | 칸 ID에 맞는 지형 타일을 XZ 평면에 배치     | `Assets/Script/ResourceGenerator/` |

---

## TODO
- 미정의 ID 칸을 진입 불가로 기록 (F-05), SO의 `TilePrefab`이 비었을 때 기본 프리팹 대체, 스폰 위치 TSV, 스테이지 선택 시 맵 URL 보관 위치 (F-12), 스테이지 메타 정보 필요 여부
