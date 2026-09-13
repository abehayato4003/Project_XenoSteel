using UnityEngine;
using TMPro;
using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Common.Units;
using XenoSteel.Core;

namespace XenoSteel.Combat.UI
{
    public class XenoSteelUnitInfoUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        [Header("表示")]
        [SerializeField] private TMP_Text _unitNameText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _enText;
        [SerializeField] private TMP_Text _defenseText;

        [Header("位置")]
        [SerializeField] private Vector3 _screenOffset = new Vector3(100f, 50f, 0f);

        [SerializeField] private CanvasGroup _canvasGroup;

        private IUnit _currentTurnUnit;
        private Unit[] _registeredUnits;
        private IUnit _displayedUnit;


        private void Start()
        {
            Unit[] units =
                FindObjectsByType<Unit>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None);

            _registeredUnits = units;

            foreach (Unit unit in _registeredUnits)
            {
                unit.UnitHighlighted += OnUnitHighlighted;
                unit.UnitDehighlighted += OnUnitDehighlighted;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnUnitHighlighted(IUnit unit)
        {
            Unit unityUnit = unit as Unit;

            if (unityUnit == null)
                return;

            ShowUnit(unityUnit);
        }

        private void OnUnitDehighlighted(IUnit unit)
        {
            if (unit == _displayedUnit)
            {
                Hide();
            }
        }

        public void SetCurrentTurnUnit(IUnit unit)
        {
            _currentTurnUnit = unit;
            Hide();
        }

        public void ShowUnit(Unit unit)
        {
            if (unit == null ||
                unit == _currentTurnUnit)
            {
                Hide();
                return;
            }

            XenoSteelInitiative initiative =
                unit.GetComponent<XenoSteelInitiative>();

            if (initiative == null ||
                initiative.UnitData == null ||
                initiative.Stats == null)
            {
                Hide();
                return;
            }

            XenoUnitData data = initiative.UnitData;
            XenoSteelUnitStats stats = initiative.Stats;

            _displayedUnit = unit;
            
            _unitNameText.text = data.unitName;
            _hpText.text = $"HP {unit.Health} / {stats.HP}";
            _enText.text = $"EN {stats.EN} / {stats.MaxEN}";
            _defenseText.text = $"DEF {stats.Armor}";

            Camera camera = Camera.main;

            if (camera == null)
            {
                Hide();
                return;
            }

            Vector3 screenPosition =
                camera.WorldToScreenPoint(unit.transform.position);

            if (screenPosition.z < 0f)
            {
                Hide();
                return;
            }

            _panel.transform.position =
                screenPosition + _screenOffset;

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Hide()
        {
            _displayedUnit = null;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void OnDestroy()
        {
            if (_registeredUnits == null)
                return;

            foreach (Unit unit in _registeredUnits)
            {
                if (unit == null)
                    continue;

                unit.UnitHighlighted -= OnUnitHighlighted;
                unit.UnitDehighlighted -= OnUnitDehighlighted;
            }
        }

        
    }
}