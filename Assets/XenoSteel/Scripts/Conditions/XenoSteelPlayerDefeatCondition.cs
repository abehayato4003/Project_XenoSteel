using System.Linq;
using TurnBasedStrategyFramework.Common.Controllers.GameResolvers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Controllers;
using TurnBasedStrategyFramework.Unity.Players;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace XenoSteel.Conditions
{
    public class XenoSteelPlayerDefeatCondition : MonoBehaviour
    {
        [SerializeField] private UnityUnitManager _unitManager;
        [SerializeField] private UnityPlayerManager _playerManager;
        [SerializeField] private UnityGridController _gridController;

        private void Awake()
        {
            if (_unitManager != null)
            {
                _unitManager.UnitRemoved += OnUnitRemoved;
            }
        }

        private void OnDestroy()
        {
            if (_unitManager != null)
            {
                _unitManager.UnitRemoved -= OnUnitRemoved;
            }
        }

        private void OnUnitRemoved(IUnit unit)
        {
            var playerUnits = _unitManager
                .GetUnits()
                .Where(u => u.PlayerNumber == 0);

            Debug.Log(
                $"PlayerDefeatCondition: 味方残存数 = {playerUnits.Count()}"
            );

            if (playerUnits.Any())
            {
                return;
            }

            Debug.Log(
                "PlayerDefeatCondition: 味方全滅を確認。GameEndedを呼び出します。"
            );

            var winner = _playerManager
                .GetPlayers()
                .First(p => p.PlayerNumber == 1);

            var losers = _playerManager
                .GetPlayers()
                .Where(p => p != winner);

            _gridController.InvokeGameEnded(
                new GameResult(winner, losers)
            );
        }
    }
}