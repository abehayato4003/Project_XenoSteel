using System.Collections.Generic;
using System.Linq;

using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Controllers.TurnResolvers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Controllers;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

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

        private int _currentRound = 1;
        public int CurrentRound => _currentRound;

        public override TurnContext ResolveStart(GridController gridController)
        {
            _currentRound = 1;

            CreateTurnOrder(gridController);

            _currentIndex = 0;

            return CreateTurnContext(gridController);
        }

        public override TurnContext ResolveTurn(GridController gridController)
        {
            _currentIndex++;
            
            if (_currentIndex >= _turnOrder.Count)
            {
                _currentRound++;

                CreateTurnOrder(gridController);
                _currentIndex = 0;
            }

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
                
                
        }


        private TurnContext CreateTurnContext(GridController gridController)
        {
            IUnit unit = _turnOrder[_currentIndex];

            var player = gridController.PlayerManager
                .GetPlayers()
                .FirstOrDefault(p => p.PlayerNumber == unit.PlayerNumber);

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