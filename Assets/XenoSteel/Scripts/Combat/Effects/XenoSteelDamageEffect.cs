using UnityEngine;

using TurnBasedStrategyFramework.Unity.Units;

using XenoSteel.Core;
using XenoSteel.Units;
using XenoSteel.Information;

using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Combat
{
    [System.Serializable]
    public class XenoSteelDamageEffect : XenoSteelEffect
    {
        [Header("ダメージ性能")]
        public int power = 100;

        [Header("範囲攻撃ダメージ")]
        [Range(0f, 1f)]
        public float areaDamageMultiplier = 0.5f;

        [Header("属性")]
        public string attribute;

        public override void Execute(
            XenoSteelEffectContext context)
        {
            if (context == null)
                return;

            Unit attackerUnit = context.User as Unit;
            Unit targetUnit = context.Target as Unit;

            if (attackerUnit == null || targetUnit == null)
                return;

            XenoSteelInitiative attackerInitiative =
                attackerUnit.GetComponent<XenoSteelInitiative>();

            XenoSteelInitiative targetInitiative =
                targetUnit.GetComponent<XenoSteelInitiative>();

            if (attackerInitiative == null ||
                targetInitiative == null)
                return;

            int damage =
                XenoSteelDamageCalculator.CalculateDamage(
                    attackerInitiative.Stats,
                    power,
                    targetInitiative.Stats
                );

            targetUnit.ModifyHealth(
                -damage,
                attackerUnit
            );

            if (targetUnit.Health <= 0 &&
                targetUnit.PlayerNumber == 0)
            {
                XenoSteelEnemyInformationManager enemyInformationManager =
                    Object.FindFirstObjectByType<XenoSteelEnemyInformationManager>();

                if (enemyInformationManager != null)
                {
                    enemyInformationManager.RemovePlayerInformation(
                        targetUnit
                    );
                }
            }

            targetUnit.InvokeAttacked(
                new UnitAttackedEventArgs(
                    targetUnit,
                    attackerUnit,
                    damage
                )
            );

            Debug.Log(
                $"Damage Effect: " +
                $"Target={targetUnit.name}, " +
                $"Damage={damage}"
            );
        }
    }
}