using System.Collections.Generic;
using System.Linq;

using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Controllers.TurnResolvers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Controllers;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

using XenoSteel.Units;
using XenoSteel.Combat.UI;
using XenoSteel.Information;


namespace XenoSteel.Core
{
    /// <summary>
    /// XenoSteel用のターンリゾルバー。
    /// 全ユニットを機動力順に並べ、1ユニットずつ行動させる。
    /// </summary>
    public class XenoSteelTurnResolver : UnityTurnResolver
    {
        private List<IUnit> _turnOrder = new List<IUnit>();
        private int _currentIndex = 0;

        public IReadOnlyList<IUnit> TurnOrder => _turnOrder;
        public int CurrentIndex => _currentIndex;

        private int _currentRound = 1;
        public int CurrentRound => _currentRound;

        public override TurnContext ResolveStart(GridController gridController)
        {
            Debug.Log("XenoSteelTurnResolver.ResolveStart");

            _currentRound = 1;

            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager != null)
            {
                informationManager.RemoveExpiredInformations(
                    _currentRound
                );
            }

            CreateTurnOrder(gridController);

            _currentIndex = 0;

            UpdateTurnOrderUI();

            XenoSteelVisionManager visionManager =
                Object.FindFirstObjectByType<XenoSteelVisionManager>();

            if (visionManager != null)
            {
                visionManager.UpdateAllUnitsVision(
                    gridController,
                    this
                );
            }

            RunRadarScan(gridController);

            if (visionManager != null)
            {
                visionManager.UpdateTacticalMapEnemyMarkers();
            }

            return CreateTurnContext(gridController);
        }

        private void RunRadarScan(GridController gridController)
        {
            if (_turnOrder == null ||
                _turnOrder.Count == 0)
            {
                return;
            }

            XenoSteelRadarSystem radarSystem =
                new XenoSteelRadarSystem();


            foreach (IUnit unit in _turnOrder)
            {
                if (unit == null)
                    continue;

                Debug.Log(
                    $"Radar Call Start: Unit={unit.UnitID}"
                );

                if (unit.PlayerNumber == 0)
                {
                    XenoSteelInformationManager informationManager =
                        Object.FindFirstObjectByType<XenoSteelInformationManager>();

                    if (informationManager == null)
                        continue;

                    radarSystem.UpdateRadarInformation(
                        unit,
                        gridController,
                        informationManager,
                        _currentRound
                    );
                }
                else
                {
                    XenoSteelEnemyInformationManager informationManager =
                        Object.FindFirstObjectByType<XenoSteelEnemyInformationManager>();

                    if (informationManager == null)
                        continue;

                    radarSystem.UpdateRadarInformation(
                        unit,
                        gridController,
                        informationManager,
                        _currentRound
                    );
                }
            }
        }

        public override TurnContext ResolveTurn(
            GridController gridController)
        {
            // 現在のTurnOrderから死亡・破壊済みUnitを除外
            _turnOrder = _turnOrder
                .Where(unit =>
                    unit != null &&
                    unit.Health > 0)
                .ToList();

            // 現在Indexが範囲外になった場合
            if (_currentIndex >= _turnOrder.Count)
            {
                _currentRound++;

                // ラウンド開始時に最新のUnitからTurnOrderを再構築
                CreateTurnOrder(gridController);

                _currentIndex = 0;
            }
            else
            {
                // 次のUnitへ進む
                _currentIndex++;

                // 次のUnitが存在しない場合は新ラウンド
                if (_currentIndex >= _turnOrder.Count)
                {
                    _currentRound++;

                    CreateTurnOrder(gridController);

                    _currentIndex = 0;
                }
            }

            // 念のため、Indexが有効なUnitを指すまで進める
            while (
                _currentIndex < _turnOrder.Count &&
                (
                    _turnOrder[_currentIndex] == null ||
                    _turnOrder[_currentIndex].Health <= 0
                ))
            {
                _currentIndex++;
            }

            // 全Unitがいなくなった場合
            if (_turnOrder.Count == 0)
            {
                return CreateTurnContext(gridController);
            }

            // Indexが末尾まで進んだ場合は新ラウンド
            if (_currentIndex >= _turnOrder.Count)
            {
                _currentRound++;

                CreateTurnOrder(gridController);

                _currentIndex = 0;
            }

            XenoSteelVisionManager visionManager =
                Object.FindFirstObjectByType<XenoSteelVisionManager>();

            if (visionManager != null)
            {
                visionManager.UpdateAllUnitsVision(
                    gridController,
                    this
                );
            }

            RunRadarScan(gridController);

            if (visionManager != null)
            {
                visionManager.UpdateTacticalMapEnemyMarkers();
            }

            UpdateTurnOrderUI();

            return CreateTurnContext(gridController);
        }

        private void CreateTurnOrder(GridController gridController)
        {
            _turnOrder = gridController.UnitManager
                .GetUnits()
                .Where(unit => unit != null)
                .Where(unit => unit.Health > 0)
                .OrderByDescending(GetMobility)
                .ThenBy(unit => unit.UnitID)
                .ToList();
                
            //UpdateTurnOrderUI();
                
        }

        private void UpdateTurnOrderUI()
        {
            XenoSteelTurnOrderUI ui =
                Object.FindFirstObjectByType<XenoSteelTurnOrderUI>();


            if (ui != null)
            {
                ui.SetTurnOrder(
                    _turnOrder,
                    _currentIndex);
            }
        }


        private TurnContext CreateTurnContext(GridController gridController)
        {
            IUnit unit = _turnOrder[_currentIndex];

            XenoUnitStatusUI statusUI =
                Object.FindFirstObjectByType<XenoUnitStatusUI>();

            if (statusUI != null)
            {
                statusUI.SetCurrentUnit(unit);
            }

            var player = gridController.PlayerManager
                .GetPlayers()
                .FirstOrDefault(p => p.PlayerNumber == unit.PlayerNumber);

            XenoSteelSkillSelectionUI skillSelectionUI =
                Object.FindFirstObjectByType<XenoSteelSkillSelectionUI>(
                    FindObjectsInactive.Include
                );

            if (skillSelectionUI != null)
            {
                skillSelectionUI.SetCurrentUnit(
                    unit,
                    player != null && player.PlayerNumber == 0
                );
            }

            XenoSteelRadarUI radarUI =
                Object.FindFirstObjectByType<XenoSteelRadarUI>();

            if (radarUI != null)
            {
                radarUI.SetGridController(gridController);

                radarUI.SetCurrentUnit(
                    unit,
                    player != null && player.PlayerNumber == 0
                );
            }

            XenoSteelEndTurnUI endTurnUI =
                Object.FindFirstObjectByType<XenoSteelEndTurnUI>();

            if (endTurnUI != null)
            {
                endTurnUI.SetCurrentUnit(
                    unit as TurnBasedStrategyFramework.Unity.Units.Unit
                );
            }

            XenoSteelUnitInfoUI infoUI =
                Object.FindFirstObjectByType<XenoSteelUnitInfoUI>();

            if (infoUI != null)
            {
                infoUI.SetCurrentTurnUnit(unit);
            }

            

            Debug.Log(
                "Current Unit: " + unit.UnitID +
                " / Player Number: " + unit.PlayerNumber +
                " / Current Player: " + 
                (player != null ? player.PlayerNumber.ToString() : "NULL")
            );

            return new TurnContext(
                player,
                new IUnit[] { unit }
            );
        }

        private int GetMobility(IUnit unit)
        {
            var unityUnit = unit as Unit;

            if (unityUnit == null)
            {
                return 0;
            }

            var initiative = unityUnit.GetComponent<XenoSteelInitiative>();

            if (initiative == null)
            {
                return 0;
            }

            return initiative.Mobility;
        }
    }
}