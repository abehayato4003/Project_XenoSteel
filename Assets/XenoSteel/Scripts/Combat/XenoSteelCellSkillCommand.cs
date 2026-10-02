using System.Threading.Tasks;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

namespace XenoSteel.Combat
{
    public readonly struct XenoSteelCellSkillCommand : ICommand
    {
        private readonly int _actionCost;

        public XenoSteelCellSkillCommand(int actionCost = 1)
        {
            _actionCost = actionCost;
        }

        public Task Execute(
            IUnit unit,
            IGridController controller)
        {
            unit.ActionPoints -= _actionCost;
            return Task.CompletedTask;
        }

        public Task Undo(
            IUnit unit,
            IGridController controller)
        {
            unit.ActionPoints += _actionCost;
            return Task.CompletedTask;
        }

        public System.Collections.Generic.Dictionary<string, object> Serialize()
        {
            return new System.Collections.Generic.Dictionary<string, object>
            {
                { "action_cost", _actionCost }
            };
        }

        public ICommand Deserialize(
            System.Collections.Generic.Dictionary<string, object> actionParams,
            IGridController gridController)
        {
            int actionCost =
                System.Convert.ToInt32(actionParams["action_cost"]);

            return new XenoSteelCellSkillCommand(actionCost);
        }
    }
}