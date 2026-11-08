# 프로젝트 코드 컨벤션
## 1. 기본 코드 네이밍 
- 이름을 지을 때, 쉬운 영단어를 사용하여 이름만 보더라도 어떤 역할을 할 수 있는지 바로 알 수 있도록 직관적이게 짓는다.
- 외부 라이브러리·Unity 콜백(`Awake`, `Update` 등)은 원래 이름을 따른다.

| 대상 | 규칙 | 예시 | 
|:---:|:---:|:---:|
| 클래스, 구조체, Enum | PascalCase | `PlayerController`, `WeaponType` | 
| 인터페이스 | `I` + PascalCase | `IDamageable`, `IPoolable` | 
| 추상 기본 클래스 | PascalCase + `Base` | `PlayerBase`, `MonsterBase` | 
| 메서드 | PascalCase | `TakeDamage()` | 
| 프로퍼티, 이벤트 | PascalCase | `MaxPlayerHealth`, `OnHealthChanged` | 
| 지역변수, 매개변수 | camelCase | `damageAmount`, `playerMoveSpeed` | 
| private / protected 필드 | `_camelCase` | `_currentHealth` |
| 상수 | UPPER_SNAKE_CASE | `MAX_PLAYER_LEVEL` |
| 스크립트 파일명 | PascalCase | `GameManager.cs` |  