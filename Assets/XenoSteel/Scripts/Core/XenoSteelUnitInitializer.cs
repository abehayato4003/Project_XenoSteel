using UnityEngine;
using TurnBasedStrategyFramework.Unity.Units;

namespace XenoSteel.Core
{
    public class XenoSteelUnitInitializer : MonoBehaviour
    {
        [SerializeField]
        private XenoUnitData _unitData;

        private XenoSteelUnitStats _stats;

        private void Awake()
        {
            if (_unitData == null)
            {
                Debug.LogWarning("XenoSteelUnitInitializer: UnitDataが設定されていません。");
                return;
            }

            var unit = GetComponent<Unit>();

            if (unit == null)
            {
                Debug.LogWarning("XenoSteelUnitInitializer: Unitコンポーネントが見つかりません。");
                return;
            }

            _stats = new XenoSteelUnitStats(_unitData);

            unit.Health = _stats.HP;
            unit.MaxHealth = _stats.HP;

            Debug.Log($"XenoSteel HP initialized: {_stats.HP}");
        }
    }
}