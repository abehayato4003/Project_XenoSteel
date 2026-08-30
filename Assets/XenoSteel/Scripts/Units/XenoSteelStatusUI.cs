using UnityEngine;
using TMPro;
using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Units
{
    public class XenoUnitStatusUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _statusText;

        private Unit[] _units;

        private Unit _selectedUnit;

        private void Start()
        {
            _panel.SetActive(false);

            _units = FindObjectsByType<Unit>(FindObjectsSortMode.None);

            foreach (Unit unit in _units)
            {
                unit.UnitSelected += OnUnitSelected;
            }
        }

        public void SelectUnit(Unit unit)
        {
            if (_selectedUnit != null)
            {
                _selectedUnit.UnitSelected -= OnUnitSelected;
            }

            _selectedUnit = unit;

            if (_selectedUnit != null)
            {
                _selectedUnit.UnitSelected += OnUnitSelected;
                ShowUnitStatus(_selectedUnit);
            }
        }

        private void OnUnitSelected(IUnit unit)
        {
            if (unit is Unit selectedUnit)
            {
                ShowUnitStatus(selectedUnit);
            }
        }

        private void ShowUnitStatus(Unit unit)
        {
            XenoSteel.Core.XenoSteelInitiative initiative =
                unit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            if (initiative == null || initiative.UnitData == null)
            {
                Hide();
                return;
            }

            Show(initiative.UnitData, unit);
        }

        private void Show(XenoUnitData data, Unit unit){
            if (data == null)
            {
                Hide();
                return;
            }

            XenoSteelUnitStats stats = new XenoSteelUnitStats(data);

            _panel.SetActive(true);

            _statusText.text =
            $"<b>機体：{data.unitName}</b>\n" +
            $"パイロット：{(data.pilot != null ? data.pilot.pilotName : "なし")}\n" +
            $"HP: {unit.Health} / {stats.HP}\n" +
            $"EN: {stats.EN}\n" +
            $"Attack: {stats.Attack}\n" +
            $"Armor: {stats.Armor}\n" +
            $"Mobility: {stats.Mobility}\n" +
            $"Movement: {stats.Movement}\n" +
            $"Terrain: {GetTerrainAdaptationText(stats.TerrainAdaptation)}\n" +
            $"Size: {stats.Size}";
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private string GetTerrainAdaptationText(TerrainAdaptation adaptation)
        {
            switch (adaptation)
            {
                case TerrainAdaptation.Good:
                    return "○";

                case TerrainAdaptation.Normal:
                    return "△";

                case TerrainAdaptation.Bad:
                    return "×";

                default:
                    return "-";
            }
        }

        private void OnDestroy()
        {
            if (_units == null)
                return;

            foreach (Unit unit in _units)
            {
                if (unit != null)
                {
                    unit.UnitSelected -= OnUnitSelected;
                }
            }
        }
    }
}