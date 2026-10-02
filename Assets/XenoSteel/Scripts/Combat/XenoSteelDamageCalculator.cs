using System;

namespace XenoSteel.Combat
{
    public static class XenoSteelDamageCalculator
    {
        public static int CalculateDamage(
            XenoSteelUnitStats attackerStats,
            int power,
            XenoSteelUnitStats defenderStats)
        {
            int baseDamage = attackerStats.Attack + power;

            int damage = baseDamage - defenderStats.Armor;

            return Math.Max(damage, 1);
        }
    }
}