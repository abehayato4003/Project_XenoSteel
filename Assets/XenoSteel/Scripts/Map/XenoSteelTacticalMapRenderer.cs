using System.Collections.Generic;

using UnityEngine;

using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapRenderer
    {
        private RectTransform _miniMapContent;
        private RectTransform _fullMapContent;

        private XenoSteelTacticalMapEnemyMarker _markerPrefab;
        private XenoSteelTacticalMapRadarMarker _radarMarkerPrefab;
        private float _cellSize;

        private readonly List<XenoSteelTacticalMapEnemyMarker> _enemyMarkers =
            new List<XenoSteelTacticalMapEnemyMarker>();

        private readonly List<XenoSteelTacticalMapRadarMarker> _radarMarkers =
            new List<XenoSteelTacticalMapRadarMarker>();

        private int _minX;
        private int _minY;

        public XenoSteelTacticalMapRenderer(
            XenoSteelTacticalMapEnemyMarker markerPrefab,
            XenoSteelTacticalMapRadarMarker radarMarkerPrefab)
        {
            _markerPrefab = markerPrefab;
            _radarMarkerPrefab = radarMarkerPrefab;
        }

        public void SetMapData(
            RectTransform miniContent,
            RectTransform fullContent,
            float size,
            int minX,
            int minY)
        {
            _miniMapContent = miniContent;
            _fullMapContent = fullContent;
            _cellSize = size;
            _minX = minX;
            _minY = minY;
        }

        public void UpdateConfirmedEnemyMarkers(
            XenoSteelInformationManager informationManager)
        {
            if (informationManager == null ||
                _miniMapContent == null ||
                _fullMapContent == null)
            {
                return;
            }

            Clear();

            foreach (
                KeyValuePair<object, XenoSteelEnemyInformation> pair
                in informationManager.GetAllInformations())
            {
                object enemyObject = pair.Key;
                XenoSteelEnemyInformation information = pair.Value;

                if (enemyObject == null || information == null)
                {
                    continue;
                }

                IUnit enemyUnit = enemyObject as IUnit;

                if (enemyUnit == null)
                {
                    continue;
                }

                if (enemyUnit.PlayerNumber == 0)
                {
                    continue;
                }

                if (enemyUnit.Health <= 0)
                {
                    continue;
                }

                if (information.State ==
                    XenoSteelEnemyInformationState.LastKnown)
                {
                    CreateLastKnownMarker(
                        information,
                        _miniMapContent
                    );

                    CreateLastKnownMarker(
                        information,
                        _fullMapContent
                    );

                    continue;
                }

                if (information.State !=
                    XenoSteelEnemyInformationState.Confirmed)
                {
                    continue;
                }

                if (information.Source ==
                    XenoSteelInformationSource.Visual)
                {
                    CreateVisualMarker(
                        information,
                        _miniMapContent
                    );

                    CreateVisualMarker(
                        information,
                        _fullMapContent
                    );

                    continue;
                }

                if (information.Source ==
                    XenoSteelInformationSource.Radar)
                {
                    CreateRadarMarker(
                        information,
                        _miniMapContent
                    );

                    CreateRadarMarker(
                        information,
                        _fullMapContent
                    );
                }
            }
        }

        private void CreateLastKnownMarker(
            XenoSteelEnemyInformation information,
            RectTransform content)
        {
            if (_markerPrefab == null)
            {
                return;
            }

            XenoSteelTacticalMapEnemyMarker marker =
                Object.Instantiate(
                    _markerPrefab,
                    content);

            marker.SetLastKnown();

            Vector2 position =
                GetMapPosition(
                    information.LastKnownCell,
                    content
                );

            marker.SetPosition(position);

            _enemyMarkers.Add(marker);
        }

        private void CreateVisualMarker(
            XenoSteelEnemyInformation information,
            RectTransform content)
        {
            if (_markerPrefab == null)
            {
                return;
            }

            XenoSteelTacticalMapEnemyMarker marker =
                Object.Instantiate(
                    _markerPrefab,
                    content);

            marker.SetEnemy();

            Vector2 position =
                GetMapPosition(
                    information.LastKnownCell,
                    content
                );

            marker.SetPosition(position);

            _enemyMarkers.Add(marker);
        }

        private void CreateRadarMarker(
            XenoSteelEnemyInformation information,
            RectTransform content)
        {
            if (_radarMarkerPrefab == null)
            {
                return;
            }

            int precision =
                information.InformationPrecision;

            if (precision <= 0)
            {
                return;
            }

            float radius =
                GetRadarRadius(precision);

            Vector2Int centerCell =
                GetRadarCenter(
                    information.LastKnownCell,
                    precision);

            XenoSteelTacticalMapRadarMarker marker =
                Object.Instantiate(
                    _radarMarkerPrefab,
                    content);

            marker.SetRadarMarker(
                radius,
                _cellSize);

            marker.SetPosition(
                GetMapPosition(
                    centerCell,
                    content
                ));

            _radarMarkers.Add(marker);
        }

        private float GetRadarRadius(int precision)
        {
            switch (precision)
            {
                case 1:
                    return 5f;

                case 2:
                    return 4f;

                case 3:
                    return 3f;

                case 4:
                    return 2f;

                case 5:
                    return 1f;

                default:
                    return 0f;
            }
        }

        private Vector2Int GetRadarCenter(
            Vector2Int cell,
            int precision)
        {
            int zoneSize;

            if (precision <= 2)
            {
                zoneSize = 5;
            }
            else if (precision == 3)
            {
                zoneSize = 3;
            }
            else
            {
                zoneSize = 2;
            }

            int centerX =
                Mathf.FloorToInt(
                    (float)cell.x / zoneSize) * zoneSize;

            int centerY =
                Mathf.FloorToInt(
                    (float)cell.y / zoneSize) * zoneSize;

            Vector2Int center =
                new Vector2Int(centerX, centerY);

            // 実際の敵セルと一致した場合は
            // 1セルずらして正確な位置を隠す
            if (center == cell)
            {
                center += Vector2Int.right;
            }

            return center;
        }

        private Vector2 GetMapPosition(
            Vector2Int cell,
            RectTransform content)
        {
            int x = cell.x - _minX;
            int y = cell.y - _minY;

            Vector2 targetPosition =
                new Vector2(
                    x * _cellSize,
                    y * _cellSize);

            Vector2 mapCenter =
                new Vector2(
                    content.sizeDelta.x * 0.5f,
                    content.sizeDelta.y * 0.5f);

            return targetPosition - mapCenter;
        }

        public void Clear()
        {
            foreach (
                XenoSteelTacticalMapEnemyMarker marker
                in _enemyMarkers)
            {
                if (marker != null)
                {
                    Object.Destroy(marker.gameObject);
                }
            }

            _enemyMarkers.Clear();

            foreach (
                XenoSteelTacticalMapRadarMarker marker
                in _radarMarkers)
            {
                if (marker != null)
                {
                    Object.Destroy(marker.gameObject);
                }
            }

            _radarMarkers.Clear();
        }
    }
}