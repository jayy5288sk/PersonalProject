## 문서명: `261110 Feature JsonData`

## 1. 구현 일자
- 일자: 2026-11-10 (화요일)

## 2. Core Contents
- 구현 방향: 외부 TSV 시트(URL) → `UnitDataLoader` (TSV → JSON 변환) → `UnitData` (ScriptableObject)
- 목표: 유닛·무기 수치를 코드 밖 데이터로 분리해, 코드를 고치지 않고 시트 수정만으로 반영한다.

## 3. F-01 유닛, 무기 외부 데이터
- What : 유닛·무기 데이터 SO 정의, 외부 TSV를 내려받아 ID가 같은 SO에 값을 적용하는 로더
- Where: `Assets/Script/GameData/Data/UnitData.cs`, `WeaponData.cs`
- Where: `Assets/Script/GameData/Loader/HeadDataLoader.cs`, `UnitDataLoader.cs`, `WeaponDataLoader.cs`
- Where: `Assets/ScriptableObject/Unit/` (유닛 SO 에셋)
- Tune : TSV 시트 URL (`UnitDataLoader` Inspector), 시트 열 구성 (`UnitDataLoader.COLUMN_COUNT`), 적용 대상 SO 목록 (`UnitDataLoader` Inspector)

## 4. 선택 이유
- TSV 시트: 표 형태로 수치를 한눈에 보고 수정할 수 있고, 맵 격자 데이터도 같은 방식으로 다룰 수 있다.
- Newtonsoft.Json: JsonUtility는 최상위 배열을 바로 다루지 못하므로, `JArray` / `JObject`로 행 단위 데이터를 다루기 위해 사용한다.
- ScriptableObject: 다른 시스템이 Inspector에서 데이터를 참조할 수 있고, 외부 데이터 로딩 전에도 기본값을 가진다.
- `HeadDataLoader`: 데이터 종류별 로더의 실행 순서를 한 곳에서 관리한다.

## 5. Features 목록
| ID   | 기능              | 사용처                                  | 핵심 위치                     |
|------|-------------------|-----------------------------------------|-------------------------------|
| F-01 | 유닛 데이터 로드   | 시트의 유닛 수치를 ID가 같은 유닛 SO에 적용 | `Assets/Script/GameData/`     |
| F-01 | 무기 데이터 구조   | 무기 수치(사거리·범위·EN 소모)를 담는 SO   | `Assets/Script/GameData/Data/` |

---

## TODO
- `WeaponDataLoader` 로드 기능 구현, 맵 TSV 스키마 설계, 칸 인덱스 ↔ 월드 좌표 변환 규칙 확정
