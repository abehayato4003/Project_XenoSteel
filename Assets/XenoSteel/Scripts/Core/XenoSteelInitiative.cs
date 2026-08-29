using UnityEngine;

namespace XenoSteel.Core
{
    /// <summary>
    /// Sprint 1で使用する仮の機動力。
    /// Sprint 2以降、機体データの機動力に置き換える。
    /// </summary>
    public class XenoSteelInitiative : MonoBehaviour
    {
        [SerializeField]
        private int _mobility = 50;

        public int Mobility => _mobility;
    }
}