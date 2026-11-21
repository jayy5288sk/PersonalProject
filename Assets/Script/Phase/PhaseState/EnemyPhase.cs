public class EnemyPhase : PhaseBase
{
    private readonly StateManager _stateManager;
    private readonly EnemyAIController _enemyAIController;

    public EnemyPhase(StateManager stateManager, EnemyAIController enemyAIController)
    {
        _stateManager = stateManager;
        _enemyAIController = enemyAIController;
    }

    public override PhaseType PhaseName()
    {
        return PhaseType.Enemy;
    }

    public override PhaseBase PhaseEnter()
    {
        // 적 페이즈 동안 플레이어 클릭·버튼 입력 차단
        _stateManager.SetInputLock(true);

        // 적 행동은 대기 시간이 있어 바로 끝나지 않음. 완료 시 PhaseManager가 아군 페이즈로 전환
        _enemyAIController.StartEnemyTurn();
        return null;
    }

    public override void PhaseExit()
    {
        _stateManager.SetInputLock(false);
    }
}
