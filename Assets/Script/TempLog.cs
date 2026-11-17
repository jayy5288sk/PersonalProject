using UnityEngine;

// 디버그 전용: 조작 단계·페이즈 전환 로그. F-06/F-08 디버깅에 사용하고 출시 전 제거
public class TempLog : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;
    [SerializeField] private PhaseManager _phaseManager;

    private void OnEnable()
    {
        _stateManager.OnStateChanged += PrintLog;
        _phaseManager.OnPhaseChanged += PrintPhaseLog;
    }

    private void OnDisable()
    {
        _stateManager.OnStateChanged -= PrintLog;
        _phaseManager.OnPhaseChanged -= PrintPhaseLog;
    }

    private void PrintLog(StateType state)
    {
        Debug.Log(state);
    }

    private void PrintPhaseLog(PhaseType phase)
    {
        Debug.Log($"[Phase] {phase}");
    }
}
