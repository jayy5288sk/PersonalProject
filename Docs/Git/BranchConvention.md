## 브랜치 개설 방식
- main에서 브랜치를 개설하는 것이 아닌 main에서 파생된 main-develop에서 브랜치를 개설하여 진행한다.
- 이슈 작성이 완료되었을 때, 브랜치를 개설한다. 
- 브랜치 이름은 다음과 같은 규칙에 의거하여 작성한다.

| 브랜치 | 용도 |
| --- | --- |
| `main` | 한 페이즈가 끝났을 때 병합 (직접 커밋 금지) |
| `main-develop` | 개발이 완료된 결과물을 병합 (직접 커밋 금지) |
| `feature-영문이름축약-기능(PascalCase)` | 개인 작업 (예: `Feature-JY-PlayerMove`) |

- 브랜치 앞의 타입은 다음과 같다.

| 타입 | 언제 쓰나 | 예시 |
|---|---|---|
| `Feature` | 새로운 기능 추가 | `Feature-JY-PlayerMove` |
| `Fix` | 버그 수정, 동작 변화 없는 구조 개선 | `Fix-JY-PlayerMove` |
| `Docs` | 기술 문서 작성·수정 | `Docs-JY-PlayerDesignDocumentation` |

- `main`과 `main-develop` 에는 절대 직접 커밋하지 않는다 → 모든 작업은 PR 로 올린다
- 병합이 끝난 브랜치는 관련 이슈가 닫히고 나서 삭제한다.