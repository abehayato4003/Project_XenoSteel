using UnityEngine;

namespace XenoSteel.Information
{
    public class XenoSteelVisionLight : MonoBehaviour
    {
        [SerializeField]
        private Light visionLight;

        [SerializeField]
        private float rangeMultiplier = 1.0f;

        [SerializeField]
        private float intensity = 2.0f;

        private void Awake()
        {
            if (visionLight == null)
                visionLight = GetComponent<Light>();

            if (visionLight == null)
                return;

            visionLight.type = LightType.Point;
            visionLight.intensity = intensity;
        }

        public void SetVisionRange(int visionRange)
        {
            if (visionLight == null)
                return;

            visionLight.range =
                visionRange * rangeMultiplier;
        }
    }
}