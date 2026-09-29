using System.Collections.Generic;
using UnityEngine;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapEnemyManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform mapContent;

        [SerializeField]
        private XenoSteelTacticalMapEnemyMarker markerPrefab;

        [SerializeField]
        private XenoSteelTacticalMapRadarMarker radarMarkerPrefab;

        [SerializeField]
        private float cellSize = 20f;

        private readonly List<XenoSteelTacticalMapEnemyMarker> _enemyMarkers =
            new List<XenoSteelTacticalMapEnemyMarker>();

        private readonly List<XenoSteelTacticalMapRadarMarker> _radarMarkers =
            new List<XenoSteelTacticalMapRadarMarker>();

        private int _minX;
        private int _minY;

        public void SetMapData(
            RectTransform content,
            float size,
            int minX,
            int minY)
        {
            mapContent = content;
            cellSize = size;
            _minX = minX;
            _minY = minY;
        }

        public void UpdateConfirmedEnemyMarkers(
            XenoSteelInformationManager informationManager)
        {
            if (informationManager == null || mapContent == null)
                return;

            ClearEnemyMarkers();

            foreach (
                KeyValuePair<object, XenoSteelEnemyInformation> pair
                in informationManager.GetAllInformations())
            {
                object enemyObject = pair.Key;
                XenoSteelEnemyInformation information = pair.Value;

                if (enemyObject == null || information == null)
                    continue;

                IUnit enemyUnit = enemyObject as IUnit;

                if (enemyUnit == null)
                    continue;

                // プレイヤー側の戦術マップには敵ユニットだけ表示する
                if (enemyUnit.PlayerNumber == 0)
                    continue;

                if (information.State ==
                    XenoSteelEnemyInformationState.LastKnown)
                {
                    CreateLastKnownMarker(information);
                    continue;
                }

                if (information.State !=
                    XenoSteelEnemyInformationState.Confirmed)
                    continue;

                if (information.Source ==
                    XenoSteelInformationSource.Visual)
                {
                    CreateVisualMarker(information);
                    continue;
                }

                if (information.Source ==
                    XenoSteelInformationSource.Radar)
                {
                    CreateRadarMarker(information);
                }
            }
        }

        private void CreateLastKnownMarker(
            XenoSteelEnemyInformation information)
        {
            if (markerPrefab == null)
                return;

            XenoSteelTacticalMapEnemyMarker marker =
                Instantiate(markerPrefab, mapContent);

            marker.SetLastKnown();

            Vector2 position =
                GetMapPosition(information.LastKnownCell);

            marker.SetPosition(position);

            _enemyMarkers.Add(marker);
        }

        private void CreateVisualMarker(
            XenoSteelEnemyInformation information)
        {
            if (markerPrefab == null)
                return;

            XenoSteelTacticalMapEnemyMarker marker =
                Instantiate(markerPrefab, mapContent);

            marker.SetEnemy();

            Vector2 position =
                GetMapPosition(information.LastKnownCell);

            marker.SetPosition(position);

            _enemyMarkers.Add(marker);
        }

        private void CreateRadarMarker(
            XenoSteelEnemyInformation information)
        {
            if (radarMarkerPrefab == null)
                return;

            int precision =
                information.InformationPrecision;

            if (precision <= 0)
                return;

            float radius =
                GetRadarRadius(precision);

            Vector2Int centerCell =
                GetRadarCenter(
                    information.LastKnownCell,
                    precision);

            XenoSteelTacticalMapRadarMarker marker =
                Instantiate(
                    radarMarkerPrefab,
                    mapContent);

            marker.SetRadarMarker(
                radius,
                cellSize);

            marker.SetPosition(
                GetMapPosition(centerCell));

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
            // 1セルずらして正確な位置を隠す。
            if (center == cell)
            {
                center += Vector2Int.right;
            }

            return center;
        }

        private Vector2 GetMapPosition(
            Vector2Int cell)
        {
            int x =
                cell.x - _minX;

            int y =
                cell.y - _minY;

            Vector2 targetPosition =
                new Vector2(
                    x * cellSize,
                    y * cellSize);

            Vector2 mapCenter =
                new Vector2(
                    mapContent.sizeDelta.x * 0.5f,
                    mapContent.sizeDelta.y * 0.5f);

            return targetPosition - mapCenter;
        }

        private void ClearEnemyMarkers()
        {
            foreach (
                XenoSteelTacticalMapEnemyMarker marker
                in _enemyMarkers)
            {
                if (marker != null)
                    Destroy(marker.gameObject);
            }

            _enemyMarkers.Clear();

            foreach (
                XenoSteelTacticalMapRadarMarker marker
                in _radarMarkers)
            {
                if (marker != null)
                    Destroy(marker.gameObject);
            }

            _radarMarkers.Clear();
        }
    }
}