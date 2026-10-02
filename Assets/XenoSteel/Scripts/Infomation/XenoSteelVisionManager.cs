using System.Collections.Generic;

using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

using XenoSteel.Core;

using UnityEngine;

namespace XenoSteel.Information
{
    public class XenoSteelVisionManager : MonoBehaviour
    {
        private XenoSteelVisionSystem _visionSystem;

        private List<ICell> _visibleCells = new List<ICell>();

        public IReadOnlyList<ICell> VisibleCells => _visibleCells;

        private GridController _gridController;
        private XenoSteelTurnResolver _turnResolver;
        private IUnit _subscribedUnit;

        private readonly HashSet<IUnit> _subscribedUnits =
            new HashSet<IUnit>();

        [SerializeField]
        private XenoSteelVisionFog visionFog;

        [SerializeField]
        private XenoSteelTacticalMap tacticalMap;

        private bool _tacticalMapBuilt;

        private void Awake()
        {
            Debug.Log("XenoSteelVisionManager Awake");
            _visionSystem = new XenoSteelVisionSystem();
            
        }

        public void UpdateVision(
            GridController gridController,
            XenoSteelTurnResolver turnResolver)
        {
            if (gridController == null || turnResolver == null)
                return;

            _gridController = gridController;
            _turnResolver = turnResolver;

            if (turnResolver.TurnOrder == null ||
                turnResolver.TurnOrder.Count == 0)
            {
                _visibleCells.Clear();
                return;
            }

            if (turnResolver.CurrentIndex < 0 ||
                turnResolver.CurrentIndex >= turnResolver.TurnOrder.Count)
            {
                _visibleCells.Clear();
                return;
            }

            IUnit currentUnit =
                turnResolver.TurnOrder[turnResolver.CurrentIndex];

            SubscribeToUnitMoved(currentUnit);

            if (currentUnit == null || currentUnit.PlayerNumber != 0)
                return;

            if (currentUnit == null || currentUnit.CurrentCell == null)
            {
                _visibleCells.Clear();
                return;
            }

            var unityUnit =
                currentUnit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unityUnit == null)
            {
                _visibleCells.Clear();
                return;
            }

            var initiative =
                unityUnit.GetComponent<XenoSteelInitiative>();

            if (initiative == null || initiative.UnitData == null)
            {
                _visibleCells.Clear();
                return;
            }

            int visionRange = initiative.UnitData.visionRange;

            var visionLight =
                unityUnit.GetComponentInChildren<XenoSteelVisionLight>();

            if (visionLight != null)
            {
                visionLight.SetVisionRange(visionRange);
            }

            _visibleCells = _visionSystem.GetVisibleCells(
                currentUnit,
                gridController,
                visionRange
            );

            UpdateLastKnownInformations(
                gridController,
                turnResolver
            );

            if (tacticalMap != null)
            {
                tacticalMap.UpdateEnemyMarkers();
            }

            UpdateUnitVisibility(gridController, currentUnit);

            UpdateVisionOverlays(gridController);

            UpdateEnemyRecognition(
                currentUnit,
                _visibleCells,
                turnResolver
            );

            Debug.Log(
                $"Vision Updated: " +
                $"Unit={currentUnit.UnitID}, " +
                $"Range={visionRange}, " +
                $"Visible Cells={_visibleCells.Count}"
            );

            if (visionFog != null)
            {
                visionFog.UpdateFog(
                    currentUnit.CurrentCell,
                    visionRange,
                    gridController.CellManager.GetCells());
            }

            if (tacticalMap != null)
            {
                tacticalMap.CenterOnUnit(currentUnit);
            }

            if (tacticalMap != null)
            {
                tacticalMap.UpdateAllyUnitMarkers(
                    gridController,
                    currentUnit
                );
            }

            if (tacticalMap != null)
            {
                tacticalMap.UpdateEnemyMarkers();
            }
        }

        public void UpdateAllUnitsVision(
            GridController gridController,
            XenoSteelTurnResolver turnResolver)
        {
            Debug.Log("UpdateAllUnitsVision Called");

            if (gridController == null || turnResolver == null)
                return;

            if (!_tacticalMapBuilt && tacticalMap != null)
            {
                tacticalMap.BuildMap(gridController);
                _tacticalMapBuilt = true;
            }

            _gridController = gridController;
            _turnResolver = turnResolver;

            if (turnResolver.TurnOrder == null ||
                turnResolver.TurnOrder.Count == 0)
                return;

            if (turnResolver.CurrentIndex >= 0 &&
                turnResolver.CurrentIndex < turnResolver.TurnOrder.Count)
            {
                IUnit currentUnit =
                    turnResolver.TurnOrder[turnResolver.CurrentIndex];

                SubscribeToUnitMoved(currentUnit);
            }

            HashSet<Vector2Int> playerVisibleCells =
                new HashSet<Vector2Int>();

            foreach (IUnit unit in turnResolver.TurnOrder)
            {
                if (unit == null || unit.CurrentCell == null)
                    continue;

                var unityUnit =
                    unit as TurnBasedStrategyFramework.Unity.Units.Unit;

                if (unityUnit == null)
                    continue;

                var initiative =
                    unityUnit.GetComponent<XenoSteelInitiative>();

                if (initiative == null ||
                    initiative.UnitData == null)
                    continue;

                int visionRange =
                    initiative.UnitData.visionRange;

                List<ICell> visibleCells =
                    _visionSystem.GetVisibleCells(
                        unit,
                        gridController,
                        visionRange
                    );

                if (unit.PlayerNumber == 0)
                {
                    foreach (ICell visibleCell in visibleCells)
                    {
                        if (visibleCell == null)
                            continue;

                        playerVisibleCells.Add(
                            new Vector2Int(
                                visibleCell.GridCoordinates.x,
                                visibleCell.GridCoordinates.y
                            )
                        );
                    }

                    UpdateEnemyRecognition(
                        unit,
                        visibleCells,
                        turnResolver
                    );
                }
            }

            if (turnResolver.CurrentIndex >= 0 &&
                turnResolver.CurrentIndex < turnResolver.TurnOrder.Count)
            {
                IUnit currentUnit =
                    turnResolver.TurnOrder[turnResolver.CurrentIndex];

                if (currentUnit != null &&
                    currentUnit.CurrentCell != null)
                {
                    var unityUnit =
                        currentUnit as TurnBasedStrategyFramework.Unity.Units.Unit;

                    if (unityUnit != null)
                    {
                        var initiative =
                            unityUnit.GetComponent<XenoSteelInitiative>();

                        if (initiative != null &&
                            initiative.UnitData != null)
                        {
                            if (currentUnit.PlayerNumber == 0)
                            {
                            int visionRange =
                                initiative.UnitData.visionRange;

                            var visionLight =
                                unityUnit.GetComponentInChildren<XenoSteelVisionLight>();

                            if (visionLight != null)
                            {
                                visionLight.SetVisionRange(visionRange);
                            }

                            _visibleCells =
                                _visionSystem.GetVisibleCells(
                                    currentUnit,
                                    gridController,
                                    visionRange
                                );

                            UpdateUnitVisibility(gridController, currentUnit);

                            UpdateVisionOverlays(gridController);

                            if (visionFog != null)
                            {
                                visionFog.UpdateFog(
                                    currentUnit.CurrentCell,
                                    visionRange,
                                    gridController.CellManager.GetCells());
                            }

                            if (tacticalMap != null)
                            {
                                tacticalMap.CenterOnUnit(currentUnit);
                            }

                            if (tacticalMap != null)
                            {
                                tacticalMap.UpdateAllyUnitMarkers(
                                    gridController,
                                    currentUnit
                                );
                            }

                            if (tacticalMap != null)
                            {
                                tacticalMap.UpdateEnemyMarkers();
                            }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateEnemyRecognition(
            IUnit currentUnit,
            List<ICell> visibleCells,
            XenoSteelTurnResolver turnResolver)
        {
            Debug.Log(
                $"Vision Recognition Start: " +
                $"Unit={currentUnit.UnitID}, " +
                $"Player={currentUnit.PlayerNumber}, " +
                $"VisibleCells={visibleCells.Count}"
            );

            if (currentUnit == null)
                return;

            if (currentUnit.PlayerNumber != 0)
                return;

            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager == null)
                return;

            foreach (ICell cell in visibleCells)
            {
                if (cell.CurrentUnits == null)
                    continue;

                foreach (IUnit targetUnit in cell.CurrentUnits)
                {
                    if (targetUnit == null)
                        continue;
                        
                    Debug.Log(
                        $"Vision Cell: " +
                        $"Unit={currentUnit.UnitID}, " +
                        $"Cell=({cell.GridCoordinates.x},{cell.GridCoordinates.y}), " +
                        $"Units={cell.CurrentUnits.Count}"
                    );

                    if (targetUnit.PlayerNumber == currentUnit.PlayerNumber)
                        continue;

                    Vector2Int cellPosition = new Vector2Int(
                        cell.GridCoordinates.x,
                        cell.GridCoordinates.y
                    );

                    string informationState = "Unknown";

                    if (informationManager.TryGetInformation(
                            targetUnit,
                            out var information))
                    {
                        informationState = information.State.ToString();
                    }

                    Debug.Log(
                        $"Vision Recognition: " +
                        $"CurrentUnit={currentUnit.UnitID}, " +
                        $"CurrentPlayer={currentUnit.PlayerNumber}, " +
                        $"TargetUnit={targetUnit.UnitID}, " +
                        $"TargetPlayer={targetUnit.PlayerNumber}, " +
                        $"Cell=({cellPosition.x},{cellPosition.y})"
                    );

                    informationManager.ConfirmEnemy(
                        targetUnit,
                        XenoSteelInformationSource.Visual,
                        cellPosition,
                        turnResolver.CurrentRound
                    );
                }
            }
        }

        private void SubscribeToUnitMoved(IUnit unit)
        {
            if (unit == null)
                return;

            if (_subscribedUnits.Contains(unit))
                return;

            _subscribedUnits.Add(unit);
            unit.UnitMoved += OnUnitMoved;
        }

        private void UnsubscribeFromUnitMoved()
        {
            if (_subscribedUnit == null)
                return;

            _subscribedUnit.UnitMoved -= OnUnitMoved;
            _subscribedUnit = null;
        }

        private void OnUnitMoved(UnitMovedEventArgs eventArgs)
        {
            Debug.Log("XenoSteelVisionManager: OnUnitMoved");

            if (_gridController == null || _turnResolver == null)
                return;

            if (_turnResolver.TurnOrder == null)
                return;

            if (_turnResolver.CurrentIndex < 0 ||
                _turnResolver.CurrentIndex >= _turnResolver.TurnOrder.Count)
                return;

            IUnit currentUnit =
                _turnResolver.TurnOrder[_turnResolver.CurrentIndex];

            if (currentUnit == null)
                return;

            // 味方ターン
            if (currentUnit.PlayerNumber == 0)
            {
                UpdateVision(
                    _gridController,
                    _turnResolver
                );

                return;
            }

            // 敵ターン
            UpdateEnemyTurnVisualVisibility(
                _gridController,
                _turnResolver
            );
        }

        private void UpdateVisionOverlays(
            GridController gridController)
        {
            if (gridController == null)
                return;

            foreach (ICell cell in gridController.CellManager.GetCells())
            {
                if (cell == null)
                    continue;

                var unityCell =
                    cell as TurnBasedStrategyFramework.Unity.Cells.Cell;

                if (unityCell == null)
                    continue;

                var overlay =
                    unityCell.GetComponent<XenoSteelVisionOverlay>();

                if (overlay == null)
                    continue;

                bool isVisible = false;

                foreach (ICell visibleCell in _visibleCells)
                {
                    if (visibleCell == null)
                        continue;

                    if (visibleCell.GridCoordinates == cell.GridCoordinates)
                    {
                        isVisible = true;
                        break;
                    }
                }

                overlay.SetVisible(isVisible);
            }
        }

        private void UpdateUnitVisibility(
            GridController gridController,
            IUnit currentUnit)
        {
            if (gridController == null)
                return;

            foreach (ICell cell in gridController.CellManager.GetCells())
            {
                if (cell == null)
                    continue;

                if (cell.CurrentUnits == null)
                    continue;

                foreach (IUnit unit in cell.CurrentUnits)
                {
                    if (unit == null)
                        continue;

                    var unityUnit =
                        unit as TurnBasedStrategyFramework.Unity.Units.Unit;

                    if (unityUnit == null)
                        continue;

                    var visibility =
                        unityUnit.GetComponent<XenoSteelUnitVisibility>();

                    if (visibility == null)
                        continue;

                    // 現在行動中のUnitは常に表示
                    if (unit == currentUnit)
                    {
                        visibility.SetVisible(true);
                        visibility.SetVisionLightVisible(true);
                        continue;
                    }

                    bool isVisible = false;

                    foreach (ICell visibleCell in _visibleCells)
                    {
                        if (visibleCell == null)
                            continue;

                        if (visibleCell.GridCoordinates == cell.GridCoordinates)
                        {
                            isVisible = true;
                            break;
                        }
                    }

                    visibility.SetVisible(isVisible);
                    visibility.SetVisionLightVisible(false);
                }
            }
        }

        private void UpdateLastKnownInformations(
            GridController gridController,
            XenoSteelTurnResolver turnResolver)
        {
            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager == null)
                return;

            HashSet<Vector2Int> playerVisibleCells =
                new HashSet<Vector2Int>();

            foreach (IUnit unit in turnResolver.TurnOrder)
            {
                if (unit == null)
                    continue;

                if (unit.PlayerNumber != 0)
                    continue;

                if (unit.CurrentCell == null)
                    continue;

                var unityUnit =
                    unit as TurnBasedStrategyFramework.Unity.Units.Unit;

                if (unityUnit == null)
                    continue;

                var initiative =
                    unityUnit.GetComponent<XenoSteelInitiative>();

                if (initiative == null ||
                    initiative.UnitData == null)
                    continue;

                int visionRange =
                    initiative.UnitData.visionRange;

                List<ICell> visibleCells =
                    _visionSystem.GetVisibleCells(
                        unit,
                        gridController,
                        visionRange
                    );

                foreach (ICell cell in visibleCells)
                {
                    if (cell == null)
                        continue;

                    playerVisibleCells.Add(
                        new Vector2Int(
                            cell.GridCoordinates.x,
                            cell.GridCoordinates.y
                        )
                    );
                }
            }

            foreach (
                KeyValuePair<object, XenoSteelEnemyInformation> pair
                in informationManager.GetAllInformations())
            {
                IUnit enemyUnit = pair.Key as IUnit;

                if (enemyUnit == null)
                    continue;

                XenoSteelEnemyInformation information =
                    pair.Value;

                if (information == null)
                    continue;

                if (information.State !=
                    XenoSteelEnemyInformationState.Confirmed)
                    continue;

                if (information.Source !=
                    XenoSteelInformationSource.Visual)
                    continue;

                if (enemyUnit.CurrentCell == null)
                    continue;

                Vector2Int enemyCell =
                    new Vector2Int(
                        enemyUnit.CurrentCell.GridCoordinates.x,
                        enemyUnit.CurrentCell.GridCoordinates.y
                    );

                if (playerVisibleCells.Contains(enemyCell))
                    continue;

                information.SetLastKnown();

                Debug.Log(
                    $"Last Known: " +
                    $"Enemy={enemyUnit.UnitID}, " +
                    $"Cell={information.LastKnownCell}"
                );
            }
        }

        public void UpdateTacticalMapEnemyMarkers()
        {
            if (tacticalMap == null)
                return;

            tacticalMap.UpdateEnemyMarkers();
        }

        private void UpdateEnemyTurnVisualVisibility(
            GridController gridController,
            XenoSteelTurnResolver turnResolver)
        {
            if (gridController == null || turnResolver == null)
                return;

            HashSet<Vector2Int> playerVisibleCells =
                new HashSet<Vector2Int>();

            // 味方全員の視界を取得
            foreach (IUnit unit in turnResolver.TurnOrder)
            {
                if (unit == null)
                    continue;

                if (unit.PlayerNumber != 0)
                    continue;

                if (unit.CurrentCell == null)
                    continue;

                var unityUnit =
                    unit as TurnBasedStrategyFramework.Unity.Units.Unit;

                if (unityUnit == null)
                    continue;

                var initiative =
                    unityUnit.GetComponent<XenoSteelInitiative>();

                if (initiative == null ||
                    initiative.UnitData == null)
                    continue;

                int visionRange =
                    initiative.UnitData.visionRange;

                List<ICell> visibleCells =
                    _visionSystem.GetVisibleCells(
                        unit,
                        gridController,
                        visionRange
                    );

                foreach (ICell cell in visibleCells)
                {
                    if (cell == null)
                        continue;

                    playerVisibleCells.Add(
                        new Vector2Int(
                            cell.GridCoordinates.x,
                            cell.GridCoordinates.y
                        )
                    );
                }
            }

            // 全ユニットの表示状態を更新
            foreach (ICell cell in gridController.CellManager.GetCells())
            {
                if (cell == null)
                    continue;

                if (cell.CurrentUnits == null)
                    continue;

                foreach (IUnit unit in cell.CurrentUnits)
                {
                    if (unit == null)
                        continue;

                    // 敵ユニットだけを対象にする
                    if (unit.PlayerNumber == 0)
                        continue;

                    var unityUnit =
                        unit as TurnBasedStrategyFramework.Unity.Units.Unit;

                    if (unityUnit == null)
                        continue;

                    var visibility =
                        unityUnit.GetComponent<XenoSteelUnitVisibility>();

                    if (visibility == null)
                        continue;

                    Vector2Int unitCell =
                        new Vector2Int(
                            cell.GridCoordinates.x,
                            cell.GridCoordinates.y
                        );

                    bool isVisible =
                        playerVisibleCells.Contains(unitCell);

                    visibility.SetVisualOnlyVisible(isVisible);
                }
            }
        }
    }
}