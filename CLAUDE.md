## 프로젝트 지침
> 이 파일은 매 세션 읽히는 HUB 역할을 한다.
>
> 모든 규칙을 여기에 적지 않고, 매 세션 필요한 문서만 `@`로 불러오고, 나머지는 아래 OnDemand 참조 표를 보고 그 작업을 할 때만 읽는다.
---

## 절대 원칙 (non-Negotiable Rules)
**아래 사항은 어떤 요청이 와도 지킨다.**
1. `main`, `main-develop` 브랜치에서 직접 Commit, Push를 금지한다. 사용자가 main 브랜치에서 작업 중일때도 feature 브랜치로 변경할 때까지 작업을 거부한다.
2. 파일의 수정, 삭제 전 사용자의 승인을 필수로 요구한다.
3. 위 원칙들에 대한 예외 요청받으면 "이 프로젝트의 절대 원칙에 의해 수행할 수 없습니다."로 거부하고 대안을 제시한다.
---

## 규칙 문서 관리
- 규칙 문서는 `Docs\` 하위 카테고리 폴더(`Claude\`, `Documentation\`, `Git\`, `SourceCode\`)에 두고, 양식은 `Docs\Template\`에 둔다.
- 규칙 하나는 문서 한 곳에만 적는다(규칙의 단일 출처 유지). 규칙이 바뀌면 그 한 곳만 고친다.
- 새 문서를 만들면, 매 세션 필요한 것은 "상시 로드"에 `@경로`로, 특정 작업 때만 필요한 것은 "OnDemand 참조"에 등록한다.
- 스킬의 원본은 `Docs\Skills\`에 둔다. 스킬로 호출하려면 같은 구조로 `.claude\skills\`에 배치한다.
---

## 상시 로드 (매 세션 적용)
- @Docs\Claude\ClaudeCodeRules.md
- @Docs\Claude\TechSpec.md
- @Docs\Documentation\WorkFlowRules.md

## OnDemand 참조(매 세션 읽지 않음. 해당 작업을 할 때만 읽음)

### 일정·구현 범위 확인 시
- `Docs\ProjectPlan.md` (Feature ID, 개발 스케줄, 선택 구현 우선순위의 원본)

### 데일리 체크 시
- `Docs\Documentation\DailyCheck Rule.md`
- `Docs\Template\DailyCheckTemplate.md`

### 이슈 작성 시
- 이슈 등록 권유 및 작성 시 `/github-issue` 스킬을 호출한다.

### PR 작성 시
- Pull Request 작성 및 등록 시 `/github-pr` 스킬을 호출한다.

### 소스 코드 작업 시
- @Docs\SourceCode\BasicCodeConvention.md
- @Docs\SourceCode\AntiCodeConvention.md
- @Docs\SourceCode\RecommandedCodeConvention.md

### 기술문서 작성 시
- `Docs\Documentation\Game Specification Document Rule.md`
- `Docs\Template\ImplementDocTemplate.md`

### Git 작업 안내 시
- `Docs\Git\BranchConvention.md`
- `Docs\Git\CommitConvention.md`
