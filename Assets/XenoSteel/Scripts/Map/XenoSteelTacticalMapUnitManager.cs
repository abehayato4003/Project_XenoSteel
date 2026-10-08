using System.Collections.Generic;

using UnityEngine;

using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapUnitManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform miniMapContent;

        [SerializeField]
        private RectTransform fullMapContent;

        [SerializeField]
        private XenoSteelTacticalMapUnitMarker markerPrefab;

        [SerializeField]
        private float cellSize = 20f;

        private readonly List<XenoSteelTacticalMapUnitMarker> _unitMarkers =
            new List<XenoSteelTacticalMapUnitMarker>();

        private int _minX;
        private int _minY;

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

        public void UpdateAllyUnitMarkers(
            GridController gridController,
            IUnit currentUnit)
        {
            if (gridController == null ||
                miniMapContent == null ||
                fullMapContent == null)
            {
                return;
            }

            ClearUnitMarkers();

            var cells = gridController.CellManager.GetCells();

            if (cells == null)
            {
                return;
            }

            foreach (ICell cell in cells)
            {
                if (cell == null || cell.CurrentUnits == null)
                {
                    continue;
                }

                foreach (IUnit unit in cell.CurrentUnits)
                {
                    if (unit == null)
                    {
                        continue;
                    }

                    if (unit.PlayerNumber != 0)
                    {
                        continue;
                    }

                    if (unit.CurrentCell == null)
                    {
                        continue;
                    }

                    if (markerPrefab == null)
                    {
                        continue;
                    }

                    CreateUnitMarker(
                        unit,
                        currentUnit,
                        miniMapContent
                    );

                    CreateUnitMarker(
                        unit,
                        currentUnit,
                        fullMapContent
                    );
                }
            }
        }

        private void CreateUnitMarker(
            IUnit unit,
            IUnit currentUnit,
            RectTransform content)
        {
            if (content == null || markerPrefab == null)
            {
                return;
            }

            XenoSteelTacticalMapUnitMarker marker =
                Instantiate(
                    markerPrefab,
                    content
                );

            marker.SetUnit(
                unit,
                unit == currentUnit
            );

            int x =
                unit.CurrentCell.GridCoordinates.x - _minX;

            int y =
                unit.CurrentCell.GridCoordinates.y - _minY;

            Vector2 targetPosition =
                new Vector2(
                    x * cellSize,
                    y * cellSize
                );

            Vector2 mapCenter =
                new Vector2(
                    content.sizeDelta.x * 0.5f,
                    content.sizeDelta.y * 0.5f
                );

            Vector2 position =
                targetPosition - mapCenter;

            marker.SetPosition(position);

            _unitMarkers.Add(marker);
        }

        private void ClearUnitMarkers()
        {
            foreach (
                XenoSteelTacticalMapUnitMarker marker
                in _unitMarkers)
            {
                if (marker != null)
                {
                    Destroy(marker.gameObject);
                }
            }

            _unitMarkers.Clear();
        }
    }
}