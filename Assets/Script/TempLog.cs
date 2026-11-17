using UnityEngine;

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
