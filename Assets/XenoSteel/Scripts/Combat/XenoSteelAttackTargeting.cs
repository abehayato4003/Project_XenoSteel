using System.Collections.Generic;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

using System;
using XenoSteel.Units;

namespace XenoSteel.Combat
{
    /// <summary>
    /// XenoSteelの攻撃対象・攻撃範囲を判定する。
    /// PlayerとAIの両方から使用する。
    /// </summary>
    public static class XenoSteelAttackTargeting
    {
        /// <summary>
        /// 指定したSkillで攻撃可能な敵を取得する。
        /// </summary>
        public static List<IUnit> GetAttackableTargets(
            IUnit attacker,
            SkillData skill,
            IGridController gridController)
        {
            var targets = new List<IUnit>();

            if (attacker == null ||
                skill == null ||
                gridController == null ||
                attacker.CurrentCell == null)
            {
                return targets;
            }

            var enemyUnits = gridController.UnitManager
                .GetEnemyUnits(attacker.PlayerNumber);

            foreach (var enemy in enemyUnits)
            {
                if (enemy == null || enemy.CurrentCell == null)
                {
                    continue;
                }

                if (IsTargetInRange(
                    attacker.CurrentCell,
                    enemy.CurrentCell,
                    skill))
                {
                    targets.Add(enemy);
                }
            }

            return targets;
        }

        public static List<IUnit> GetAffectedTargets(
            IUnit attacker,
            IUnit selectedTarget,
            SkillData skill,
            IGridController gridController)
        {
            var targets = new List<IUnit>();

            if (attacker == null ||
                selectedTarget == null ||
                skill == null ||
                gridController == null ||
                attacker.CurrentCell == null ||
                selectedTarget.CurrentCell == null)
            {
                return targets;
            }

            var enemyUnits = gridController.UnitManager
                .GetEnemyUnits(attacker.PlayerNumber);

            switch (skill.attackShape)
            {
                case SkillAttackShape.Single:
                    targets.Add(selectedTarget);
                    break;

                case SkillAttackShape.Line:
                    AddLineTargets(
                        targets,
                        enemyUnits,
                        attacker.CurrentCell,
                        selectedTarget.CurrentCell,
                        skill);
                    break;

                case SkillAttackShape.Cross:
                    AddCrossTargets(
                        targets,
                        enemyUnits,
                        attacker.CurrentCell,
                        skill);
                    break;
            }

            return targets;
        }

        private static void AddLineTargets(
            List<IUnit> targets,
            IEnumerable<IUnit> enemyUnits,
            ICell attackerCell,
            ICell selectedTargetCell,
            SkillData skill)
        {
            int dx =
                selectedTargetCell.GridCoordinates.x -
                attackerCell.GridCoordinates.x;

            int dy =
                selectedTargetCell.GridCoordinates.y -
                attackerCell.GridCoordinates.y;

            // 選択した敵への方向を決定
            bool horizontal = Math.Abs(dx) >= Math.Abs(dy);

            int directionX = 0;
            int directionY = 0;

            if (horizontal)
            {
                directionX = dx > 0 ? 1 : -1;
            }
            else
            {
                directionY = dy > 0 ? 1 : -1;
            }

            int halfWidth =
                Math.Max(0, skill.attackWidth - 1) / 2;

            foreach (var enemy in enemyUnits)
            {
                if (enemy == null || enemy.CurrentCell == null)
                {
                    continue;
                }

                int targetDx =
                    enemy.CurrentCell.GridCoordinates.x -
                    attackerCell.GridCoordinates.x;

                int targetDy =
                    enemy.CurrentCell.GridCoordinates.y -
                    attackerCell.GridCoordinates.y;

                int forwardDistance;

                if (horizontal)
                {
                    forwardDistance = targetDx * directionX;

                    if (forwardDistance < 1 ||
                        forwardDistance > skill.range)
                    {
                        continue;
                    }

                    if (Math.Abs(targetDy) > halfWidth)
                    {
                        continue;
                    }
                }
                else
                {
                    forwardDistance = targetDy * directionY;

                    if (forwardDistance < 1 ||
                        forwardDistance > skill.range)
                    {
                        continue;
                    }

                    if (Math.Abs(targetDx) > halfWidth)
                    {
                        continue;
                    }
                }

                targets.Add(enemy);
            }
        }

        private static void AddCrossTargets(
            List<IUnit> targets,
            IEnumerable<IUnit> enemyUnits,
            ICell attackerCell,
            SkillData skill)
        {
            foreach (var enemy in enemyUnits)
            {
                if (enemy == null || enemy.CurrentCell == null)
                {
                    continue;
                }

                int dx =
                    enemy.CurrentCell.GridCoordinates.x -
                    attackerCell.GridCoordinates.x;

                int dy =
                    enemy.CurrentCell.GridCoordinates.y -
                    attackerCell.GridCoordinates.y;

                // 縦方向
                if (dx == 0 &&
                    Math.Abs(dy) >= 1 &&
                    Math.Abs(dy) <= skill.range)
                {
                    targets.Add(enemy);
                    continue;
                }

                // 横方向
                if (dy == 0 &&
                    Math.Abs(dx) >= 1 &&
                    Math.Abs(dx) <= skill.range)
                {
                    targets.Add(enemy);
                }
            }
        }

        /// <summary>
        /// Skillの射程内に対象が存在するか判定する。
        /// </summary>
        private static bool IsTargetInRange(
            ICell attackerCell,
            ICell targetCell,
            SkillData skill)
        {
            int dx =
                targetCell.GridCoordinates.x -
                attackerCell.GridCoordinates.x;

            int dy =
                targetCell.GridCoordinates.y -
                attackerCell.GridCoordinates.y;

            switch (skill.attackShape)
            {
                case SkillAttackShape.Single:
                    // 単体攻撃は通常の射程判定
                    return attackerCell.GetDistance(targetCell) <= skill.range;

                case SkillAttackShape.Line:
                    // 直線攻撃
                    return IsInLine(
                        dx,
                        dy,
                        skill.range,
                        skill.attackWidth);

                case SkillAttackShape.Cross:
                    // 十字攻撃
                    return IsInCross(
                        dx,
                        dy,
                        skill.range);

                default:
                    return false;
            }
        }

        private static bool IsInLine(
            int dx,
            int dy,
            int range,
            int width)
        {
            width = Math.Max(1, Math.Min(width, 10));

            int halfWidth = width / 2;

            // 横方向の直線
            if (Math.Abs(dx) >= 1 &&
                Math.Abs(dx) <= range &&
                Math.Abs(dy) <= halfWidth)
            {
                return true;
            }

            // 縦方向の直線
            if (Math.Abs(dy) >= 1 &&
                Math.Abs(dy) <= range &&
                Math.Abs(dx) <= halfWidth)
            {
                return true;
            }

            return false;
        }

        private static bool IsInCross(
            int dx,
            int dy,
            int range)
        {
            // 同じ縦列
            if (dx == 0 &&
                Math.Abs(dy) >= 1 &&
                Math.Abs(dy) <= range)
            {
                return true;
            }

            // 同じ横列
            if (dy == 0 &&
                Math.Abs(dx) >= 1 &&
                Math.Abs(dx) <= range)
            {
                return true;
            }

            return false;
        }
    }
}