using UnityEngine;

public class TempLog : MonoBehaviour
{
    [SerializeField] private StateManager _stateManager;

    private void OnEnable()
    {
        _stateManager.OnStateChanged += PrintLog;
    }

    private void OnDisable()
    {
        _stateManager.OnStateChanged -= PrintLog;
    }
    
    private void PrintLog(StateType state)
    {
        Debug.Log(state);
    }
}
