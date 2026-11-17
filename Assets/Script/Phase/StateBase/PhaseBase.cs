public abstract class PhaseBase
{
    public abstract PhaseType PhaseName();

    // 들어가자마자 넘어갈 다음 페이즈를 반환. 머무르면 null
    public virtual PhaseBase PhaseEnter()
    {
        return null;
    }

    public virtual void PhaseExit()
    {
        return;
    }
}
