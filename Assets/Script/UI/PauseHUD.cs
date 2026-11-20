using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 일시정지 입력(Esc, 버튼)을 TimeManager에 전달하고, 일시정지 상태에 맞춰 패널을 표시
/// </summary>
public class PauseHUD : MonoBehaviour
{
    [SerializeField] private InputActionReference _pauseAction;
    [SerializeField] private TimeManager _timeManager;
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _pauseButton;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _mainMenuButton;

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _pauseAction.action.Enable();
        _pauseAction.action.performed += HandlePauseInput;
        _timeManager.OnPauseChanged += HandlePauseChanged;
        _pauseButton.onClick.AddListener(HandlePauseClicked);
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
    }

    private void UnbindEvents()
    {
        _pauseAction.action.performed -= HandlePauseInput;
        _pauseAction.action.Disable();
        _timeManager.OnPauseChanged -= HandlePauseChanged;
        _pauseButton.onClick.RemoveListener(HandlePauseClicked);
        _resumeButton.onClick.RemoveListener(HandleResumeClicked);
        _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
    }

    // Esc: 일시정지 중이면 재개, 아니면 일시정지
    private void HandlePauseInput(InputAction.CallbackContext context)
    {
        _timeManager.TogglePause();
    }

    private void HandlePauseClicked()
    {
        _timeManager.Pause();
    }

    private void HandleResumeClicked()
    {
        _timeManager.Resume();
    }

    // 멈춘 시간을 먼저 되돌린 뒤 이동. 되돌리지 않으면 다음 전투가 멈춘 채로 시작됨
    private void HandleMainMenuClicked()
    {
        _timeManager.Resume();
        SceneManager.LoadScene(SceneIndex.MAIN_MENU);
    }

    private void HandlePauseChanged(bool isPaused)
    {
        _pausePanel.SetActive(isPaused);
    }
}
