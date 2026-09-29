using UnityEngine;
using UnityEngine.UI;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapRadarMarker : MonoBehaviour
    {
        [SerializeField]
        private float markerSize = 100f;

        [SerializeField]
        private Color markerColor = new Color(1f, 1f, 1f, 0.25f);

        private Image _markerImage;

        private void Awake()
        {
            _markerImage = GetComponent<Image>();

            if (_markerImage == null)
                _markerImage = gameObject.AddComponent<Image>();
        }

        public void SetRadarMarker(float radius, float cellSize)
        {
            if (_markerImage == null)
                return;

            float diameter =
                (radius * 2f + 1f) * cellSize;

            RectTransform rectTransform =
                GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.sizeDelta =
                    new Vector2(diameter, diameter);
            }

            _markerImage.color = markerColor;
            _markerImage.enabled = true;
        }

        public void SetPosition(Vector2 position)
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            if (rectTransform == null)
                return;

            rectTransform.anchoredPosition = position;
        }
    }
}