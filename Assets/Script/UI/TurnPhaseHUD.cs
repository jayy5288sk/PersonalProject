using TMPro;
using UnityEngine;

public class TurnPhaseHUD : MonoBehaviour
{
    private const string ALLY_PHASE_NAME = "Ally Phase";
    private const string ENEMY_PHASE_NAME = "Enemy Phase";

    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private TextMeshProUGUI _turnPhaseText;

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _phaseManager.OnPhaseChanged += HandlePhaseChanged;
    }

    private void UnbindEvents()
    {
        _phaseManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    // TurnCount는 이벤트 발송 전에 증가하므로 여기서 읽으면 새 턴 번호
    private void HandlePhaseChanged(PhaseType phase)
    {
        string phaseName = phase == PhaseType.Ally ? ALLY_PHASE_NAME : ENEMY_PHASE_NAME;
        _turnPhaseText.text = $"Turn {_phaseManager.TurnCount} · {phaseName}";
    }
}
