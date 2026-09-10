using System.Collections.Generic;
using UnityEngine;
using TMPro;

using TurnBasedStrategyFramework.Common.Units;
using XenoSteel.Core;

using TurnBasedStrategyFramework.Unity.Units;

namespace XenoSteel.Combat.UI
{
    public class XenoSteelTurnOrderUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Transform _slotParent;
        [SerializeField] private GameObject _slotPrefab;

        [SerializeField] private int _displayCount = 4;



        public void SetTurnOrder(
            IReadOnlyList<IUnit> units,
            int currentIndex)
        {
            Debug.Log(
                $"SetTurnOrder called: units={units?.Count}, index={currentIndex}"
            );

            Clear();

            if (units == null || units.Count == 0)
            {
                Debug.Log("TurnOrderUI: units が空です");
                return;
            }

            int displayed = 0;

            for (int i = currentIndex + 1;
                i < units.Count && displayed < _displayCount;
                i++)
            {
                IUnit unit = units[i];

                if (unit == null)
                {
                    Debug.Log("TurnOrderUI: unit が null です");
                    continue;
                }

                Unit unityUnit = unit as Unit;

                if (unityUnit == null)
                {
                    Debug.Log("TurnOrderUI: Unitへの変換に失敗: " + unit);
                    continue;
                }

                GameObject slot =
                    Instantiate(_slotPrefab, _slotParent);

                Debug.Log(
                    "TurnOrderUI: Slot生成 / " +
                    unityUnit.name
                );

                TMP_Text text =
                    slot.GetComponentInChildren<TMP_Text>();

                if (text != null)
                {
                    XenoSteelInitiative initiative =
                        unityUnit.GetComponent<XenoSteelInitiative>();

                    if (initiative != null &&
                        initiative.UnitData != null)
                    {
                        text.text =
                            initiative.UnitData.unitName;
                    }
                    else
                    {
                        text.text = unityUnit.name;
                    }

                    Debug.Log(
                        "TurnOrderUI: 名前設定 = " +
                        text.text
                    );
                }
                else
                {
                    Debug.Log("TurnOrderUI: TMP_Text が見つかりません");
                }

                displayed++;
            }

            Debug.Log(
                "TurnOrderUI: 表示数 = " +
                displayed
            );

            if (displayed > 0)
            {
                _panel.SetActive(true);
            }
        }

        public void Clear()
        {
            if (_slotParent != null)
            {
                for (int i = _slotParent.childCount - 1;
                    i >= 0;
                    i--)
                {
                    Destroy(
                        _slotParent.GetChild(i).gameObject);
                }
            }
        }
    }
}