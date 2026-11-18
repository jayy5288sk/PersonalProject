public static class BattleCalculator
{
    /// <summary>
    /// 계산식 v1: 데미지 = 무장 공격력.
    /// 공격자·방어자는 지금 쓰지 않지만, 보정(지형, 방어 등)이 생겨도 호출부를 고치지 않도록 미리 받는다
    /// </summary>
    public static int CalculateDamage(MapUnit attacker, WeaponData weapon, MapUnit defender)
    {
        return weapon.AttackDamage;
    }
}
