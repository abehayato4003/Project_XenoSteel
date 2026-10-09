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
using XenoSteel.Information;
using System.Threading.Tasks;


namespace XenoSteel.Combat
{
    public class XenoSteelAttackAbility : Ability
    {
        private SkillData _currentSkill;
        private HashSet<IUnit> _attackableUnits;
        private IGridController _gridController;

        private HashSet<ICell> _cellsInRange;
        private ICell _highlightedCell;

        public SkillData CurrentSkill => _currentSkill;

        [SerializeField]
        private XenoSteelAttackPresentation attackPresentation;
        [SerializeField]
        private XenoSteelInformationManager informationManager;

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
                _attackableUnits = null;
            }
        }

        public void SetCurrentSkill(SkillData skill)
        {
            if (skill == null || _gridController == null)
            {
                return;
            }

            _gridController.GridState =
                new GridStateUnitSelected(UnitReference, this);

            if (_attackableUnits != null)
            {
                _gridController.UnitManager.UnMark(_attackableUnits);
                _attackableUnits = null;
            }

            if (_cellsInRange != null)
            {
                _gridController.CellManager.UnMark(_cellsInRange);
                _cellsInRange = null;
            }

            _currentSkill = skill;

            if (_currentSkill.targetType == SkillTargetType.None)
            {
                // None Skillはここでは実行しない
                // UI側でYesを押したときにExecuteCurrentSkill()を呼ぶ
                return;
            }

            if (_currentSkill.targetType == SkillTargetType.Cell)
            {
                DisplayCellTargets(_gridController);
                return;
            }

            _attackableUnits = new HashSet<IUnit>(
                XenoSteelAttackTargeting.GetAttackableTargets(
                    UnitReference,
                    _currentSkill,
                    _gridController));

            Display(_gridController);
        }

        public void ClearCurrentSkill()
        {
            if (_gridController != null)
            {
                if (_attackableUnits != null)
                {
                    _gridController.UnitManager.UnMark(_attackableUnits);
                }

                if (_cellsInRange != null)
                {
                    _gridController.CellManager.UnMark(_cellsInRange);
                }

                _gridController.GridState = new GridStateAwaitInput();
            }

            _attackableUnits = null;
            _cellsInRange = null;
            _highlightedCell = null;
            _currentSkill = null;
        }

        public void ConfirmCurrentSkill()
        {
            if (_currentSkill == null)
            {
                return;
            }

            if (_currentSkill.targetType != SkillTargetType.None)
            {
                return;
            }

            ExecuteNoTargetSkill(_gridController);
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

            if (_cellsInRange != null)
            {
                gridController.CellManager.UnMark(_cellsInRange);
            }

            _cellsInRange = null;
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

            if (_currentSkill.targetType != SkillTargetType.Unit)
            {
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

            if (attackerUnit == null)
            {
                return;
            }

            var attackerInitiative =
                attackerUnit.GetComponent<XenoSteelInitiative>();

            if (attackerInitiative == null)
            {
                return;
            }

            int energyCost = _currentSkill.energyCost;

            if (!attackerInitiative.Stats.CanConsumeEN(energyCost))
            {
                Debug.Log(
                    $"EN不足: Skill={_currentSkill.skillName}, " +
                    $"必要EN={energyCost}, " +
                    $"現在EN={attackerInitiative.Stats.EN}"
                );

                return;
            }

            await ExecuteUnitSkill(
                unit,
                _currentSkill,
                gridController
            );
        }

        private async Task<bool> ExecuteUnitSkill(
            IUnit selectedTarget,
            SkillData skill,
            IGridController gridController)
        {
            if (selectedTarget == null ||
                skill == null)
            {
                return false;
            }

            var attackerUnit = UnitReference as Unit;
            var targetUnit = selectedTarget as Unit;

            if (attackerUnit == null ||
                targetUnit == null)
            {
                return false;
            }

            var attackerInitiative =
                attackerUnit.GetComponent<XenoSteelInitiative>();

            if (attackerInitiative == null)
            {
                return false;
            }

            int energyCost = skill.energyCost;

            if (!attackerInitiative.Stats.CanConsumeEN(energyCost))
            {
                return false;
            }

            // ----------------------------------------
            // 攻撃形状による対象取得
            // ----------------------------------------

            var affectedTargets =
                XenoSteelAttackTargeting.GetAffectedTargets(
                    UnitReference,
                    selectedTarget,
                    skill,
                    gridController
                );

            // ----------------------------------------
            // areaによる追加対象取得
            // ----------------------------------------

            var areaTargets = new List<IUnit>();

            if (skill.area <= 0)
            {
                areaTargets.AddRange(affectedTargets);
            }
            else
            {
                var enemyUnits =
                    gridController.UnitManager.GetEnemyUnits(
                        UnitReference.PlayerNumber
                    );

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
                                target.CurrentCell
                            );

                        if (distance <= skill.area &&
                            !areaTargets.Contains(enemy))
                        {
                            areaTargets.Add(enemy);
                        }
                    }
                }
            }

            if (areaTargets.Count == 0)
            {
                return false;
            }

            // ----------------------------------------
            // 実際の攻撃
            // ----------------------------------------

            foreach (var target in areaTargets)
            {
                var targetUnitInArea = target as Unit;

                if (targetUnitInArea == null)
                {
                    continue;
                }

                var targetInitiative =
                    targetUnitInArea.GetComponent<XenoSteelInitiative>();

                if (targetInitiative == null)
                {
                    continue;
                }

                Debug.Log(
                    $"Area Target: {targetUnitInArea.name}"
                );

                // 戦闘演出
                if (attackPresentation != null)
                {
                    await attackPresentation.PlayAttackPresentation(
                        skill.presentation
                    );
                }

                var turnResolver =
                    gridController.TurnResolver as XenoSteelTurnResolver;

                if (turnResolver == null)
                {
                    return false;
                }

                // ----------------------------------------
                // AttackCommandを先に実行
                // ----------------------------------------

                await UnitReference.HumanExecuteAbility(
                    new XenoSteelAttackCommand(
                        target,
                        (int)attackerUnit.ActionPoints
                    ),
                    gridController
                );

                // ----------------------------------------
                // AttackCommand完了後にEffect実行
                // ----------------------------------------

                var effectContext =
                    new XenoSteelEffectContext(
                        UnitReference,
                        target,
                        null,
                        gridController,
                        turnResolver,
                        informationManager,
                        turnResolver.CurrentRound
                    );

                if (skill.effects != null)
                {
                    foreach (var effect in skill.effects)
                    {
                        if (effect == null)
                        {
                            continue;
                        }

                        effect.Execute(effectContext);
                    }
                }
            }

            // ----------------------------------------
            // EN消費
            // ----------------------------------------

            if (!attackerInitiative.Stats.ConsumeEN(energyCost))
            {
                return false;
            }

            Debug.Log(
                $"EN consumed: {energyCost}, " +
                $"EN after attack: {attackerInitiative.Stats.EN}"
            );

            // ----------------------------------------
            // 攻撃スキル完了
            // ----------------------------------------

            gridController.EndTurn();

            return true;
        }

        public async Task<bool> ExecuteAISkill(
            SkillData skill,
            IUnit target,
            IGridController gridController)
        {
            if (skill == null ||
                target == null)
            {
                return false;
            }

            if (skill.targetType != SkillTargetType.Unit)
            {
                return false;
            }

            if (UnitReference.ActionPoints <= 0)
            {
                return false;
            }

            _currentSkill = skill;

            return await ExecuteUnitSkill(
                target,
                skill,
                gridController
            );
        }

        public override async void OnCellClicked(
            ICell cell,
            IGridController gridController)
        {
            if (_currentSkill == null ||
                _currentSkill.targetType != SkillTargetType.Cell)
            {
                return;
            }

            if (_cellsInRange == null ||
                !_cellsInRange.Contains(cell))
            {
                return;
            }

            if (UnitReference.ActionPoints <= 0)
            {
                return;
            }

            XenoSteelTurnResolver turnResolver =
                FindFirstObjectByType<XenoSteelTurnResolver>();

            XenoSteelEffectContext context =
                new XenoSteelEffectContext(
                    UnitReference,
                    null,
                    cell,
                    gridController,
                    turnResolver,
                    informationManager,
                    turnResolver.CurrentRound
                );

            foreach (var effect in _currentSkill.effects)
            {
                if (effect == null)
                {
                    continue;
                }

                effect.Execute(context);
            }

            // EN消費
            var attackerUnit = UnitReference as Unit;
            if (attackerUnit == null)
            {
                return;
            }

            var initiative =
                attackerUnit.GetComponent<XenoSteelInitiative>();

            if (initiative != null)
            {
                initiative.Stats.ConsumeEN(_currentSkill.energyCost);
            }

            // AP消費
            await UnitReference.HumanExecuteAbility(
                new XenoSteelCellSkillCommand(
                    (int)UnitReference.ActionPoints),
                gridController);

            // ターン終了
            gridController.EndTurn();
        }

        private int _unitPlayerNumber()
        {
            return UnitReference.PlayerNumber;
        }

        private async void ExecuteNoTargetSkill(
            IGridController gridController)
        {
            if (_currentSkill == null)
            {
                return;
            }

            if (UnitReference.ActionPoints <= 0)
            {
                return;
            }

            var attackerUnit = UnitReference as Unit;

            if (attackerUnit == null)
            {
                return;
            }

            var attackerInitiative =
                attackerUnit.GetComponent<XenoSteelInitiative>();

            if (attackerInitiative == null)
            {
                return;
            }

            var stats = attackerInitiative.Stats;

            int energyCost =
                _currentSkill.energyCost;

            if (!stats.CanConsumeEN(energyCost))
            {
                Debug.Log(
                    $"EN不足: Skill={_currentSkill.skillName}, " +
                    $"必要EN={energyCost}, " +
                    $"現在EN={stats.EN}");
                return;
            }

            var turnResolver =
                gridController.TurnResolver as XenoSteelTurnResolver;

            if (turnResolver == null)
            {
                return;
            }

            var effectContext =
                new XenoSteelEffectContext(
                    UnitReference,
                    null,
                    null,
                    gridController,
                    turnResolver,
                    informationManager,
                    turnResolver.CurrentRound
                );

            if (_currentSkill.effects != null)
            {
                foreach (var effect in _currentSkill.effects)
                {
                    if (effect == null)
                        continue;

                    effect.Execute(effectContext);
                }
            }

            if (stats.ConsumeEN(energyCost))
            {
                Debug.Log(
                    $"EN consumed: {energyCost}, " +
                    $"EN after skill: {stats.EN}");
            }

            await UnitReference.HumanExecuteAbility(
                new XenoSteelCellSkillCommand(
                    (int)UnitReference.ActionPoints),
                gridController);

            // AP消費
            await UnitReference.HumanExecuteAbility(
                new XenoSteelCellSkillCommand(
                    (int)UnitReference.ActionPoints),
                gridController);

            // ターン終了
            gridController.EndTurn();
        }

        public void ExecuteCurrentSkill()
        {
            if (_currentSkill == null)
            {
                return;
            }

            if (_currentSkill.targetType != SkillTargetType.None)
            {
                return;
            }

            ExecuteNoTargetSkill(_gridController);
        }

        private void DisplayCellTargets(IGridController gridController)
        {
            if (_currentSkill == null || UnitReference.CurrentCell == null)
            {
                return;
            }

            _cellsInRange = new HashSet<ICell>(
                gridController.CellManager.GetCells()
                    .Where(cell =>
                        cell.GetDistance(UnitReference.CurrentCell) <= _currentSkill.range)
            );

            gridController.CellManager.MarkAsReachable(_cellsInRange);
        }

        public override void OnCellHighlighted(
            ICell cell,
            IGridController gridController)
        {
            if (_currentSkill == null ||
                _currentSkill.targetType != SkillTargetType.Cell ||
                _cellsInRange == null)
            {
                return;
            }

            if (!_cellsInRange.Contains(cell))
            {
                return;
            }

            _highlightedCell = cell;

            gridController.CellManager.MarkAsPath(
                new[] { cell },
                UnitReference.CurrentCell
            );
        }

        public override void OnCellDehighlighted(
            ICell cell,
            IGridController gridController)
        {
            if (_currentSkill == null ||
                _currentSkill.targetType != SkillTargetType.Cell)
            {
                return;
            }

            if (_highlightedCell != cell)
            {
                return;
            }

            gridController.CellManager.UnMark(cell);

            if (_cellsInRange != null &&
                _cellsInRange.Contains(cell))
            {
                gridController.CellManager.MarkAsReachable(cell);
            }

            _highlightedCell = null;
        }
    }
}