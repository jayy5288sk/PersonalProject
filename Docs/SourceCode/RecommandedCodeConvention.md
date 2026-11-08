# 권장 패턴
- Awake/Start에서 GetComponent, Find 결과를 받는 함수인 CacheComponents나 Initialization을 만들어 한 번만 캐싱하여 적용한다.
- 단일 책임 원칙(SRP)에 따라 기능별로 컴포넌트를 분리하고, 작은 컴포넌트를 조합(Composition)해서 구성
- 대량 오브젝트는 오브젝트 풀링(`ObjectPool<T>` 또는 자체 풀)으로 비활성화 후 재사용
- 인터페이스, 직접 참조, C# `event`/`Action`/`UnityEvent` 등 타입 안전한 호출 사용 (문자열이 필요하면 `nameof()` 활용)
- Update()에서는 GC 할당 없이 처리: 값이 바뀔 때만 갱신
- Update() 단에서 처리하는 것에 대해서 함수로 캐싱하여 사용
- `StringBuilder` 재사용, 배열·리스트 재사용, `NonAlloc` API를 사용하여 콜라이더에 대한 충돌 검사
- 의존성 주입(인스펙터·생성자·Dependecy Injection 컨테이너), 이벤트/메시지 버스, ScriptableObject 기반 데이터·이벤트 채널로 느슨한 결합 유지 (싱글톤은 정말 전역인 소수 서비스에만 제한적으로 사용)
- 물리 로직은 FixedUpdate()에서 처리하고, 입력은 Update()에서 받아 FixedUpdate()에 전달
- Find 계열 대신 `[SerializeField]` 참조 연결, 생성 시점에 등록하는 매니저/레지스트리 패턴 사용
- 매직 넘버는 `const`, `static readonly`, enum, `[SerializeField]` 필드, ScriptableObject 설정 데이터로 의미 있는 이름을 붙여 분리
- 이벤트는 OnEnable에서 구독하고 OnDisable에서 해제해 누수와 null 참조 방지
- 수천 단위 이상의 대량 오브젝트는 풀링에 더해 Job System/Burst, GPU Instancing, DOTS(ECS) 등 데이터 지향 접근 검토
- 최적화는 추측하지 말고 Profiler로 병목과 GC.Alloc을 측정한 뒤 적용

- 소스 코드를 작성함에 있어 위 사항들을 제외하고 분기점이 갈리는 지점이 존재할 때, 바로 구현하기 보다는 사용자에게 선택을 할 수 있도록 넘버링을 하여 보여준다.
- 그리고 선택을 받은 사항에 대해서 구현을 진행한다.