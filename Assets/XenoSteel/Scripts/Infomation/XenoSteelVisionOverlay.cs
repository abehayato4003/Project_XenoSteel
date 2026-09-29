using UnityEngine;

namespace XenoSteel.Information
{
    public class XenoSteelVisionOverlay : MonoBehaviour
    {
        [SerializeField]
        private GameObject overlayObject;

        public void SetVisible(bool visible)
        {
            if (overlayObject == null)
                return;

            overlayObject.SetActive(!visible);
        }
    }
}