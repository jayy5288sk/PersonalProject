using System;
using UnityEngine;

/// <summary>
/// 전투 씬의 시간 흐름(일시정지·재개)을 관리. 상태가 바뀌면 OnPauseChanged로 알림
/// </summary>
public class TimeManager : MonoBehaviour
{
    private const float PAUSED_TIME_SCALE = 0f;
    private const float NORMAL_TIME_SCALE = 1f;

    [SerializeField] private BattleResultJudge _battleResultJudge;

    private bool _isGameOver;

    public bool IsPaused { get; private set; }

    public event Action<bool> OnPauseChanged;   // true = 일시정지, false = 재개

    private void OnEnable() => BindEvents();

    private void OnDisable() => UnbindEvents();

    private void BindEvents()
    {
        _battleResultJudge.OnBattleResult += HandleBattleResult;
    }

    private void UnbindEvents()
    {
        _battleResultJudge.OnBattleResult -= HandleBattleResult;
    }

    public void TogglePause()
    {
        if (IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    // 결과가 나온 뒤에는 결과 패널 위에 일시정지가 겹치지 않도록 무시
    public void Pause()
    {
        if (_isGameOver)
        {
            return;
        }

        SetPaused(true, PAUSED_TIME_SCALE);
    }

    // timeScale은 씬을 바꿔도 유지되므로 씬 이동 전에도 호출해 되돌림
    public void Resume()
    {
        SetPaused(false, NORMAL_TIME_SCALE);
    }

    private void SetPaused(bool isPaused, float timeScale)
    {
        IsPaused = isPaused;
        Time.timeScale = timeScale;
        OnPauseChanged?.Invoke(isPaused);
    }

    private void HandleBattleResult(BattleResult result)
    {
        _isGameOver = true;
    }
}
