# 안티패턴(피해야 할 사항)
- Update()에서 매 프레임 GetComponent, Find, Camera.main 호출
- 다량의 기능이 들어간 God Object 생성
- 대량 오브젝트를 Instantiate/Destroy로 반복 생성·삭제 (오브젝트 풀링 안 함)
- SendMessage, 문자열 기반 호출
- Update()에서 문자열 연결 등으로 GC 할당 발생
- 싱글톤 남용, 오브젝트 간 강한 결합
- 물리 로직을 Update()에 작성 
- 무분별한 Find 계열 함수 사용
- 의미를 알 수 없는 숫자인 매직 넘버 추가