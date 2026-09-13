using UnityEngine;
using UnityEngine.UI;
using TurnBasedStrategyFramework.Unity.Controllers;
using TurnBasedStrategyFramework.Unity.Units;

namespace XenoSteel.Combat.UI
{
    public class XenoSteelEndTurnUI : MonoBehaviour
    {
        [SerializeField] private Button _endTurnButton;
        [SerializeField] private UnityGridController _gridController;

        private Unit _currentUnit;

        public void SetCurrentUnit(Unit unit)
        {
            _currentUnit = unit;

            if (_endTurnButton == null)
                return;

            if (_currentUnit == null)
            {
                _endTurnButton.interactable = false;
                return;
            }

            _endTurnButton.interactable =
                _currentUnit.PlayerNumber == 0;
        }

        public void EndTurn()
        {
            if (_gridController == null)
                return;

            if (_currentUnit == null)
                return;

            if (_currentUnit.PlayerNumber != 0)
                return;

            _gridController.EndTurn();
        }
    }
}