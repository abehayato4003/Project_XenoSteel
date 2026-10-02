using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using XenoSteel.Core;
using XenoSteel.Information;

namespace XenoSteel.Combat
{
    public class XenoSteelEffectContext
    {
        public IUnit User { get; }
        public IUnit Target { get; }
        public ICell TargetCell { get; }
        public IGridController GridController { get; }
        public XenoSteelTurnResolver TurnResolver { get; }
        public XenoSteelInformationManager InformationManager { get; }
        public int CurrentRound { get; }

        public XenoSteelEffectContext(
            IUnit user,
            IUnit target,
            ICell targetCell,
            IGridController gridController,
            XenoSteelTurnResolver turnResolver,
            XenoSteelInformationManager informationManager,
            int currentRound)
        {
            User = user;
            Target = target;
            TargetCell = targetCell;
            GridController = gridController;
            TurnResolver = turnResolver;
            InformationManager = informationManager;
            CurrentRound = currentRound;
        }
    }
}