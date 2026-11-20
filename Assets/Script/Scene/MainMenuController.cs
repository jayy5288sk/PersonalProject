using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private const int FIRST_STAGE_INDEX = 0;
    private const int SECOND_STAGE_INDEX = 1;

    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _stageSelectPanel;
    [SerializeField] private Button _gameStartButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _firstStageButton;
    [SerializeField] private Button _secondStageButton;

    private void Awake()
    {
        // 시작 시에는 메인 패널만
        ShowMain();
    }

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _gameStartButton.onClick.AddListener(HandleGameStartClicked);
        _quitButton.onClick.AddListener(HandleQuitClicked);
        _backButton.onClick.AddListener(HandleBackClicked);
        _firstStageButton.onClick.AddListener(PressedFirstButton);
        _secondStageButton.onClick.AddListener(PressedSecondButton);
    }

    private void UnbindEvents()
    {
        _gameStartButton.onClick.RemoveListener(HandleGameStartClicked);
        _quitButton.onClick.RemoveListener(HandleQuitClicked);
        _backButton.onClick.RemoveListener(HandleBackClicked);
        _firstStageButton.onClick.RemoveListener(PressedFirstButton);
        _secondStageButton.onClick.RemoveListener(PressedSecondButton);
    }

    // 게임 시작 버튼: 스테이지 선택 패널을 연다.
    private void HandleGameStartClicked()
    {
        _mainPanel.SetActive(false);
        _stageSelectPanel.SetActive(true);
    }

    // Back 버튼: 메인 패널로 돌아간다.
    private void HandleBackClicked() => ShowMain();

    // 게임 종료 버튼: 빌드에서는 앱을 종료하고, 에디터에서는 플레이 모드를 멈춘다.
    private void HandleQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PressedFirstButton()
    {
        StartStage(FIRST_STAGE_INDEX);
    }

    private void PressedSecondButton()
    {
        StartStage(SECOND_STAGE_INDEX);
    }

    private void StartStage(int stageIndex)
    {
        StageSelection.SelectedStageIndex = stageIndex;
        SceneManager.LoadScene(SceneIndex.BATTLE);
    }

    private void ShowMain()
    {
        _stageSelectPanel.SetActive(false);
        _mainPanel.SetActive(true);
    }
}
