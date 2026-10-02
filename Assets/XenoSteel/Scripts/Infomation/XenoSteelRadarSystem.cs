using System.Collections.Generic;

using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

using UnityEngine;

using XenoSteel.Information;

namespace XenoSteel.Information
{
    public class XenoSteelRadarSystem
    {
        public int CalculateDetectionDifference(
            IUnit observer,
            IUnit target)
        {
            if (observer == null || target == null)
                return int.MinValue;

            var observerUnit =
                observer as TurnBasedStrategyFramework.Unity.Units.Unit;

            var targetUnit =
                target as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (observerUnit == null || targetUnit == null)
                return int.MinValue;

            var observerInitiative =
                observerUnit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            var targetInitiative =
                targetUnit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            if (observerInitiative == null ||
                targetInitiative == null ||
                observerInitiative.UnitData == null ||
                targetInitiative.UnitData == null)
            {
                return int.MinValue;
            }

            int radarAccuracy =
                observerInitiative.UnitData.radarAccuracy;

            int stealth =
                targetInitiative.UnitData.stealth;

            int difference = radarAccuracy - stealth;

            if (difference <= 0)
                return 0;

            return difference;
        }

        public List<XenoSteelRadarDetectionResult> GetDetectedEnemies(
            IUnit observer,
            IGridController gridController)
        {
            Debug.Log(
                $"Radar Scan: Observer={observer.UnitID}"
            );

            var detectedEnemies =
                new List<XenoSteelRadarDetectionResult>();

            if (observer == null ||
                gridController == null ||
                observer.CurrentCell == null)
            {
                return detectedEnemies;
            }

            var observerUnit =
                observer as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (observerUnit == null)
                return detectedEnemies;

            var observerInitiative =
                observerUnit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            if (observerInitiative == null ||
                observerInitiative.UnitData == null)
            {
                return detectedEnemies;
            }

            int radarRange =
                observerInitiative.UnitData.radarRange;

            if (radarRange <= 0)
                return detectedEnemies;

            foreach (IUnit target in gridController.UnitManager.GetUnits())
            {
                if (target == null)
                    continue;

                if (target.PlayerNumber == observer.PlayerNumber)
                    continue;

                if (target.Health <= 0)
                    continue;

                if (target.CurrentCell == null)
                    continue;

                int distance =
                    observer.CurrentCell.GetDistance(target.CurrentCell);

                if (distance > radarRange)
                    continue;

                int detectionDifference =
                    CalculateDetectionDifference(
                        observer,
                        target
                    );

                if (detectionDifference <= 0)
                    continue;

                int informationPrecision =
                    System.Math.Min(detectionDifference, 5);

                detectedEnemies.Add(
                    new XenoSteelRadarDetectionResult(
                        target,
                        System.Math.Min(detectionDifference, 5)
                    )
                );
            }

            return detectedEnemies;
        }

        public void UpdateRadarInformation(
            

            IUnit observer,
            IGridController gridController,
            XenoSteelInformationManager informationManager,
            int round)
        {
            Debug.Log(
                $"Radar Update: Observer={observer.UnitID}, Round={round}"
            );
            if (observer == null ||
                gridController == null ||
                informationManager == null)
            {
                return;
            }

            List<XenoSteelRadarDetectionResult> detectedEnemies =
                GetDetectedEnemies(
                    observer,
                    gridController
                );

            foreach (XenoSteelRadarDetectionResult result in detectedEnemies)
            {
                if (result.Target == null)
                    continue;

                if (result.Target.CurrentCell == null)
                    continue;

                var cell = result.Target.CurrentCell;

                Vector2Int cellPosition = new Vector2Int(
                    cell.GridCoordinates.x,
                    cell.GridCoordinates.y
                );

                informationManager.ConfirmRadarEnemy(
                    result.Target,
                    cellPosition,
                    round,
                    result.InformationPrecision
                );
            }
        }

        public void UpdateRadarInformation(
            IUnit observer,
            IGridController gridController,
            XenoSteelInformationManager informationManager,
            int round,
            int radarRange,
            int radarAccuracy)
        {
            Debug.Log(
                $"Radar Skill Scan: " +
                $"Observer={observer?.UnitID}, " +
                $"Range={radarRange}, " +
                $"Accuracy={radarAccuracy}"
            );

            if (observer == null ||
                gridController == null ||
                informationManager == null)
            {
                return;
            }

            if (radarRange <= 0 ||
                radarAccuracy <= 0)
            {
                return;
            }

            if (observer.CurrentCell == null)
            {
                return;
            }

            foreach (IUnit target in gridController.UnitManager.GetUnits())
            {
                if (target == null)
                    continue;

                if (target.PlayerNumber == observer.PlayerNumber)
                    continue;

                if (target.Health <= 0)
                    continue;

                if (target.CurrentCell == null)
                    continue;

                int distance =
                    observer.CurrentCell.GetDistance(
                        target.CurrentCell
                    );

                if (distance > radarRange)
                    continue;

                var targetUnit =
                    target as TurnBasedStrategyFramework.Unity.Units.Unit;

                if (targetUnit == null)
                    continue;

                var targetInitiative =
                    targetUnit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

                if (targetInitiative == null ||
                    targetInitiative.UnitData == null)
                {
                    continue;
                }

                int stealth =
                    targetInitiative.UnitData.stealth;

                int detectionDifference =
                    radarAccuracy - stealth;

                if (detectionDifference <= 0)
                    continue;

                int informationPrecision =
                    System.Math.Min(
                        detectionDifference,
                        5
                    );

                Vector2Int cellPosition =
                    new Vector2Int(
                        target.CurrentCell.GridCoordinates.x,
                        target.CurrentCell.GridCoordinates.y
                    );

                informationManager.ConfirmRadarEnemy(
                    target,
                    cellPosition,
                    round,
                    informationPrecision
                );

                Debug.Log(
                    $"Radar Skill Detection: " +
                    $"Target={target.UnitID}, " +
                    $"Precision={informationPrecision}"
                );
            }
        }
    }
}