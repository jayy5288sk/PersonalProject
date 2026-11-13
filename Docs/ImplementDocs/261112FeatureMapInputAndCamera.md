## 문서명: `261112 Feature MapInputAndCamera`

## 1. 구현 일자
- 일자: 2026-11-12 (목요일) ~ 2026-11-13 (금요일)

## 2. Core Contents
- 구현 방향: `MapGenerator` (맵 생성 완료 이벤트, 행·열 수 공개) → `TileSelector` (마우스 → 칸 강조·클릭) / `CameraView` (WASD 이동, 맵 범위 제한)
- 목표: 맵이 만들어진 뒤, 마우스가 가리키는 칸을 강조하고 클릭한 칸의 (행, 열)을 얻는다. 쿼터뷰 카메라를 WASD로 움직이되 맵 밖으로 나가지 않게 한다.
- 입력 정의: `Assets/GameInputActions.inputactions` (`GameMap` 맵: `TileOverlap` 마우스 위치, `TileClick` 좌클릭, `CameraMove` WASD 2D Vector)

## 3. F-03 칸 강조 · 클릭 칸 인덱스 전달
- What : 마우스 Ray와 타일 윗면 높이의 바닥 평면(`Plane`)이 만나는 점을 (열, 행)으로 바꾸고, 맵 안이면 하이라이트를 그 칸으로 옮긴다
- What : 칸이 바뀐 프레임에만 하이라이트를 갱신하고, 맵 밖으로 나간 순간에만 숨긴다
- What : 클릭 시 Update에서 계산해 둔 현재 칸을 그대로 사용 (현재는 로그 출력까지)
- What : 변환 규칙 - `Docs\Claude\TechSpec.md`의 "마우스 → 칸 변환" 참고
- Where: `Assets/Script/Input/TileSelector.cs`
- Where: `Assets/Script/ResourceGenerator/MapGenerator.cs` (`OnMapGenerateCompleted`, `RowCount`, `ColumnCount`, `TERRAIN_SIZE` 공개)
- Where: `Assets/Prefab/Map/TileSelector.prefab`, `Assets/Prefab/Map/TempMapTiles/Highlight.prefab`
- Tune : 타일 윗면 높이·하이라이트 띄움 거리 (`TileSelector` Inspector), 하이라이트 모양 (`Highlight.prefab`)

## 3. F-02 쿼터뷰 카메라
- What : 회전 없는 Pivot 아래에 X축으로 기울인 카메라를 둔다. Pivot만 XZ로 움직이고 카메라는 따라간다
- What : WASD 입력을 Pivot의 `right`/`forward` 방향으로 바꿔 이동, Pivot 위치를 맵 범위 (0 ~ (열 수 - 1) × 칸 크기, 0 ~ (행 수 - 1) × 칸 크기)로 제한
- Where: `Assets/Script/Camera/CameraView.cs`
- Where: `Assets/Prefab/Cameras/InGameCameraPivot.prefab` (Pivot 위치, 카메라 기울기)
- Tune : 이동 속도 (`CameraView` Inspector), 시점 각도·높이 (`InGameCameraPivot.prefab`)

## 4. 선택 이유
- 콜라이더 Raycast 대신 수학 평면(`Plane`): 타일마다 콜라이더가 필요 없고, 칸은 반올림만으로 구할 수 있다. 맵이 평평한 동안은 이걸로 충분하다.
- 칸이 바뀔 때만 갱신: 매 프레임 위치를 다시 넣거나 `SetActive`를 반복 호출하지 않는다.
- 클릭은 계산해 둔 칸 재사용: 같은 계산을 두 번 하지 않고, 강조된 칸과 클릭된 칸이 항상 같다.
- 맵 생성 완료 이벤트: 행·열 수는 TSV를 내려받은 뒤에야 정해진다. `TileSelector`·`CameraView`는 이벤트를 받기 전에는 아무 계산도 하지 않는다.
- 생성기는 이벤트만 발생: `MapGenerator`가 입력·카메라를 몰라도 되어, 구독하는 쪽만 늘리면 된다.
- Pivot + 기울인 카메라: 이동(Pivot)과 시점 각도(카메라)를 나눠서, 이동 방향이 기울기의 영향을 받지 않는다 (Y 변화 없음).
- 제한 기준은 Pivot 위치: 계산이 단순하다. 단, 화면 중심과 Pivot 위치가 달라서 보이는 범위가 맵과 정확히 맞지는 않는다 (TODO 참고).

## 5. Features 목록
| ID   | 기능            | 사용처                                   | 핵심 위치                          |
|------|-----------------|------------------------------------------|------------------------------------|
| F-03 | 칸 강조          | 마우스가 가리키는 맵 칸에 하이라이트 표시    | `Assets/Script/Input/`             |
| F-03 | 클릭 칸 인덱스    | 클릭한 칸의 (행, 열) 획득 (F-04 연결 예정)  | `Assets/Script/Input/`             |
| F-03 | 맵 생성 완료 알림 | 행·열 수 공개, 입력·카메라 준비 시작        | `Assets/Script/ResourceGenerator/` |
| F-02 | 카메라 이동·제한  | WASD로 시점 이동, 맵 범위 밖 이탈 방지      | `Assets/Script/Camera/`            |

---

## TODO
- 카메라 제한 기준을 화면 중심으로 보정 검토 (미검증: 에디터 확인 필요), `_groundHeight`(0.75)는 `MapGenerator` 타일 Y·크기와 따로 관리됨 → 변경 시 함께 수정, 클릭 칸을 F-04 유닛 선택에 전달, 화면 가장자리 이동 (P2), `CameraView`·`TileSelector`의 미사용 `using System;` 정리
