# 구현 다이어그램
> 구현마다 이 파일 하나를 최신화하여 갱신한다.
>
> 점선은 미구현 흐름을 나타낸다.

## 외부 데이터 로딩 (F-01)

```mermaid
flowchart TB
    Head["<b>HeadDataLoader</b><br/>LoadExternalDataRoutine()<br/>두 로더 동시 실행 후 완료 대기"]

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

    Head -->|코루틴 실행| UnitLoader
    Head -->|코루틴 실행| WeaponLoader
```

### 로더 내부 처리 순서 (유닛·무기 공통)

```mermaid
flowchart LR
    A["Awake()<br/>SO 목록 → ID 딕셔너리"] --> B["Load…Table()<br/>TSV 다운로드"] --> C["ConvertTsvToJson()<br/>줄 단위 분리"] --> D["ParseLine()<br/>열 검사·숫자 변환"] --> E["ApplyToJson()<br/>ID 일치 SO에 SetData()"]
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
