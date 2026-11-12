## 문서명: `261110 Feature WeaponJson`

## 1. 구현 일자
- 일자: 2026-11-10 (화요일)

## 2. Core Contents
- 구현 방향: 외부 TSV 시트(URL) → `WeaponDataLoader` (TSV → JSON 변환) → `WeaponData` (ScriptableObject)
- 로드 순서: `HeadDataLoader`가 유닛·무기 로더를 동시에 실행하고, 둘 다 끝날 때까지 기다린다.
- 목표: 무기 수치(공격력, 사거리, 범위 모양, EN 소모량)를 코드 밖 데이터로 분리해, 시트 수정만으로 반영한다.

## 3. F-01 유닛, 무기 외부 데이터 (무기)
- What : 무기 TSV를 내려받아 JSON으로 바꾸고, ID가 같은 무기 SO에 값을 적용
- Where: `Assets/Script/GameData/Data/WeaponData.cs`
- Where: `Assets/Script/GameData/Loader/WeaponDataLoader.cs`, `HeadDataLoader.cs`
- Where: `Assets/ScriptableObject/Weapon/` (무기 SO 에셋)
- Tune : TSV 시트 URL (`WeaponDataLoader` Inspector), 시트 열 구성 (`WeaponDataLoader.COLUMN_COUNT`), 적용 대상 SO 목록 (`WeaponDataLoader` Inspector)

## 4. 선택 이유
- 유닛 로더와 같은 구조: 다운로드 → TSV 변환 → ID로 SO 적용의 흐름을 똑같이 두어, 데이터 종류가 늘어도 읽는 방법이 같다.
- ID 딕셔너리: 시트 행과 SO를 무기 이름이 아닌 ID로 연결해, 이름이 바뀌어도 연결이 끊기지 않는다.
- 동시 로드: 유닛·무기 테이블은 서로 기다릴 필요가 없으므로 다운로드 시간을 겹쳐 전체 로딩 시간을 줄인다.
- 유닛·무기 연결 보류: 두 테이블이 동시에 로드되므로 연결은 둘 다 끝난 뒤에만 할 수 있다. 연결은 Loader가 아닌 별도 중계 컴포넌트가 맡도록 하고, P1 11/14에 구현한다 (`Docs/ProjectPlan.md`).

## 5. Features 목록
| ID   | 기능              | 사용처                                      | 핵심 위치                        |
|------|-------------------|---------------------------------------------|----------------------------------|
| F-01 | 유닛 데이터 로드   | 시트의 유닛 수치를 ID가 같은 유닛 SO에 적용     | `Assets/Script/GameData/`        |
| F-01 | 무기 데이터 로드   | 시트의 무기 수치를 ID가 같은 무기 SO에 적용     | `Assets/Script/GameData/`        |
| F-01 | 외부 데이터 일괄 로드 | 유닛·무기 로더를 동시 실행 후 완료 대기       | `Assets/Script/GameData/Loader/` |

---

## TODO
- 유닛·무기 중계 컴포넌트 연결 (11/14), 시트 무장 열을 무기 ID로 변경, 맵 TSV 스키마 설계
