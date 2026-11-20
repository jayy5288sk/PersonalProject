using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleResultHUD : MonoBehaviour
{
    private const string VICTORY_TITLE = "Stage Clear";
    private const string DEFEAT_TITLE = "Defeated";

    [SerializeField] private BattleResultJudge _battleResultJudge;
    [SerializeField] private PhaseManager _phaseManager;
    [SerializeField] private HeadDataLoader _headDataLoader;
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _enemyKillText;
    [SerializeField] private TextMeshProUGUI _allyDeathText;
    [SerializeField] private Button _nextStageButton;
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private TimeManager _timeManager;

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _battleResultJudge.OnBattleResult += HandleBattleResult;
        _nextStageButton.onClick.AddListener(HandleNextStageClicked);
        _retryButton.onClick.AddListener(HandleRetryClicked);
        _mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
    }

    private void UnbindEvents()
    {
        _battleResultJudge.OnBattleResult -= HandleBattleResult;
        _nextStageButton.onClick.RemoveListener(HandleNextStageClicked);
        _retryButton.onClick.RemoveListener(HandleRetryClicked);
        _mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
    }

    // 결과 패널 표시
    private void HandleBattleResult(BattleResult result)
    {
        bool isVictory = result == BattleResult.Victory;

        _resultPanel.SetActive(true);

        _titleText.text = isVictory ? VICTORY_TITLE : DEFEAT_TITLE;
        _turnText.text = $"{_phaseManager.TurnCount}";
        _enemyKillText.text = $"{_battleResultJudge.EnemyKillCount}";
        _allyDeathText.text = $"{_battleResultJudge.AllyDeathCount}";

        _nextStageButton.gameObject.SetActive(isVictory);
    }

    // 다음 스테이지로 진행. 마지막 스테이지 다음은 첫 스테이지로 순환
    private void HandleNextStageClicked()
    {
        StageSelection.SelectedStageIndex = (StageSelection.SelectedStageIndex + 1) % _headDataLoader.StageCount;
        LoadSceneWithTimeReset(SceneIndex.BATTLE);
    }

    // 선택한 스테이지 번호가 그대로 남아 있으므로 같은 스테이지로 재시작
    private void HandleRetryClicked()
    {
        LoadSceneWithTimeReset(SceneIndex.BATTLE);
    }

    private void HandleMainMenuClicked()
    {
        LoadSceneWithTimeReset(SceneIndex.MAIN_MENU);
    }

    // timeScale은 씬을 바꿔도 유지되므로 이동 전에 되돌림
    private void LoadSceneWithTimeReset(int sceneIndex)
    {
        _timeManager.Resume();
        SceneManager.LoadScene(sceneIndex);
    }
}
