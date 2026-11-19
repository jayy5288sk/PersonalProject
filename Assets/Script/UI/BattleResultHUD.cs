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
    [SerializeField] private GameObject _resultPanel;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _turnText;
    [SerializeField] private TextMeshProUGUI _enemyKillText;
    [SerializeField] private TextMeshProUGUI _allyDeathText;
    [SerializeField] private Button _nextStageButton;   // 동작 연결은 스테이지 선택 구현 시
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _mainMenuButton;    // 동작 연결은 메인 메뉴 구현 시

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _battleResultJudge.OnBattleResult += HandleBattleResult;
        _retryButton.onClick.AddListener(HandleRetryClicked);
    }

    private void UnbindEvents()
    {
        _battleResultJudge.OnBattleResult -= HandleBattleResult;
        _retryButton.onClick.RemoveListener(HandleRetryClicked);
    }

    // 결과 패널 표시
    private void HandleBattleResult(BattleResult result)
    {
        bool isVictory = result == BattleResult.Victory;

        _resultPanel.SetActive(true);

        _titleText.text = isVictory ? VICTORY_TITLE : DEFEAT_TITLE;
        _turnText.text = $"Turn {_phaseManager.TurnCount}";
        _enemyKillText.text = $"Enemies Defeated {_battleResultJudge.EnemyKillCount}";
        _allyDeathText.text = $"Allies Lost {_battleResultJudge.AllyDeathCount}";

        _nextStageButton.gameObject.SetActive(isVictory);
    }

    // 현재 씬을 다시 불러와 처음 상태로 재시작
    private void HandleRetryClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
