using System.Threading.Tasks;
using System.Linq;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Common.Units.Abilities;

namespace XenoSteel.Combat
{
    public readonly struct XenoSteelAttackCommand : ICommand
    {
        private readonly IUnit _target;
        private readonly int _actionCost;

        public XenoSteelAttackCommand(
            IUnit target,
            int actionCost = 1)
        {
            _target = target;
            _actionCost = actionCost;
        }

        public async Task Execute(
            IUnit unit,
            IGridController controller)
        {
            unit.ActionPoints -= _actionCost;

            await Task.WhenAll(
                controller.UnitManager.MarkAsAttacking(
                    unit,
                    _target
                ),
                controller.UnitManager.MarkAsDefending(
                    _target,
                    unit
                )
            );
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
                { "target_id", _target.UnitID },
                { "action_cost", _actionCost }
            };
        }

        public ICommand Deserialize(
            System.Collections.Generic.Dictionary<string, object> actionParams,
            IGridController gridController)
        {
            int targetId =
                System.Convert.ToInt32(actionParams["target_id"]);

            int actionCost =
                System.Convert.ToInt32(actionParams["action_cost"]);

            IUnit target =
                gridController.UnitManager
                    .GetUnits()
                    .First(u => u.UnitID == targetId);

            return new XenoSteelAttackCommand(
                target,
                actionCost
            );
        }
    }
}