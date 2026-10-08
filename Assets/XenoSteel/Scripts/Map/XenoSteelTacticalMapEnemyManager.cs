using UnityEngine;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapEnemyManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform miniMapContent;

        [SerializeField]
        private RectTransform fullMapContent;

        [SerializeField]
        private XenoSteelTacticalMapEnemyMarker markerPrefab;

        [SerializeField]
        private XenoSteelTacticalMapRadarMarker radarMarkerPrefab;

        [SerializeField]
        private float cellSize = 20f;

        private int _minX;
        private int _minY;

        private XenoSteelTacticalMapRenderer _renderer;

        private void Awake()
        {
            _renderer =
                new XenoSteelTacticalMapRenderer(
                    markerPrefab,
                    radarMarkerPrefab);
        }

        public void SetMapData(
            RectTransform miniContent,
            RectTransform fullContent,
            float size,
            int minX,
            int minY)
        {
            miniMapContent = miniContent;
            fullMapContent = fullContent;
            cellSize = size;
            _minX = minX;
            _minY = minY;
        }

        public void UpdateConfirmedEnemyMarkers(
            XenoSteelInformationManager informationManager)
        {
            if (_renderer == null)
            {
                return;
            }

            _renderer.SetMapData(
                miniMapContent,
                fullMapContent,
                cellSize,
                _minX,
                _minY
            );

            _renderer.UpdateConfirmedEnemyMarkers(
                informationManager
            );
        }
    }
}