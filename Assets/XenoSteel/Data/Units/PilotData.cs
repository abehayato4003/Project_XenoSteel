using UnityEngine;

namespace XenoSteel.Units
{
    /// <summary>
    /// パイロットのステータス倍率を保持するデータ。
    /// </summary>
    [CreateAssetMenu(fileName = "PilotData", menuName = "XenoSteel/Units/Pilot Data")]
    public class PilotData : ScriptableObject
    {

        [Header("基本情報")]
        public string pilotName;

        [Header("Pilot Status Multiplier")]
        [SerializeField] private float hpMultiplier = 1.0f;
        [SerializeField] private float enMultiplier = 1.0f;
        [SerializeField] private float armorMultiplier = 1.0f;
        [SerializeField] private float mobilityMultiplier = 1.0f;

        public float HPMultiplier => hpMultiplier;
        public float ENMultiplier => enMultiplier;
        public float ArmorMultiplier => armorMultiplier;
        public float MobilityMultiplier => mobilityMultiplier;
    }
}