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

        private void Awake()
        {
            _visionSystem = new XenoSteelVisionSystem();
        }

        public void UpdateVision(
            GridController gridController,
            XenoSteelTurnResolver turnResolver)
        {
            if (gridController == null || turnResolver == null)
                return;

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

            _visibleCells = _visionSystem.GetVisibleCells(
                currentUnit,
                gridController,
                visionRange
            );

            UpdateEnemyRecognition(
                currentUnit,
                turnResolver
            );

            Debug.Log(
                $"Vision Updated: " +
                $"Unit={currentUnit.UnitID}, " +
                $"Range={visionRange}, " +
                $"Visible Cells={_visibleCells.Count}"
            );
        }

        private void UpdateEnemyRecognition(
            IUnit currentUnit,
            XenoSteelTurnResolver turnResolver)
        {
            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager == null)
                return;

            foreach (ICell cell in _visibleCells)
            {
                if (cell.CurrentUnits == null)
                    continue;

                foreach (IUnit targetUnit in cell.CurrentUnits)
                {
                    if (targetUnit == null)
                        continue;

                    if (targetUnit.PlayerNumber == currentUnit.PlayerNumber)
                        continue;

                    if (informationManager.TryGetInformation(
                            targetUnit,
                            out var information))
                    {
                        if (information.State !=
                            XenoSteelEnemyInformationState.Unknown)
                        {
                            continue;
                        }
                    }

                    Vector3 position = new Vector3(
                        cell.WorldPosition.x,
                        cell.WorldPosition.y,
                        cell.WorldPosition.z
                    );

                    informationManager.SetRecognitionPending(
                        targetUnit,
                        position,
                        turnResolver.CurrentRound,
                        turnResolver.CurrentIndex
                    );
                }
            }
        }
    }
}