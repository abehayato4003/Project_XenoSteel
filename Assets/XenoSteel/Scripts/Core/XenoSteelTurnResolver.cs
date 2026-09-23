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

            CreateTurnOrder(gridController);

            _currentIndex = 0;

            UpdateTurnOrderUI();

            return CreateTurnContext(gridController);
        }

        public override TurnContext ResolveTurn(GridController gridController)
        {
             Debug.Log($"ResolveTurn: Index={_currentIndex}, Count={_turnOrder.Count}");

            IUnit endingUnit = null;

            if (_currentIndex >= 0 &&
                _currentIndex < _turnOrder.Count)
            {
                endingUnit = _turnOrder[_currentIndex];
            }

            ConfirmPendingInformation(endingUnit);

            _currentIndex++;

            // 現ラウンドの全ユニットが行動終了
            if (_currentIndex >= _turnOrder.Count)
            {
                _currentRound++;

                // 新しいラウンドの行動順を作り直す
                CreateTurnOrder(gridController);

                // 新ラウンドの先頭ユニット
                _currentIndex = 0;

                Debug.Log(
                    $"New Round: {_currentRound}, " +
                    $"TurnOrder Count: {_turnOrder.Count}"
                );
            }

            Debug.Log(
                $"Update TurnOrder: Round={_currentRound}, " +
                $"Index={_currentIndex}, " +
                $"Count={_turnOrder.Count}"
            );

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
                Object.FindFirstObjectByType<XenoSteelSkillSelectionUI>();

            if (skillSelectionUI != null)
            {
                skillSelectionUI.SetCurrentUnit(
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

        private void ConfirmPendingInformation(IUnit endingUnit)
        {
            if (endingUnit == null)
                return;

            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager == null)
                return;

            if (!informationManager.TryGetInformation(
                    endingUnit,
                    out var information))
            {
                return;
            }

            if (information.State !=
                XenoSteelEnemyInformationState.RecognitionPending)
            {
                return;
            }

            if (endingUnit.CurrentCell == null)
                return;

            informationManager.ConfirmPendingEnemy(
                endingUnit,
                new Vector3(
                    endingUnit.CurrentCell.WorldPosition.x,
                    endingUnit.CurrentCell.WorldPosition.y,
                    endingUnit.CurrentCell.WorldPosition.z
                ),
                _currentRound
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