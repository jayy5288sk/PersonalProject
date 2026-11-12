# 구현 다이어그램
> 구현마다 이 파일 하나를 최신화하여 갱신한다.
>
> 점선은 미구현 흐름을 나타낸다.

## 외부 데이터 로딩 (F-01, F-03)

```mermaid
flowchart TB
    Head["<b>HeadDataLoader</b><br/>LoadExternalDataRoutine()<br/>다섯 로더 동시 실행 후 완료 대기"]

    subgraph Unit["유닛"]
        direction TB
        UnitSheet[("유닛 TSV 시트")]
        UnitLoader["<b>UnitDataLoader</b><br/>LoadUnitTable()"]
        UnitSO["<b>UnitData (SO)</b><br/>SetData()"]
        UnitSheet -->|다운로드| UnitLoader
        UnitLoader -->|ID 일치 시 값 적용| UnitSO
    end

    subgraph Weapon["무기"]
        direction TB
        WeaponSheet[("무기 TSV 시트")]
        WeaponLoader["<b>WeaponDataLoader</b><br/>LoadWeaponTable()"]
        WeaponSO["<b>WeaponData (SO)</b><br/>SetData()"]
        WeaponSheet -->|다운로드| WeaponLoader
        WeaponLoader -->|ID 일치 시 값 적용| WeaponSO
    end

    subgraph Terrain["지형"]
        direction TB
        TerrainSheet[("지형 TSV 시트")]
        TerrainLoader["<b>TerrainDataLoader</b><br/>LoadTerrainTable()"]
        TerrainSO["<b>TerrainData (SO)</b><br/>SetTerrainData()"]
        TerrainSheet -->|다운로드| TerrainLoader
        TerrainLoader -->|ID 일치 시 값 적용| TerrainSO
    end

    subgraph Map["맵 디자인"]
        direction TB
        MapSheet[("스테이지 맵 TSV 시트")]
        MapLoader["<b>MapDesignDataLoader</b><br/>LoadMapTable()"]
        MapArray["<b>MapArray (JArray)</b><br/>[행][열] = 지형 ID"]
        MapSheet -->|다운로드| MapLoader
        MapLoader -->|변환 후 보관| MapArray
    end

    subgraph Spawn["스폰 디자인"]
        direction TB
        SpawnSheet[("스테이지 스폰 TSV 시트")]
        SpawnLoader["<b>UnitSpawnDataLoader</b><br/>LoadUnitSpawnTable()"]
        SpawnArray["<b>UnitSpawnArray (JArray)</b><br/>[행][열] = 유닛 ID"]
        SpawnSheet -->|다운로드| SpawnLoader
        SpawnLoader -->|변환 후 보관| SpawnArray
    end

    Generator["<b>MapGenerator</b><br/>GenerateMap()"]
    UnitGen["<b>UnitGenerator</b><br/>PlacementUnit()"]

    Head -->|코루틴 실행| UnitLoader
    Head -->|코루틴 실행| WeaponLoader
    Head -->|코루틴 실행| TerrainLoader
    Head -->|코루틴 실행| MapLoader
    Head -->|코루틴 실행| SpawnLoader
    Head -->|1. 모든 로드 완료 후 MapArray 전달| Generator
    Head -->|2. 맵 생성 다음, UnitSpawnArray·맵 행 수 전달| UnitGen
```

### 로더 내부 처리 순서 (유닛·무기 공통)

```mermaid
flowchart LR
    A["Awake()<br/>SO 목록 → ID 딕셔너리"] --> B["Load…Table()<br/>TSV 다운로드"] --> C["ConvertTsvToJson()<br/>줄 단위 분리"] --> D["ParseLine()<br/>열 검사·숫자 변환"] --> E["ApplyToJson()<br/>ID 일치 SO에 SetData()"]
```

### 맵 로더 내부 처리 순서

```mermaid
flowchart LR
    A["LoadMapTable()<br/>TSV 다운로드"] --> B["ConvertTsvToJArray()<br/>줄 단위 분리·1행 기준 칸 수 검사"] --> C["ParseRow()<br/>칸마다 숫자 변환<br/>실패 시 -1"] --> D["MapArray에 보관"]
```

## 맵 생성 (F-03)

```mermaid
flowchart LR
    Gen["<b>MapGenerator</b><br/>GenerateMap()<br/>행 → 열 2중 반복"]
    Terrain["<b>TerrainDataLoader</b><br/>TryGetTerrainData(ID)"]
    SO["<b>TerrainData (SO)</b><br/>TilePrefab"]
    Def["<b>Default.prefab</b>"]
    Spawn["<b>Instantiate</b><br/>위치 계산 (TechSpec.md 참고)<br/>MapGenerator의 자식으로 생성"]
    Block["진입 불가 칸 기록<br/>(미구현, F-05)"]

    Gen -->|칸의 지형 ID| Terrain
    Terrain -->|찾음| SO
    Terrain -->|못 찾음| Def
    SO --> Spawn
    Def --> Spawn
    Def -.-> Block
```

## 유닛 생성 (F-03)

```mermaid
flowchart LR
    Gen["<b>UnitGenerator</b><br/>PlacementUnit()<br/>스폰 데이터 null이면 종료<br/>행 → 열 2중 반복"]
    Unit["<b>UnitDataLoader</b><br/>TryGetUnitData(ID)"]
    SO["<b>UnitData (SO)</b><br/>UnitPrefab"]
    Skip["그 칸 건너뜀"]
    Err["에러 로그 후<br/>그 칸 건너뜀"]
    Spawn["<b>Instantiate</b><br/>위치 계산 (TechSpec.md 참고, 행 수 = 맵 행 수)<br/>UnitGenerator의 자식으로 생성"]

    Gen -->|칸의 유닛 ID| Unit
    Unit -->|찾음| SO
    Unit -->|못 찾음| Skip
    SO -->|프리팹 있음| Spawn
    SO -->|프리팹 없음| Err
```

## 유닛·무기 연결 (미구현, P1 11/14)

```mermaid
flowchart LR
    Head["<b>HeadDataLoader</b><br/>두 로드 완료"]
    Linker["<b>유닛·무기 중계 컴포넌트</b>"]
    WeaponLoader["<b>WeaponDataLoader</b><br/>무기 ID로 조회"]
    UnitSO["<b>UnitData (SO)</b><br/>현재 무장: 무기 이름(string)"]

    Head -.->|연결 실행| Linker
    Linker -.->|1. 무기 ID 전달| WeaponLoader
    WeaponLoader -.->|2. WeaponData 반환| Linker
    Linker -.->|3. WeaponData 참조 설정| UnitSO
```
