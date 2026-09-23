using System.Collections.Generic;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    public class XenoSteelVisionSystem
    {
        public List<ICell> GetVisibleCells(
            IUnit unit,
            IGridController gridController,
            int visionRange)
        {
            var visibleCells = new List<ICell>();

            if (unit == null || unit.CurrentCell == null)
                return visibleCells;

            ICell originCell = unit.CurrentCell;

            foreach (var cell in gridController.CellManager.GetCells())
            {
                if (originCell.GetDistance(cell) > visionRange)
                    continue;

                if (HasLineOfSight(originCell, cell, gridController))
                {
                    visibleCells.Add(cell);
                }
            }

            return visibleCells;
        }

        private bool HasLineOfSight(
            ICell originCell,
            ICell targetCell,
            IGridController gridController)
        {
            if (originCell.Equals(targetCell))
                return true;

            int startX = originCell.GridCoordinates.x;
            int startY = originCell.GridCoordinates.y;

            int targetX = targetCell.GridCoordinates.x;
            int targetY = targetCell.GridCoordinates.y;

            int deltaX = targetX - startX;
            int deltaY = targetY - startY;

            int steps = System.Math.Max(
                System.Math.Abs(deltaX),
                System.Math.Abs(deltaY)
            );

            if (steps == 0)
                return true;

            var cells = gridController.CellManager.GetCells();

            for (int i = 1; i < steps; i++)
            {
                float t = (float)i / steps;

                int x = (int)System.Math.Round(
                    startX + deltaX * t
                );

                int y = (int)System.Math.Round(
                    startY + deltaY * t
                );

                ICell intermediateCell = null;

                foreach (var cell in cells)
                {
                    if (cell.GridCoordinates.x == x &&
                        cell.GridCoordinates.y == y)
                    {
                        intermediateCell = cell;
                        break;
                    }
                }

                if (intermediateCell == null)
                    continue;

                if (IsObstacleCell(intermediateCell))
                    return false;
            }

            return true;
        }

        private bool IsObstacleCell(ICell cell)
        {
            return cell.IsTaken &&
                   cell.CurrentUnits != null &&
                   cell.CurrentUnits.Count == 0;
        }
    }
}