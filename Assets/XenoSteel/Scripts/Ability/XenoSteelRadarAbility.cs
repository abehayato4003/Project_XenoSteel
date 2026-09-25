using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units.Abilities;
using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Unity.Units.Abilities;
using XenoSteel.Core;
using UnityEngine;

namespace XenoSteel.Information
{
    public class XenoSteelRadarAbility : Ability
    {
        private IGridController _gridController;

        private bool _hasUsedThisTurn;
        public bool CanUseRadar()
        {
            return !_hasUsedThisTurn;
        }

        public override void Initialize(IGridController gridController)
        {
            base.Initialize(gridController);

            _gridController = gridController;
            _hasUsedThisTurn = false;
        }

        public override void OnTurnStart(IGridController gridController)
        {
            _hasUsedThisTurn = false;
        }

        public override bool CanPerform(IGridController gridController)
        {
            return true;
        }

        public void ExecuteRadar(IGridController gridController)
        {
            var unit = UnitReference as Unit;

            if (unit == null)
                return;

            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager == null)
            {
                Debug.Log("Radar: InformationManager not found");
                return;
            }

            XenoSteelRadarSystem radarSystem =
                new XenoSteelRadarSystem();

            var turnResolver =
                gridController.TurnResolver
                    as XenoSteel.Core.XenoSteelTurnResolver;

            if (turnResolver == null)
            {
                Debug.Log("Radar: TurnResolver not found");
                return;
            }

            radarSystem.UpdateRadarInformation(
                unit,
                gridController,
                informationManager,
                turnResolver.CurrentRound
            );

            _hasUsedThisTurn = true;

            Debug.Log(
                $"Manual Radar: Unit={unit.UnitID}"
            );
        }

        public void ExecuteFromUI()
        {
            if (_gridController == null)
            {
                Debug.LogError("Radar: GridController not initialized.");
                return;
            }

            OnAbilitySelected(_gridController);
        }
    }
}