## 문서명: `261110 Feature MapDesign`

## 1. 구현 일자
- 일자: 2026-11-11 (수요일)

## 2. Core Contents
- 구현 방향: 스테이지 맵 TSV 시트 → `MapDesignDataLoader` (TSV → `JArray` 행의 배열) → `MapGenerator` (칸 ID → 지형 SO → 프리팹) → XZ 평면에 타일 생성
- 목표: 코드를 고치지 않고 맵 시트 URL만 바꿔서 다른 스테이지 맵을 생성한다.
- 설계 원본: `Docs\ImplementDocs\MapDesign.md`

## 3. F-03 맵 외부 데이터
- What : 맵 시트를 내려받아 칸마다 지형 ID를 담은 2차원 `JArray`로 보관하고, 모든 로드가 끝난 뒤 ID에 맞는 지형 프리팹을 칸 위치에 생성
- What : 좌표 규칙 - `Docs\Claude\TechSpec.md`의 "칸 인덱스 ↔ 월드 좌표 변환 규칙" 참고
- What : 예외 처리
  - 숫자가 아닌 칸·빈 칸: 몇 행 몇 열인지 에러 로그, `INVALID_TERRAIN_ID`(-1)로 저장
  - 1행과 칸 수가 다른 행: 행 번호 에러 로그 (행은 버리지 않음)
  - 지형 정의에 없는 ID: 기본 프리팹(`Default.prefab`)으로 대체
- Where: `Assets/Script/GameData/Loader/MapDesignDataLoader.cs` (맵 TSV 다운로드·변환·보관)
- Where: `Assets/Script/ResourceGenerator/MapGenerator.cs` (타일 생성)
- Where: `Assets/Script/GameData/Loader/TerrainDataLoader.cs` (`TryGetTerrainData()`: ID로 지형 SO 조회)
- Where: `Assets/Script/GameData/Data/TerrainData.cs` (`TilePrefab` 프로퍼티)
- Where: `Assets/Script/GameData/Loader/HeadDataLoader.cs` (로드 완료 후 `GenerateMap()` 호출)
- Tune : 맵 시트 URL (`MapDesignDataLoader` Inspector), 칸 크기 (`MapGenerator.TERRAIN_SIZE`), 지형별 프리팹 (`Assets/ScriptableObject/Terrain/`), 기본 프리팹 (`MapGenerator` Inspector)

## 4. 선택 이유
- 로더와 생성기 분리: 로더는 데이터만 만들고 배치·예외 처리는 생성기가 맡아, 데이터 형식이 바뀌어도 생성 코드는 그대로 둔다.
- `JArray`로 보관: 맵 데이터는 값을 넣을 SO가 없으므로, 생성기가 `[행][열]`로 바로 꺼낼 수 있는 형태로 보관한다.
- 맵 크기를 시트에서 읽기: 행 수와 1행의 칸 수를 기준으로 삼아, 맵 크기가 바뀌어도 코드를 고치지 않는다.
- 잘못된 칸은 -1로 저장: 줄을 버리면 맵 모양이 무너지므로, 칸 하나만 지형 정의에 없는 값으로 바꿔 기본 프리팹이 놓이게 한다.
- 로드 완료 후 생성: 다운로드 속도와 관계없이 지형 정의와 맵 데이터가 모두 준비된 뒤에 생성한다.
- 위치는 코드에서 지정: `Instantiate`에 위치를 넘기므로 프리팹 자체의 위치값은 결과에 영향을 주지 않는다.

## 5. Features 목록
| ID   | 기능          | 사용처                                   | 핵심 위치                          |
|------|---------------|------------------------------------------|------------------------------------|
| F-03 | 맵 데이터 로드 | 스테이지 맵 시트를 칸별 지형 ID 표로 보관   | `Assets/Script/GameData/Loader/`   |
| F-03 | 맵 그리드 생성 | 칸 ID에 맞는 지형 타일을 XZ 평면에 배치     | `Assets/Script/ResourceGenerator/` |

---

## TODO
- 지형 정의에 없는 칸을 진입 불가 칸으로 기록 (F-05 이동 기능), SO의 `TilePrefab`이 비었을 때 기본 프리팹 대체, 스테이지 선택 시 맵 URL 보관 위치 (F-12), 마우스 오버 칸 강조·클릭 칸 정보 전달 (F-03 잔여)
