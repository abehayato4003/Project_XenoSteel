using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Unity.Units.Abilities;
using XenoSteel.Core;

using System.Collections.Generic;
using System.Linq;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

using UnityEngine;

namespace XenoSteel.Combat
{
    public class XenoSteelAttackAbility : Ability
    {
        private SkillData _currentSkill;
        private HashSet<IUnit> _attackableUnits;

        public override void Initialize(IGridController gridController)
        {
            base.Initialize(gridController);

            var unit = UnitReference as Unit;

            if (unit == null)
            {
                return;
            }

            var initiative = unit.GetComponent<XenoSteelInitiative>();

            if (initiative == null)
            {
                return;
            }

            var unitData = initiative.UnitData;

            if (unitData.skills == null || unitData.skills.Length == 0)
            {
                return;
            }

            _currentSkill = unitData.skills[0];
        }

        public SkillData CurrentSkill => _currentSkill;

        public override void OnAbilitySelected(IGridController gridController)
        {
            if (_currentSkill == null)
            {
                return;
            }

            var enemyUnits = gridController.UnitManager
                .GetEnemyUnits(gridController.TurnContext.CurrentPlayer);

            _attackableUnits = new HashSet<IUnit>(
                enemyUnits.Where(unit =>
                    unit.CurrentCell.GetDistance(UnitReference.CurrentCell)
                    <= _currentSkill.range));
        }

        public override async void Display(IGridController gridController)
        {
            if (_attackableUnits == null)
            {
                return;
            }

            await gridController.UnitManager.MarkAsTargetable(_attackableUnits);
        }

        public override void OnUnitClicked(IUnit unit, IGridController gridController)
        {
            Debug.Log("XenoSteelAttackAbility.OnUnitClicked");

            if (UnitReference.ActionPoints <= 0)
            {
                return;
            }


            if (_currentSkill == null ||
                _attackableUnits == null ||
                !_attackableUnits.Contains(unit))
            {
                return;
            }

            var attackerUnit = UnitReference as Unit;
            var defenderUnit = unit as Unit;

            if (attackerUnit == null || defenderUnit == null)
            {
                return;
            }

            var attackerInitiative = attackerUnit.GetComponent<XenoSteelInitiative>();
            var defenderInitiative = defenderUnit.GetComponent<XenoSteelInitiative>();

            if (attackerInitiative == null || defenderInitiative == null)
            {
                return;
            }

            var damage = XenoSteelDamageCalculator.CalculateDamage(
                attackerInitiative.Stats,
                _currentSkill,
                defenderInitiative.Stats);

                Debug.Log($"XenoSteel Damage: {damage}");
                Debug.Log($"Enemy Health before attack: {defenderUnit.Health}");

            UnitReference.HumanExecuteAbility(
            new AttackCommand(unit, damage, (int)attackerUnit.ActionPoints),
            gridController);
        }
    }
}