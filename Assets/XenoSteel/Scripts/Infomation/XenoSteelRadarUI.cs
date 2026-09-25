using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;
using TurnBasedStrategyFramework.Unity.Units;
using UnityEngine;
using UnityEngine.UI;

namespace XenoSteel.Information
{
    public class XenoSteelRadarUI : MonoBehaviour
    {
        [SerializeField] private GameObject radarPanel;
        [SerializeField] private Button radarButton;

        private IUnit _currentUnit;
        private IGridController _gridController;
        private XenoSteelRadarAbility _radarAbility;

        private void Awake()
        {
            radarButton.onClick.AddListener(OnRadarButtonClicked);
        }

        public void SetCurrentUnit(
            IUnit unit,
            bool isPlayerUnit)
        {
            _currentUnit = unit;
            _radarAbility = null;

            if (!isPlayerUnit)
            {
                UpdateButton();
                return;
            }

            var unityUnit =
                unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unityUnit == null)
            {
                UpdateButton();
                return;
            }

            _radarAbility =
                unityUnit.GetComponent<XenoSteelRadarAbility>();

            UpdateButton();
        }

        public void SetGridController(
            IGridController gridController)
        {
            _gridController = gridController;
        }

        private void UpdateButton()
        {
            if (_radarAbility == null)
            {
                radarButton.interactable = false;
                return;
            }

            radarButton.interactable =
                _radarAbility.CanUseRadar();
        }

        private void OnRadarButtonClicked()
        {
            if (_radarAbility == null)
                return;

            if (!_radarAbility.CanUseRadar())
                return;

            if (_gridController == null)
            {
                Debug.LogError(
                    "RadarUI: GridController not found"
                );
                return;
            }

            _radarAbility.ExecuteRadar(_gridController);

            UpdateButton();
        }
    }
}