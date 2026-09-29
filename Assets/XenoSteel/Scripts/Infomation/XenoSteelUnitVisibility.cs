using UnityEngine;

namespace XenoSteel.Information
{
    public class XenoSteelUnitVisibility : MonoBehaviour
    {
        [SerializeField]
        private GameObject visualRoot;

        [SerializeField]
        private GameObject highlightObject;

        [SerializeField]
        private GameObject visionLight;

        public void SetVisible(bool visible)
        {
            if (visualRoot != null)
                visualRoot.SetActive(visible);

            if (highlightObject != null)
                highlightObject.SetActive(visible);

            if (visionLight != null)
                visionLight.SetActive(visible);
        }

        public void SetVisionLightVisible(bool visible)
        {
            if (visionLight != null)
                visionLight.SetActive(visible);
        }

        public void SetVisualOnlyVisible(bool visible)
        {
            if (visualRoot != null)
                visualRoot.SetActive(visible);

            if (highlightObject != null)
                highlightObject.SetActive(false);

            if (visionLight != null)
                visionLight.SetActive(false);
        }
    }
}