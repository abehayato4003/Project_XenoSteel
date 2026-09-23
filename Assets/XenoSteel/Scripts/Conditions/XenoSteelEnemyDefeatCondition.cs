using System.Linq;
using TurnBasedStrategyFramework.Common.Controllers.GameResolvers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Controllers;
using TurnBasedStrategyFramework.Unity.Players;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;

namespace XenoSteel.Conditions
{
    public class XenoSteelEnemyDefeatCondition : MonoBehaviour
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
            var enemyUnits = _unitManager
                .GetUnits()
                .Where(u => u.PlayerNumber == 1);

            //Debug.Log($"EnemyDefeatCondition: 敵残存数 = {enemyUnits.Count()}");

            if (enemyUnits.Any())
            {
                return;
            }

            //Debug.Log("EnemyDefeatCondition: 敵全滅を確認。GameEndedを呼び出します。");

            var winner = _playerManager
                .GetPlayers()
                .First(p => p.PlayerNumber == 0);

            var losers = _playerManager
                .GetPlayers()
                .Where(p => p != winner);

            _gridController.InvokeGameEnded(
                new GameResult(winner, losers)
            );
        }
    }
}