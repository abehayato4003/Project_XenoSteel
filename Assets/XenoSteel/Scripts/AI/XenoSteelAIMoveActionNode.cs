using UnityEngine;
using System.Collections.Generic;

using System.Linq;

using System.Threading.Tasks;

using TurnBasedStrategyFramework.Common.AI.BehaviourTrees;

using TurnBasedStrategyFramework.Common.Cells;

using TurnBasedStrategyFramework.Common.Controllers;

using TurnBasedStrategyFramework.Common.Units;

using TurnBasedStrategyFramework.Common.Units.Abilities;

using XenoSteel.Core;

using XenoSteel.Units;

using XenoSteel.Information;

namespace XenoSteel.AI
{
    /// <summary>
    /// XenoSteel用のAI移動処理。
    /// 敵軍情報をもとに、AI Personalityの設定から
    /// 移動先を評価して決定する。
    /// </summary>
    public class XenoSteelAIMoveActionNode : ITreeNode
    {
        private readonly IUnit _unit;
        private readonly IGridController _gridController;

        public XenoSteelAIDecision LastDecision { get; private set; }

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

            var pilot = initiative.UnitData.pilot;

            if (pilot == null || pilot.aiPersonality == null)
            {
                return Task.FromResult(false);
            }

            var personality = pilot.aiPersonality;

            var informationManager =
                UnityEngine.Object.FindFirstObjectByType<
                    XenoSteelEnemyInformationManager>();

            if (informationManager == null)
            {
                return Task.FromResult(false);
            }

            var knownPlayers = informationManager
                .GetAllInformations()
                .Where(pair => pair.Value != null)
                .ToList();

            if (!knownPlayers.Any())
            {
                _unit.CachePaths(_gridController.CellManager);

                var searchCells =
                    _gridController.CellManager.GetCells();

                var searchCandidates = searchCells
                    .Where(cell =>
                        cell != null &&
                        !cell.Equals(_unit.CurrentCell) &&
                        _unit.IsCellMovableTo(cell))
                    .Select(cell =>
                    {
                        var path =
                            _unit.FindPath(
                                cell,
                                _gridController.CellManager)
                            .ToList();

                        return new
                        {
                            Cell = cell,
                            Path = path
                        };
                    })
                    .Where(candidate => candidate.Path.Any())
                    .Select(candidate =>
                    {
                        var availableDestinations =
                            _unit.GetAvailableDestinations(
                                candidate.Path);

                        if (availableDestinations == null ||
                            !availableDestinations.Any())
                        {
                            return null;
                        }

                        var reachableCell = candidate.Path
                            .Where(cell =>
                                availableDestinations.Contains(cell))
                            .LastOrDefault();

                        if (reachableCell == null)
                        {
                            return null;
                        }

                        var reachablePath = candidate.Path
                            .TakeWhile(cell =>
                                !cell.Equals(reachableCell))
                            .Append(reachableCell)
                            .ToList();

                        return new
                        {
                            Cell = reachableCell,
                            Path = reachablePath
                        };
                    })
                    .Where(candidate => candidate != null)
                    .ToList();

                if (!searchCandidates.Any())
                {
                    return Task.FromResult(false);
                }

                var selectedSearch = searchCandidates
                    .Select(candidate =>
                    {
                        Vector2Int position = new Vector2Int(
                            candidate.Cell.GridCoordinates.x,
                            candidate.Cell.GridCoordinates.y
                        );

                        float score = 0f;

                        // ------------------------------------------
                        // reconImportance
                        //
                        // 敵軍がまだ観測していないセルを優先する。
                        // ------------------------------------------

                        bool isUnobserved =
                            !informationManager.IsObserved(position);

                        if (isUnobserved)
                        {
                            score +=
                                100f *
                                personality.reconImportance;
                        }

                        // ------------------------------------------
                        // aggression
                        //
                        // より遠くまで移動することを好む。
                        // ------------------------------------------

                        score +=
                            candidate.Path.Count *
                            personality.aggression;

                        // ------------------------------------------
                        // coverAttachment
                        //
                        // 敵が分からない場合でも、
                        // 障害物の近くにいることを好む。
                        // ------------------------------------------

                        int coverDistance =
                            GetNearestObstacleDistance(
                                candidate.Cell,
                                _gridController.CellManager.GetCells());

                        if (coverDistance >= 0)
                        {
                            score +=
                                (1f / (coverDistance + 1f)) *
                                30f *
                                personality.coverAttachment;
                        }

                        return new
                        {
                            Candidate = candidate,
                            Score = score
                        };
                    })
                    .OrderByDescending(result => result.Score)
                    .First()
                    .Candidate;

                LastDecision = new XenoSteelAIDecision(
                    "Move",
                    "talkSearchGather",
                    new Vector2Int(
                        selectedSearch.Cell.GridCoordinates.x,
                        selectedSearch.Cell.GridCoordinates.y
                    ),
                    null,
                    XenoSteelInformationSource.None
                );

                var searchTcs =
                    new TaskCompletionSource<bool>();

                _unit.AIExecuteAbility(
                    new MoveCommand(
                        _unit.CurrentCell,
                        selectedSearch.Cell,
                        selectedSearch.Path
                    ),
                    _gridController,
                    searchTcs
                );

                return searchTcs.Task;
            }

            // 敵軍情報のLastKnownCell(Vector2Int)を
            // 実際のマップ上のICellへ変換する。
            var knownPlayerCells = knownPlayers
                .Select(pair => new
                {
                    Information = pair.Value,

                    Cell = _gridController.CellManager
                        .GetCells()
                        .FirstOrDefault(cell =>
                            cell != null &&
                            cell.GridCoordinates.x ==
                                pair.Value.LastKnownCell.x &&
                            cell.GridCoordinates.y ==
                                pair.Value.LastKnownCell.y)
                })
                .Where(x => x.Cell != null)
                .ToList();

            if (!knownPlayerCells.Any())
            {
                return Task.FromResult(false);
            }

            // --------------------------------------------------
            // 1. AIが認識する対象を決定
            // --------------------------------------------------
            //
            // Visual:
            //   常に100%の情報として扱う。
            //
            // Radar:
            //   obedienceによって重みを変更する。
            //
            // obedience = 0
            //   Radar情報を無視する。
            //
            var targetCandidates = knownPlayerCells
                .Where(info =>
                    info.Information.Source ==
                    XenoSteelInformationSource.Visual ||
                    personality.obedience > 0f)
                .Select(info =>
                {
                    float informationWeight =
                        info.Information.Source ==
                        XenoSteelInformationSource.Visual
                            ? 1f
                            : personality.obedience;

                    float distance =
                        _unit.CurrentCell.GetDistance(info.Cell);

                    float targetScore =
                        (1f / (distance + 1f)) *
                        informationWeight;

                    return new
                    {
                        Information = info.Information,
                        Cell = info.Cell,
                        Score = targetScore
                    };
                })
                .Where(target => target.Score > 0f)
                .OrderByDescending(target => target.Score)
                .ToList();

            // obedience = 0 でRadarしかない場合、
            // AIはPlayerを認識していないものとして扱う。
            if (!targetCandidates.Any())
            {
                return Task.FromResult(false);
            }

            var selectedTarget = targetCandidates.First();

            // --------------------------------------------------
            // 2. 最大攻撃射程
            // --------------------------------------------------

            int maxRange = initiative.UnitData.skills
                .Where(skill => skill != null)
                .Select(skill => skill.range)
                .DefaultIfEmpty(1)
                .Max();

            // --------------------------------------------------
            // 3. 移動可能セルを取得
            // --------------------------------------------------

            _unit.CachePaths(_gridController.CellManager);

            var cells =
                _gridController.CellManager.GetCells();

            var candidates = cells
                .Where(cell =>
                    cell != null &&
                    !cell.Equals(_unit.CurrentCell) &&
                    _unit.IsCellMovableTo(cell)
                )
                .Select(cell =>
                {
                    var path = _unit
                        .FindPath(
                            cell,
                            _gridController.CellManager)
                        .ToList();

                    return new
                    {
                        Cell = cell,
                        Path = path
                    };
                })
                .Where(candidate => candidate.Path.Any())
                .ToList();

            // --------------------------------------------------
            // 4. 今回の移動力で到達可能なセルへ変換
            // --------------------------------------------------

            var reachableCandidates = candidates
                .Select(candidate =>
                {
                    var availableDestinations =
                        _unit.GetAvailableDestinations(
                            candidate.Path);

                    if (availableDestinations == null ||
                        !availableDestinations.Any())
                    {
                        return null;
                    }

                    var reachableCell = candidate.Path
                        .Where(cell =>
                            availableDestinations.Contains(cell))
                        .LastOrDefault();

                    if (reachableCell == null)
                    {
                        return null;
                    }

                    var reachablePath = candidate.Path
                        .TakeWhile(cell =>
                            !cell.Equals(reachableCell))
                        .Append(reachableCell)
                        .ToList();

                    return new
                    {
                        Cell = reachableCell,
                        Path = reachablePath
                    };
                })
                .Where(candidate => candidate != null)
                .ToList();

            // --------------------------------------------------
            // 5. 「その場に留まる」も候補にする
            // --------------------------------------------------
            //
            // reconImportanceが高いAIは、
            // Radar情報しかない場合に移動せず警戒する
            // 判断を後のSprintで可能にする。
            //
            // 現段階では警戒Effect自体はまだ実装しない。
            //

            var allCandidates =
                reachableCandidates
                    .Select(candidate => new
                    {
                        candidate.Cell,
                        candidate.Path
                    })
                    .ToList();

            allCandidates.Add(new
            {
                Cell = _unit.CurrentCell,
                Path = new System.Collections.Generic.List<ICell>()
            });

            // --------------------------------------------------
            // 6. Personalityによる移動先評価
            // --------------------------------------------------

            var scoredCandidates = allCandidates
                .Select(candidate =>
                {
                    float score = 0f;

                    int distance =
                        candidate.Cell.GetDistance(
                            selectedTarget.Cell);

                    bool inAttackRange =
                        distance <= maxRange;

                    bool isStaying =
                        candidate.Cell.Equals(
                            _unit.CurrentCell);

                    // ------------------------------------------
                    // aggression
                    //
                    // ターゲットへ近づくほど高評価。
                    // 距離0を100点として、1マス遠ざかるごとに10点減少。
                    // ------------------------------------------

                    float baseApproachScore =
                        (100f - (distance * 10f)) *
                        personality.aggression;

                    score += baseApproachScore;

                    // ------------------------------------------
                    // cowardice
                    // ------------------------------------------

                    if (inAttackRange &&
                        !isStaying)
                    {
                        score -=
                            50f *
                            personality.cowardice;
                    }

                    // ------------------------------------------
                    // reconImportance
                    //
                    // Radar情報しかない場合、
                    // 「その場に留まる」ことを評価する。
                    // ------------------------------------------

                    bool informationIsUncertain =
                        selectedTarget.Information.Source ==
                            XenoSteelInformationSource.Radar ||
                        selectedTarget.Information.State ==
                            XenoSteelEnemyInformationState.LastKnown;

                    if (informationIsUncertain &&
                        isStaying)
                    {
                        score +=
                            30f *
                            personality.reconImportance;
                    }

                    // ------------------------------------------
                    // Visual
                    // ------------------------------------------

                    if (selectedTarget.Information.Source ==
                        XenoSteelInformationSource.Visual)
                    {
                        score += 1f;
                    }

                    // ------------------------------------------
                    // coverAttachment
                    // ------------------------------------------

                    bool hasCover =
                        HasCoverBetween(
                            candidate.Cell,
                            selectedTarget.Cell);

                    if (hasCover)
                    {
                        score +=
                            30f *
                            personality.coverAttachment;
                    }

                    return new
                    {
                        Candidate = candidate,
                        Score = score
                    };
                })
                .OrderByDescending(result => result.Score)
                .ToList();

            if (!scoredCandidates.Any())
            {
                return Task.FromResult(false);
            }

            var selected =
                scoredCandidates.First().Candidate;

            // --------------------------------------------------
            // 7. その場に留まる判断
            // --------------------------------------------------

            if (selected.Cell.Equals(_unit.CurrentCell))
            {
                LastDecision = new XenoSteelAIDecision(
                    "Stay",
                    "talkSearchGather",
                    new Vector2Int(
                        selected.Cell.GridCoordinates.x,
                        selected.Cell.GridCoordinates.y
                    ),
                    null,
                    selectedTarget.Information.Source
                );

                return Task.FromResult(true);
            }

            // --------------------------------------------------
            // 8. AIが決定した内容を保存
            // --------------------------------------------------

            LastDecision = new XenoSteelAIDecision(
                "Move",
                "talkSearchGather",
                new Vector2Int(
                    selected.Cell.GridCoordinates.x,
                    selected.Cell.GridCoordinates.y
                ),
                null,
                selectedTarget.Information.Source
            );

            // --------------------------------------------------
            // 9. 実際の移動
            // --------------------------------------------------

            var tcs =
                new TaskCompletionSource<bool>();

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

        private bool HasCoverBetween(
            ICell candidateCell,
            ICell targetCell)
        {
            if (candidateCell == null ||
                targetCell == null)
            {
                return false;
            }

            int x1 = candidateCell.GridCoordinates.x;
            int y1 = candidateCell.GridCoordinates.y;

            int x2 = targetCell.GridCoordinates.x;
            int y2 = targetCell.GridCoordinates.y;

            int dx = x2 - x1;
            int dy = y2 - y1;

            // 水平・垂直方向のみ判定
            if (dx != 0 && dy != 0)
            {
                return false;
            }

            int stepX = dx == 0 ? 0 : dx > 0 ? 1 : -1;
            int stepY = dy == 0 ? 0 : dy > 0 ? 1 : -1;

            int x = x1 + stepX;
            int y = y1 + stepY;

            while (x != x2 || y != y2)
            {
                ICell cell =
                    _gridController.CellManager
                        .GetCells()
                        .FirstOrDefault(c =>
                            c != null &&
                            c.GridCoordinates.x == x &&
                            c.GridCoordinates.y == y);

                if (cell != null && cell.IsTaken)
                {
                    return true;
                }

                x += stepX;
                y += stepY;
            }

            return false;
        }

        private int GetNearestObstacleDistance(
            ICell candidateCell,
            IEnumerable<ICell> cells)
        {
            if (candidateCell == null ||
                cells == null)
            {
                return -1;
            }

            int minimumDistance = int.MaxValue;

            foreach (ICell cell in cells)
            {
                if (cell == null ||
                    !cell.IsTaken)
                {
                    continue;
                }

                int distance =
                    candidateCell.GetDistance(cell);

                if (distance < minimumDistance)
                {
                    minimumDistance = distance;
                }
            }

            return minimumDistance == int.MaxValue
                ? -1
                : minimumDistance;
        }
    }
}
