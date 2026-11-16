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

## 맵 생성 (F-03, F-05)

```mermaid
flowchart LR
    Gen["<b>MapGenerator</b><br/>GenerateMap()<br/>행 → 열 2중 반복<br/>칸 좌표 = (열, 행)"]
    Terrain["<b>TerrainDataLoader</b><br/>TryGetTerrainData(ID)"]
    SO["<b>TerrainData (SO)</b><br/>TilePrefab"]
    Def["<b>Default.prefab</b>"]
    Spawn["<b>Instantiate</b><br/>위치 계산 (TechSpec.md 참고)<br/>MapGenerator의 자식으로 생성"]
    Dict["<b>지형 사전</b><br/>TerrainBySection<br/>칸 → TerrainData"]
    Block["사전에 등록 안 함<br/>= 진입 불가 (F-05)"]

    Gen -->|칸의 지형 ID| Terrain
    Terrain -->|찾음| SO
    Terrain -->|못 찾음| Def
    SO --> Spawn
    SO -->|등록| Dict
    Def --> Spawn
    Def --> Block
```

## 유닛 생성 (F-03, F-05)

```mermaid
flowchart LR
    Gen["<b>UnitGenerator</b><br/>PlacementUnit()<br/>스폰 데이터 null이면 종료<br/>맵 행 수 기억<br/>행 → 열 2중 반복"]
    Unit["<b>UnitDataLoader</b><br/>TryGetUnitData(ID)"]
    SO["<b>UnitData (SO)</b><br/>UnitPrefab"]
    Skip["그 칸 건너뜀"]
    Err["에러 로그 후<br/>그 칸 건너뜀"]
    Spawn["<b>Instantiate</b><br/>SectionToUnitPosition()<br/>UnitGenerator의 자식으로 생성"]
    MU["<b>MapUnit</b><br/>AddComponent 후<br/>SetUnitData(UnitData, 칸)"]
    Dict["<b>유닛 사전</b><br/>UnitsBySection<br/>칸 → MapUnit"]

    Gen -->|칸의 유닛 ID| Unit
    Unit -->|찾음| SO
    Unit -->|못 찾음| Skip
    SO -->|프리팹 있음| Spawn
    SO -->|프리팹 없음| Err
    Spawn --> MU
    MU -->|등록| Dict
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

## 맵 입력·카메라 준비 (F-02, F-03)

```mermaid
flowchart LR
    Head["<b>HeadDataLoader</b><br/>모든 로드 완료"]
    Gen["<b>MapGenerator</b><br/>GenerateMap()<br/>RowCount·ColumnCount 기록"]
    Evt(["OnMapGenerateCompleted"])
    Sel["<b>TileSelector</b><br/>HandleMapGenerated()<br/>타일 윗면 높이에 바닥 평면 생성"]
    Cam["<b>CameraView</b><br/>HandleMapGenerated()<br/>이동 범위 (0,0) ~ ((열-1)×칸, (행-1)×칸)"]

    Head -->|MapArray 전달| Gen
    Gen -->|타일 설치 후 발생| Evt
    Evt -->|구독| Sel
    Evt -->|구독| Cam
```

### 칸 강조 처리 순서 (TileSelector, 매 프레임)

```mermaid
flowchart LR
    A["TileOverlap<br/>마우스 화면 좌표"] --> B["ScreenPointToRay()<br/>바닥 평면과 교차"]
    B -->|교차 없음| H["ClearPointedSection()<br/>맵 밖이 된 순간만 숨김"]
    B -->|교차점| C["WorldToSection()<br/>(열, 행) 반올림 변환<br/>TechSpec.md 참고"]
    C --> D{"IsInsideMap()"}
    D -->|밖| H
    D -->|안| E{"이전 칸과 같음?"}
    E -->|같음| F["갱신 없음"]
    E -->|다름| G["현재 칸 저장<br/>하이라이트 이동·표시"]
```

### 클릭 처리 (TileSelector, F-05)

```mermaid
flowchart TB
    Click["TileClick<br/>마우스 좌클릭"] --> Check{"맵 안을<br/>가리키는 중?"}
    Check -->|아니오| End["무시<br/>(선택 상태여도 유지)"]
    Check -->|예| Use["저장된 현재 칸<br/>(행, 열) 로그"]
    Use --> State{"_selectedUnit<br/>비어 있음?"}

    State -->|예: 대기| Try["<b>TrySelectUnit()</b><br/>TryGetUnitAtSection()"]
    Try --> Ally{"아군 유닛?"}
    Ally -->|아니오| End
    Ally -->|예| Calc["<b>MovementRangeCalculator</b><br/>GetReachableSections()<br/>→ _destinationData"]
    Calc --> Show["<b>MoveRangeDisplay</b><br/>ShowMoveRangeHighlight()<br/>(월드 위치, 비용) 목록"]
    Show --> Sel["_selectedUnit = 유닛<br/>→ 선택 상태"]

    State -->|아니오: 선택| Handle["<b>HandleSelectedStateClick()</b>"]
    Handle --> Dest{"_destinationData에<br/>있는 칸?"}
    Dest -->|예| Move["<b>UnitGenerator</b><br/>MoveUnitToSection()"]
    Dest -->|아니오: 취소| Clear
    Move --> Clear["ClearHighlights()<br/>_selectedUnit, _destinationData = null<br/>→ 대기 상태"]
    Clear -.->|미구현, F-04| Switch["다른 아군 바로 선택<br/>상태 머신으로 이전"]
```

## 이동 범위 (F-05)

### 이동 범위 계산 (MovementRangeCalculator, 다익스트라)

```mermaid
flowchart TB
    In["입력: 출발 칸, 이동력, 진영<br/>지형 사전, 유닛 사전"] --> Init["결과 사전[출발 칸] = 0<br/>후보 목록에 출발 칸"]
    Init --> Loop{"후보 목록<br/>남음?"}
    Loop -->|예| Pick["누적 비용이 가장 싼 칸 꺼내기"]
    Pick --> Dir["4방향 이웃 칸마다 검사"]
    Dir --> A{"지형 사전에 없음<br/>또는 진입 불가 지형?"}
    A -->|예| Skip["건너뜀"]
    A -->|아니오| B{"다른 진영<br/>유닛 칸?"}
    B -->|예| Skip
    B -->|아니오| C{"현재 비용 + 들어갈 칸 RequireMove가<br/>이동력 초과?"}
    C -->|예| Skip
    C -->|아니오| D{"이미 같거나<br/>더 싼 비용 기록?"}
    D -->|예| Skip
    D -->|아니오| Rec["비용 기록<br/>후보에 없으면 추가"]
    Rec --> Loop
    Skip --> Loop
    Loop -->|아니오| Fin["출발 칸, 같은 진영 유닛 칸 제거<br/>(지나갈 수는 있지만 멈출 수 없음)"]
    Fin --> Out["결과: 칸 → 누적 비용"]
```

### 숫자 하이라이트 표시 (MoveRangeDisplay, 오브젝트 풀)

```mermaid
flowchart LR
    Recv["ShowMoveRangeHighlight()<br/>(월드 위치, 비용) 목록"] --> Off["ClearHighlights()<br/>전부 끄기"]
    Off --> Lack{"보유 개수가<br/>필요 개수보다 적음?"}
    Lack -->|예| Dup["DuplicateHighlight()<br/>MoveRangeIndicator 복제 후 끄기"]
    Dup --> Lack
    Lack -->|아니오| On["<b>MoveRangeHighlight</b><br/>ShowMoveRange()<br/>위치 + 높이, 숫자 = 비용"]
```

### 유닛 이동 (UnitGenerator)

```mermaid
flowchart LR
    Call["MoveUnitToSection()<br/>(MapUnit, 새 칸)"] --> Rm["유닛 사전에서<br/>옛 칸 Remove"]
    Rm --> Add["새 칸에 Add"]
    Add --> Set["MapUnit.SetUnitData()<br/>기존 UnitData + 새 칸"]
    Set --> Pos["transform.position<br/>= SectionToUnitPosition()"]
    Pos -.->|미구현, P2| Walk["한 칸씩 이동 연출"]
```

### 카메라 이동 (CameraView, 매 프레임)

```mermaid
flowchart LR
    Input["CameraMove<br/>WASD 2D Vector"] --> Dir["Pivot right·forward로<br/>월드 방향 변환"]
    Dir --> Move["속도 × deltaTime 만큼 이동"]
    Move --> Clamp["X·Z를 이동 범위로 제한"]
    Clamp --> Pivot["<b>InGameCameraPivot</b><br/>위치 적용<br/>(자식 카메라 60° 기울기)"]
    Edge["화면 가장자리 이동<br/>(미구현, P2)"] -.-> Dir
```
