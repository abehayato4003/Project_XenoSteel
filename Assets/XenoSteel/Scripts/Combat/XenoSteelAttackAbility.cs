using System.Collections.Generic;
using System.Linq;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Controllers.GridStates;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;
using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Unity.Units.Abilities;
using UnityEngine;
using XenoSteel.Core;
using XenoSteel.Units;


namespace XenoSteel.Combat
{
    public class XenoSteelAttackAbility : Ability
    {
        private SkillData _currentSkill;
        private HashSet<IUnit> _attackableUnits;
        private IGridController _gridController;

        public SkillData CurrentSkill => _currentSkill;

        [SerializeField]
        private XenoSteelAttackPresentation attackPresentation;

        public override void Initialize(IGridController gridController)
        {
            base.Initialize(gridController);

            _gridController = gridController;
            _currentSkill = null;
            _attackableUnits = null;
        }

        public override void OnAbilitySelected(IGridController gridController)
        {
            _gridController = gridController;

            if (_attackableUnits != null)
            {
                _gridController.UnitManager.UnMark(_attackableUnits);
            }

            _currentSkill = null;
            _attackableUnits = null;
        }

        public void SetCurrentSkill(SkillData skill)
        {
            if (skill == null || _gridController == null)
            {
                return;
            }

            if (_attackableUnits != null)
            {
                _gridController.UnitManager.UnMark(_attackableUnits);
            }

            _currentSkill = skill;

            _attackableUnits = new HashSet<IUnit>(
                XenoSteelAttackTargeting.GetAttackableTargets(
                    UnitReference,
                    _currentSkill,
                    _gridController));

            Display(_gridController);
        }

        public override async void Display(IGridController gridController)
        {
            if (_currentSkill == null || _attackableUnits == null)
            {
                return;
            }

            await gridController.UnitManager.MarkAsTargetable(
                _attackableUnits);
        }

        public override void CleanUp(IGridController gridController)
        {
            if (_attackableUnits != null)
            {
                gridController.UnitManager.UnMark(_attackableUnits);
            }
        }

        public override async void OnUnitClicked(
            IUnit unit,
            IGridController gridController)
        {
            if (_currentSkill == null)
            {
                Debug.Log("Skill未選択");
                return;
            }

            if (UnitReference.ActionPoints <= 0)
            {
                return;
            }

            if (_attackableUnits == null ||
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

            var attackerInitiative =
                attackerUnit.GetComponent<XenoSteelInitiative>();

            var defenderInitiative =
                defenderUnit.GetComponent<XenoSteelInitiative>();

            if (attackerInitiative == null ||
                defenderInitiative == null)
            {
                return;
            }

            var stats = attackerInitiative.Stats;
            int energyCost = _currentSkill.energyCost;

            // EN確認
            if (!stats.CanConsumeEN(energyCost))
            {
                Debug.Log(
                    $"EN不足: Skill={_currentSkill.skillName}, " +
                    $"必要EN={energyCost}, " +
                    $"現在EN={stats.EN}");

                return;
            }

            // ----------------------------------------
            // 攻撃形状による対象取得
            // ----------------------------------------

            var affectedTargets =
                XenoSteelAttackTargeting.GetAffectedTargets(
                    UnitReference,
                    unit,
                    _currentSkill,
                    _gridController);

            // ----------------------------------------
            // areaによる追加対象取得
            // ----------------------------------------

            var areaTargets = new List<IUnit>();

            if (_currentSkill.area <= 0)
            {
                // area = 0なら、攻撃形状で決まった対象をそのまま使用
                areaTargets.AddRange(affectedTargets);
            }
            else
            {
                // 攻撃形状で決まった対象それぞれを中心として
                // area範囲内の敵を追加
                var enemyUnits = _gridController.UnitManager
                    .GetEnemyUnits(_unitPlayerNumber());

                

                foreach (var target in affectedTargets)
                {
                    if (target == null ||
                        target.CurrentCell == null)
                    {
                        continue;
                    }

                    foreach (var enemy in enemyUnits)
                    {
                        if (enemy == null ||
                            enemy.CurrentCell == null)
                        {
                            continue;
                        }

                        int distance =
                            enemy.CurrentCell.GetDistance(
                                target.CurrentCell);

                        if (distance <= _currentSkill.area &&
                            !areaTargets.Contains(enemy))
                        {
                            areaTargets.Add(enemy);
                        }
                    }
                }
            }

            // // ----------------------------------------
            // // 選択した攻撃対象の方向を向く
            // // ----------------------------------------

            //     var facing =
            //         attackerUnit.GetComponent<XenoSteelUnitFacing>();

            //     if (facing != null)
            //     {
            //         facing.SetAttacking(true);
            //     }

            //     if (facing != null)
            //     {
            //         Vector2Int attackerPosition =
            //             new Vector2Int(
            //                 attackerUnit.CurrentCell.GridCoordinates.x,
            //                 attackerUnit.CurrentCell.GridCoordinates.y);

            //         Vector2Int targetPosition =
            //             new Vector2Int(
            //                 defenderUnit.CurrentCell.GridCoordinates.x,
            //                 defenderUnit.CurrentCell.GridCoordinates.y);

            //         Vector2Int difference =
            //             targetPosition - attackerPosition;

            //         if (Mathf.Abs(difference.x) >= Mathf.Abs(difference.y))
            //         {
            //             facing.SetDirection(
            //                 difference.x >= 0
            //                     ? XenoSteelUnitFacing.FacingDirection.Right
            //                     : XenoSteelUnitFacing.FacingDirection.Left);
            //         }
            //         else
            //         {
            //             facing.SetDirection(
            //                 difference.y >= 0
            //                     ? XenoSteelUnitFacing.FacingDirection.Up
            //                     : XenoSteelUnitFacing.FacingDirection.Down);
            //         }

            //         Debug.Log(
            //             $"Attack Facing: " +
            //             $"Attacker={attackerPosition}, " +
            //             $"Target={targetPosition}, " +
            //             $"Difference={difference}, " +
            //             $"Direction={facing.Direction}");

                    
            //     }
            // Debug.Log(
            //     $"XenoSteel Area Attack: " +
            //     $"Skill={_currentSkill.skillName}, " +
            //     $"Range={_currentSkill.range}, " +
            //     $"Area={_currentSkill.area}, " +
            //     $"TargetCount={areaTargets.Count}");

            

            // ----------------------------------------
            // 現段階では選択した対象へ攻撃
            // ----------------------------------------

            var damage =
                XenoSteelDamageCalculator.CalculateDamage(
                    stats,
                    _currentSkill,
                    defenderInitiative.Stats);

            Debug.Log(
                $"XenoSteel Skill: {_currentSkill.skillName}");

            Debug.Log(
                $"XenoSteel Damage: {damage}");

            Debug.Log(
                $"EN before attack: {stats.EN}");



            

            foreach (var target in areaTargets)
            {
                var targetUnit = target as Unit;

                if (targetUnit == null)
                {
                    continue;
                }

                var targetInitiative =
                    targetUnit.GetComponent<XenoSteelInitiative>();

                if (targetInitiative == null)
                {
                    continue;
                }

                var targetDamage =
                    XenoSteelDamageCalculator.CalculateDamage(
                        stats,
                        _currentSkill,
                        targetInitiative.Stats);

                // 選択した中心対象以外はareaDamageMultiplierを適用
                if (target != unit)
                {
                    targetDamage = Mathf.RoundToInt(
                        targetDamage * _currentSkill.areaDamageMultiplier);

                    targetDamage = Mathf.Max(targetDamage, 1);
                }

                Debug.Log(
                    $"Area Target: {targetUnit.name}, " +
                    $"Damage={targetDamage}");



                if (attackPresentation != null)
                {
                    await attackPresentation.PlayAttackPresentation(
                        _currentSkill.presentation
                    );
                }

                await UnitReference.HumanExecuteAbility(
                    new AttackCommand(
                        target,
                        targetDamage,
                        (int)attackerUnit.ActionPoints),
                    gridController);

            }

            // if (facing != null)
            // {
            //     facing.SetAttacking(false);
            // }


            // 攻撃完了後にEN消費
            if (stats.ConsumeEN(energyCost))
            {
                Debug.Log(
                    $"EN consumed: {energyCost}, " +
                    $"EN after attack: {stats.EN}");
            }
        }

        public override void OnCellClicked(
            ICell cell,
            IGridController gridController)
        {
            gridController.GridState =
                new GridStateAwaitInput();
        }

        private int _unitPlayerNumber()
        {
            return UnitReference.PlayerNumber;
        }

        
    }
}