using System;

namespace XenoSteel.Combat
{
    public static class XenoSteelDamageCalculator
    {
        public static int CalculateDamage(
            XenoSteelUnitStats attackerStats,
            SkillData skill,
            XenoSteelUnitStats defenderStats)
        {
            int baseDamage = attackerStats.Attack + skill.power;

            int damage = baseDamage - defenderStats.Armor;

            return Math.Max(damage, 1);
        }
    }
}