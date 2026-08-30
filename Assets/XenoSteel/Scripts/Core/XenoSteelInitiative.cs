using UnityEngine;

namespace XenoSteel.Core
{
    public class XenoSteelInitiative : MonoBehaviour
    {
        [SerializeField]
        private XenoUnitData _unitData;

        private XenoSteelUnitStats _stats;

        public int Mobility => _stats != null ? _stats.Mobility : 0;

        public XenoUnitData UnitData => _unitData;

        private void Awake()
        {
            Debug.Log("XenoSteelInitiative.Awake");

            if (_unitData != null)
            {
                _stats = new XenoSteelUnitStats(_unitData);
            }
        }
    }
}