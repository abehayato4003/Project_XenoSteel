using UnityEngine;
using TurnBasedStrategyFramework.Unity.Units;

namespace XenoSteel.Units
{
    public class XenoSteelMovement : MonoBehaviour
    {
        [SerializeField]
        private XenoUnitData _unitData;

        private XenoSteelUnitStats _stats;
        private Unit _unit;

        private void Awake()
        {
            _unit = GetComponent<Unit>();

            if (_unitData != null)
            {
                _stats = new XenoSteelUnitStats(_unitData);
            }

            if (_unit != null && _stats != null)
            {
                _unit.MovementPoints = _stats.Movement;
            }
        }
    }
}