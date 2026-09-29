using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TurnBasedStrategyFramework.Common.Cells;
using TurnBasedStrategyFramework.Common.Controllers;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMap : MonoBehaviour
    {
        [SerializeField]
        private RectTransform mapContent;

        [SerializeField]
        private float cellSize = 20f;

        [SerializeField]
        private Color cellColor = Color.white;

        [SerializeField]
        private XenoSteelTacticalMapUnitManager unitManager;

        [SerializeField]
        private XenoSteelTacticalMapEnemyManager enemyManager;

        private readonly List<GameObject> _cellObjects =
            new List<GameObject>();

        private int _minX;
        private int _minY;

        public void BuildMap(GridController gridController)
        {
            if (gridController == null || mapContent == null)
                return;

            ClearMap();

            var cells = gridController.CellManager.GetCells();

            if (cells == null)
                return;

            _minX = int.MaxValue;
            int maxX = int.MinValue;
            _minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (ICell cell in cells)
            {
                if (cell == null)
                    continue;

                int x = cell.GridCoordinates.x;
                int y = cell.GridCoordinates.y;

                _minX = Mathf.Min(_minX, x);
                maxX = Mathf.Max(maxX, x);
                _minY = Mathf.Min(_minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            if (_minX == int.MaxValue)
                return;

            int width = maxX - _minX + 1;
            int height = maxY - _minY + 1;

            foreach (ICell cell in cells)
            {
                if (cell == null)
                    continue;

                GameObject cellObject = new GameObject(
                    $"TacticalCell_{cell.GridCoordinates.x}_{cell.GridCoordinates.y}",
                    typeof(RectTransform),
                    typeof(Image)
                );

                cellObject.transform.SetParent(
                    mapContent,
                    false
                );

                RectTransform rect =
                    cellObject.GetComponent<RectTransform>();

                Image image =
                    cellObject.GetComponent<Image>();

                image.color = cellColor;

                int x = cell.GridCoordinates.x - _minX;
                int y = cell.GridCoordinates.y - _minY;

                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0.5f, 0.5f);

                rect.anchoredPosition = new Vector2(
                    x * cellSize,
                    y * cellSize
                );

                rect.sizeDelta =
                    new Vector2(cellSize - 1f, cellSize - 1f);

                _cellObjects.Add(cellObject);
            }

            float mapWidth = width * cellSize;
            float mapHeight = height * cellSize;

            mapContent.sizeDelta =
                new Vector2(mapWidth, mapHeight);

            if (unitManager != null)
            {
                unitManager.SetMapData(
                    mapContent,
                    cellSize,
                    _minX,
                    _minY
                );
            }

            if (enemyManager != null)
            {
                enemyManager.SetMapData(
                    mapContent,
                    cellSize,
                    _minX,
                    _minY
                );
            }

            CenterMap();
        }

        private void CenterMap()
        {
            mapContent.anchorMin =
                new Vector2(0.5f, 0.5f);

            mapContent.anchorMax =
                new Vector2(0.5f, 0.5f);

            mapContent.pivot =
                new Vector2(0.5f, 0.5f);

            mapContent.anchoredPosition =
                Vector2.zero;
        }

        public void CenterOnUnit(IUnit unit)
        {
            if (unit == null || unit.CurrentCell == null)
                return;

            int x =
                unit.CurrentCell.GridCoordinates.x - _minX;

            int y =
                unit.CurrentCell.GridCoordinates.y - _minY;

            Vector2 targetPosition = new Vector2(
                x * cellSize,
                y * cellSize
            );

            Vector2 mapCenter = new Vector2(
                mapContent.sizeDelta.x * 0.5f,
                mapContent.sizeDelta.y * 0.5f
            );

            mapContent.anchoredPosition =
                mapCenter - targetPosition;
        }

        public void UpdateAllyUnitMarkers(
            GridController gridController,
            IUnit currentUnit)
        {
            Debug.Log(
                $"UpdateAllyUnitMarkers: unitManager={unitManager}"
            );

            if (unitManager == null)
                return;

            unitManager.UpdateAllyUnitMarkers(
                gridController,
                currentUnit
            );
        }

        public void UpdateEnemyMarkers()
        {
            if (enemyManager == null)
                return;

            XenoSteelInformationManager informationManager =
                Object.FindFirstObjectByType<XenoSteelInformationManager>();

            if (informationManager == null)
                return;

            enemyManager.UpdateConfirmedEnemyMarkers(
                informationManager
            );
        }

        private void ClearMap()
        {
            foreach (GameObject cellObject in _cellObjects)
            {
                if (cellObject != null)
                    Destroy(cellObject);
            }

            _cellObjects.Clear();
        }
    }
}