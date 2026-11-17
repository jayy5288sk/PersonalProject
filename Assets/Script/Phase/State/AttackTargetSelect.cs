using UnityEngine;

public class AttackTargetSelect : StateBase
{
    private readonly StateContext _stateContext;

    private StateBase _nextState;

    public AttackTargetSelect(StateContext stateContext)
    {
        _stateContext = stateContext;
    }

    public void SetStateBase(StateBase stateBase)
    {
        _nextState = stateBase;
    }

    public override StateType StateName()
    {
        return StateType.AttackTargetSelect;
    }

    public override void StateEnter()
    {
        Debug.Log($"{_stateContext.SelectedUnit.name} 공격 대상 선택");
    }

    public override StateBase SectionClick(Vector2Int section)
    {
        // 임시: 6단계 확인용 탈출 경로. 어느 칸을 클릭해도 커맨드 선택으로 복귀
        // F-06에서 공격 대상 판별·공격 처리로 교체
        return _nextState;
    }
}
