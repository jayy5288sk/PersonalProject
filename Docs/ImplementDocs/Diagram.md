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

## 유닛 생성 (F-03, F-04, F-05)

```mermaid
flowchart LR
    Gen["<b>UnitGenerator</b><br/>PlacementUnit()<br/>스폰 데이터 null이면 종료<br/>맵 행 수 기억<br/>행 → 열 2중 반복"]
    Unit["<b>UnitDataLoader</b><br/>TryGetUnitData(ID)"]
    SO["<b>UnitData (SO)</b><br/>UnitPrefab"]
    Skip["그 칸 건너뜀"]
    Err["에러 로그 후<br/>그 칸 건너뜀"]
    Spawn["<b>Instantiate</b><br/>SectionToUnitPosition()<br/>UnitGenerator의 자식으로 생성"]
    MU["<b>MapUnit</b><br/>AddComponent 즉시 Awake:<br/>Renderer·원래 색 저장<br/>SetUnitData(UnitData, 칸)"]
    Dict["<b>유닛 사전</b><br/>UnitsBySection<br/>칸 → MapUnit"]
    Done(["OnUnitPlacementCompleted"])
    Phase["<b>PhaseManager</b><br/>첫 아군 페이즈 시작 (F-04)"]

    Gen -->|칸의 유닛 ID| Unit
    Unit -->|찾음| SO
    Unit -->|못 찾음| Skip
    SO -->|프리팹 있음| Spawn
    SO -->|프리팹 없음| Err
    Spawn --> MU
    MU -->|등록| Dict
    Gen -->|모든 칸 반복 후 발생| Done
    Done -->|구독| Phase
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
    Sel["<b>TileSelector</b><br/>HandleMapGenerated()<br/>MapGenerator.GroundHeight 높이에 바닥 평면 생성"]
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
    B -->|교차점| C["<b>MapGenerator</b><br/>WorldToSection()<br/>(열, 행) 반올림 변환<br/>TechSpec.md 참고"]
    C --> D{"<b>MapGenerator</b><br/>IsInsideMap()"}
    D -->|밖| H
    D -->|안| E{"이전 칸과 같음?"}
    E -->|같음| F["갱신 없음"]
    E -->|다름| G["현재 칸 저장<br/>ReturnTerrainPosition() + 띄우는 높이<br/>하이라이트 이동·표시"]
```

### 클릭 처리 (TileSelector, F-04)

```mermaid
flowchart LR
    Frame["Update()<br/>UI 위 여부를 미리 계산<br/>(_isPointerOverUI)"] -.-> UI
    Click["TileClick<br/>마우스 좌클릭"] --> In{"맵 안을<br/>가리키는 중?"}
    In -->|아니오| End["무시"]
    In -->|예| UI{"마우스가<br/>UI 위?"}
    UI -->|예| End
    UI -->|아니오| SM["<b>StateManager</b><br/>SectionClicked(현재 칸)<br/>→ 아군 페이즈 조작 참고"]
```

## 아군 페이즈 조작 (F-04)

### 구조: 페이즈(바깥) + 조작 단계(안쪽)

```mermaid
flowchart TB
    subgraph Input["입력"]
        direction LR
        Tile["<b>TileSelector</b><br/>칸 클릭"]
        HUD["<b>CommandMenuHUD</b><br/>이동·공격·대기 버튼<br/>턴 종료 버튼"]
    end

    Phase["<b>PhaseManager</b><br/>현재 페이즈 1개<br/>ChangePhase() · RequestTurnEnd()"]
    SM["<b>StateManager</b><br/>현재 상태 1개 · StateContext 1개<br/>ChangeNextState() · 입력 잠금"]
    Ctx["<b>StateContext</b><br/>SelectedUnit · IsUnitMoved"]

    subgraph Show["표시"]
        direction LR
        Disp["<b>UnitSelectDisplay</b><br/>바닥 테두리 + 머리 위 마커"]
        MU["<b>MapUnit</b><br/>행동 완료 · 어둡게"]
        Range["<b>MoveRangeDisplay</b><br/>이동 범위 숫자"]
    end

    Tile -->|SectionClicked| SM
    HUD -->|ButtonInputReceiver CommandType| SM
    HUD -->|RequestTurnEnd| Phase
    SM --- Ctx
    SM -->|상태 들어가기·나가기| Show
    Phase -->|ResetControlState · SetInputLock| SM
    Phase -->|ResetUnitAction| MU
    SM -.->|OnStateChanged StateType| HUD
    SM -.->|OnStateChanged StateType| Phase
    Phase -.->|OnPhaseChanged PhaseType| HUD
```

> 실선은 메서드 호출, 점선은 이벤트 구독이다. `TempLog`(디버그 전용)도 두 이벤트를 구독한다.

### 조작 단계 상태 전환 (StateManager)

```mermaid
flowchart TB
    Start(["Start()"]) --> Wait

    Wait["<b>UnitSelectWait</b><br/>들어가기: 선택 표시 끄기 · 컨텍스트 비우기"]
    Cmd["<b>CommandSelect</b><br/>들어가기: 현재 위치로 선택 표시 켜기"]
    Move["<b>MoveSectionSelect</b><br/>들어가기: 이동 범위 계산·표시<br/>나가기: 표시 끄기"]
    Atk["<b>AttackTargetSelect</b><br/>들어가기: 로그"]

    Wait -->|"클릭: 행동 가능한 아군<br/>PutUnit"| Cmd
    Cmd -->|"Move (이동 전만)"| Move
    Cmd -->|Attack| Atk
    Cmd -->|"Wait<br/>CompleteUnitAction"| Wait
    Cmd -->|"맵 클릭 (이동 전)<br/>= 선택 취소"| Wait
    Move -->|"숫자 칸: MoveUnitToSection<br/>AlreadyMovedUnit"| Cmd
    Move -->|"그 외 칸: 취소"| Cmd
    Atk -->|"아무 칸 클릭<br/>(임시 탈출 경로)"| Cmd
    Atk -.->|"미구현, F-06"| Fire["무장 선택 · 공격 범위 · 공격"]
    Reset(["ResetControlState()<br/>아군 페이즈 시작 시"]) --> Wait
```

> 이동 후 커맨드 선택에서는 맵 클릭을 무시하고, `Move`는 HUD(버튼 비활성)와 상태(`IsUnitMoved` 확인) 양쪽에서 막는다.

### 상태 전환 처리 순서 (ChangeNextState)

```mermaid
flowchart LR
    In["SectionClicked() /<br/>ButtonInputReceiver()"] --> Lock{"입력 잠금?"}
    Lock -->|예| X["무시"]
    Lock -->|아니오| Ask["현재 상태에 위임<br/>SectionClick() / PressButton()"]
    Ask --> Null{"반환값<br/>null?"}
    Null -->|예| X2["전환 없음"]
    Null -->|아니오| Exit["현재 상태<br/>StateExit()"]
    Exit --> Swap["현재 상태 교체"]
    Swap --> Enter["새 상태<br/>StateEnter()"]
    Enter --> Evt(["OnStateChanged<br/>(StateType)"])
```

### 페이즈 전환 (PhaseManager)

```mermaid
flowchart TB
    Placed(["OnUnitPlacementCompleted"]) --> Ally
    Ally["<b>AllyPhase</b><br/>들어가기: 모든 아군 ResetUnitAction<br/>StateManager.ResetControlState()<br/>→ null (머무름)"]
    Enemy["<b>EnemyPhase</b><br/>들어가기: SetInputLock(true)<br/>→ AllyPhase 반환 (임시 즉시 복귀)<br/>나가기: SetInputLock(false)"]

    Check{"UnitSelectStandBy 수신 시<br/>아군 전원 행동 완료?<br/>(아군 0명이면 아니오)"}
    Ally --> Check
    Check -->|예| Enemy
    Turn(["턴 종료 버튼<br/>RequestTurnEnd()<br/>아군 페이즈일 때만"]) --> Enemy
    Enemy -->|임시| Ally
    Enemy -.->|미구현, F-08| AI["적 AI 행동 후 아군 페이즈"]
```

### 페이즈 전환 처리 순서 (ChangePhase)

```mermaid
flowchart LR
    Req["ChangePhase(다음 페이즈)"] --> Loop{"다음 페이즈<br/>있음?"}
    Loop -->|아니오| End["종료"]
    Loop -->|예| Exit["현재 페이즈<br/>PhaseExit()"]
    Exit --> Swap["현재 페이즈 교체"]
    Swap --> Enter["다음 페이즈 = PhaseEnter()<br/>(머무르면 null)"]
    Enter --> Evt(["OnPhaseChanged<br/>(PhaseType)"])
    Evt --> Loop
```

> 들어가기 안에서 직접 전환하지 않고 다음 페이즈를 반환하므로, 이벤트가 항상 `Enemy` → `Ally` 순서로 나간다.

### 커맨드 메뉴 HUD (CommandMenuHUD)

```mermaid
flowchart LR
    S(["OnStateChanged"]) --> IsCmd{"CommandSelect?"}
    IsCmd -->|아니오| Off["패널 끄기"]
    IsCmd -->|예| On["패널 켜기<br/>이동 버튼 = !IsSelectedUnitMoved"]
    P(["OnPhaseChanged"]) --> TE["턴 종료 버튼<br/>= 아군 페이즈일 때만 활성"]
    Btn["버튼 onClick<br/>(AddListener)"] -->|CommandType| SM["<b>StateManager</b><br/>ButtonInputReceiver()"]
    TBtn["턴 종료 onClick"] --> PM["<b>PhaseManager</b><br/>RequestTurnEnd()"]
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
