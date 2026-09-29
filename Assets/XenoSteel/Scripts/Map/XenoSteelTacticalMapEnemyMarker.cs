using UnityEngine;
using UnityEngine.UI;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapEnemyMarker : MonoBehaviour
    {
        [SerializeField]
        private Sprite enemySprite;

        [SerializeField]
        private Sprite lastKnownSprite;

        [SerializeField]
        private float markerSize = 24f;

        private Image _markerImage;

        private void Awake()
        {
            _markerImage = GetComponent<Image>();

            if (_markerImage == null)
                _markerImage = gameObject.AddComponent<Image>();
        }

        public void SetEnemy()
        {
            if (_markerImage == null)
                return;

            _markerImage.sprite = enemySprite;
            _markerImage.enabled = true;

            RectTransform rectTransform =
                GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.sizeDelta =
                    new Vector2(markerSize, markerSize);
            }
        }

        public void SetLastKnown()
        {
            if (_markerImage == null)
                return;

            _markerImage.sprite = lastKnownSprite;
            _markerImage.enabled = true;

            RectTransform rectTransform =
                GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.sizeDelta =
                    new Vector2(markerSize, markerSize);
            }
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