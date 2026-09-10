using UnityEngine;
using UnityEngine.UI;
using TMPro;

using TurnBasedStrategyFramework.Unity.Units;
using TurnBasedStrategyFramework.Common.Units;

using XenoSteel.Core;

namespace XenoSteel.Units
{
    public class XenoUnitStatusUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;

        [Header("表示")]
        [SerializeField] private TMP_Text _unitNameText;
        [SerializeField] private TMP_Text _pilotNameText;
        [SerializeField] private TMP_Text _hpText;
        [SerializeField] private TMP_Text _enText;

        [SerializeField] private Slider _hpBar;
        [SerializeField] private Slider _enBar;
        [SerializeField] private TMP_Text _attackText;
        [SerializeField] private TMP_Text _defenseText;

        [Header("アイコン")]
        [SerializeField] private Image _unitIcon;

        private Unit _currentUnit;


        private void Update()
        {
            if (_currentUnit != null)
            {
                ShowUnit(_currentUnit);
            }
        }

        public void SetCurrentUnit(IUnit unit)
        {
            if (unit is Unit unityUnit)
            {
                ShowUnit(unityUnit);
            }
        }

        public void ShowUnit(Unit unit)
        {
            if (unit == null)
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

            _currentUnit = unit;

            XenoUnitData data = initiative.UnitData;
            XenoSteelUnitStats stats = initiative.Stats;

            _panel.SetActive(true);

            _unitNameText.text = data.unitName;

            _pilotNameText.text =
                data.pilot != null
                    ? data.pilot.pilotName
                    : "なし";

            _hpText.text =
                $"HP {unit.Health} / {stats.HP}";

            _enText.text =
                $"EN {stats.EN} / {stats.MaxEN}";

            _hpBar.maxValue = stats.HP;
            _hpBar.value = unit.Health;

            _enBar.maxValue = stats.MaxEN;
            _enBar.value = stats.EN;

            _attackText.text =
                $"ATK {stats.Attack}";

            _defenseText.text =
                $"DEF {stats.Armor}";
        }

        public void Hide()
        {
            _currentUnit = null;

            if (_panel != null)
            {
                _panel.SetActive(false);
            }
        }

        public void SetUnitIcon(Sprite icon)
        {
            if (_unitIcon == null)
                return;

            _unitIcon.sprite = icon;
            _unitIcon.enabled = icon != null;
        }
    }
}