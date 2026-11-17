public class EnemyPhase : PhaseBase
{
    private readonly StateManager _stateManager;

    private PhaseBase _nextPhase;

    public EnemyPhase(StateManager stateManager)
    {
        _stateManager = stateManager;
    }

    public void SetPhaseBase(PhaseBase phaseBase)
    {
        _nextPhase = phaseBase;
    }

    public override PhaseType PhaseName()
    {
        return PhaseType.Enemy;
    }

    public override PhaseBase PhaseEnter()
    {
        // 적 페이즈 동안 플레이어 클릭·버튼 입력 차단
        _stateManager.SetInputLock(true);

        // 임시: 적 AI가 없으므로 바로 아군 페이즈로 복귀. F-08에서 적 AI 연결 후 null 반환으로 교체
        return _nextPhase;
    }

    public override void PhaseExit()
    {
        _stateManager.SetInputLock(false);
    }
}
