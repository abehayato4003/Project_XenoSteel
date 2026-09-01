using System.Linq;
using System.Threading.Tasks;

using TurnBasedStrategyFramework.Common.AI.BehaviourTrees;
using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

using XenoSteel.Core;

namespace XenoSteel.AI
{
    /// <summary>
    /// XenoSteel用のAI移動処理。
    /// 攻撃可能な射程に入るまで敵へ接近し、
    /// 射程内に入った場合はそれ以上移動しない。
    /// </summary>
    public class XenoSteelAIMoveActionNode : ITreeNode
    {
        private readonly IUnit _unit;
        private readonly IGridController _gridController;

        public XenoSteelAIMoveActionNode(
            IUnit unit,
            IGridController gridController)
        {
            _unit = unit;
            _gridController = gridController;
        }

        public Task<bool> Execute(bool debugMode)
        {
            if (_unit.ActionPoints <= 0)
            {
                return Task.FromResult(false);
            }

            if (_unit.CurrentCell == null)
            {
                return Task.FromResult(false);
            }

            var unit =
                _unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unit == null)
            {
                return Task.FromResult(false);
            }

            var initiative =
                unit.GetComponent<XenoSteelInitiative>();

            if (initiative == null ||
                initiative.UnitData == null ||
                initiative.UnitData.skills == null ||
                initiative.UnitData.skills.Length == 0)
            {
                return Task.FromResult(false);
            }

            var enemyUnits = _gridController.UnitManager
                .GetEnemyUnits(_unit.PlayerNumber)
                .Where(enemy => enemy.CurrentCell != null)
                .ToList();

            if (!enemyUnits.Any())
            {
                return Task.FromResult(false);
            }

            // 使用可能なスキルの中で最も長い射程を取得
            int maxRange = initiative.UnitData.skills
                .Where(skill => skill != null)
                .Select(skill => skill.range)
                .DefaultIfEmpty(1)
                .Max();

            // すでに攻撃可能なら移動しない
            bool alreadyInRange = enemyUnits.Any(enemy =>
                _unit.CurrentCell.GetDistance(enemy.CurrentCell) <= maxRange
            );

            if (alreadyInRange)
            {
                return Task.FromResult(false);
            }

            // 経路情報をキャッシュ
            _unit.CachePaths(_gridController.CellManager);

            var cells = _gridController.CellManager.GetCells();

            // 移動可能なセルを取得し、
            // 「敵との距離が最も短くなるセル」を探す
            var candidates = cells
                .Where(cell =>
                    cell != null &&
                    !cell.Equals(_unit.CurrentCell) &&
                    _unit.IsCellMovableTo(cell)
                )
                .Select(cell =>
                {
                    var path = _unit
                        .FindPath(cell, _gridController.CellManager)
                        .ToList();

                    return new
                    {
                        Cell = cell,
                        Path = path
                    };
                })
                .Where(candidate => candidate.Path.Any())
                .ToList();

            if (!candidates.Any())
            {
                return Task.FromResult(false);
            }

            // 今回の移動力で到達可能な範囲に限定
            var reachableCandidates = candidates
                .Select(candidate =>
                {
                    var availableDestinations =
                        _unit.GetAvailableDestinations(candidate.Path);

                    if (availableDestinations == null ||
                        !availableDestinations.Any())
                    {
                        return null;
                    }

                    var reachableCell = candidate.Path
                        .Where(cell => availableDestinations.Contains(cell))
                        .LastOrDefault();

                    if (reachableCell == null)
                    {
                        return null;
                    }

                    var reachablePath = candidate.Path
                        .TakeWhile(cell => !cell.Equals(reachableCell))
                        .Append(reachableCell)
                        .ToList();

                    return new
                    {
                        Cell = reachableCell,
                        Path = reachablePath,
                        Distance = enemyUnits.Min(enemy =>
                            reachableCell.GetDistance(enemy.CurrentCell))
                    };
                })
                .Where(candidate => candidate != null)
                .OrderBy(candidate => candidate.Distance)
                .ToList();

            if (!reachableCandidates.Any())
            {
                return Task.FromResult(false);
            }

            var selected = reachableCandidates.First();

            // 射程内に入った場合は、その位置で止まる
            // 射程外なら可能な限り敵へ近づく
            var tcs = new TaskCompletionSource<bool>();

            _unit.AIExecuteAbility(
                new MoveCommand(
                    _unit.CurrentCell,
                    selected.Cell,
                    selected.Path
                ),
                _gridController,
                tcs
            );

            return tcs.Task;
        }
    }
}