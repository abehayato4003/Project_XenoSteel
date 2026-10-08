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
        private RectTransform miniMapContent;

        [SerializeField]
        private RectTransform fullMapContent;

        [SerializeField]
        private float cellSize = 20f;

        [SerializeField]
        private Color cellColor = Color.white;

        [SerializeField]
        private XenoSteelTacticalMapUnitManager unitManager;

        [SerializeField]
        private XenoSteelTacticalMapEnemyManager enemyManager;

        [SerializeField] private GameObject fullMapPanel;

        [SerializeField]
        private float fullMapRotation = 45f;

        [SerializeField]
        private Vector2Int fullMapMoveMinCell;

        [SerializeField]
        private Vector2Int fullMapMoveMaxCell;

        private readonly List<GameObject> _cellObjects =
            new List<GameObject>();

        private int _minX;
        private int _minY;

        private IUnit _currentUnit;

        private bool _isDragging;
        private Vector2 _lastMousePosition;

        [SerializeField]
        private float fullMapZoomSpeed = 0.1f;

        [SerializeField]
        private float fullMapMinScale = 0.5f;

        [SerializeField]
        private float fullMapMaxScale = 2.0f;

        public void BuildMap(GridController gridController)
        {
            if (gridController == null ||
                miniMapContent == null ||
                fullMapContent == null)
            {
                return;
            }

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

                bool hasUnit =
                    cell.CurrentUnits != null &&
                    cell.CurrentUnits.Count > 0;

                CreateMapCell(
                    cell,
                    miniMapContent,
                    hasUnit
                );

                CreateMapCell(
                    cell,
                    fullMapContent,
                    hasUnit
                );
            }

            float mapWidth = width * cellSize;
            float mapHeight = height * cellSize;

            miniMapContent.sizeDelta =
                new Vector2(mapWidth, mapHeight);

            fullMapContent.sizeDelta =
                new Vector2(mapWidth, mapHeight);

            if (unitManager != null)
            {
                unitManager.SetMapData(
                    miniMapContent,
                    fullMapContent,
                    cellSize,
                    _minX,
                    _minY
                );
            }

            if (enemyManager != null)
            {
                enemyManager.SetMapData(
                    miniMapContent,
                    fullMapContent,
                    cellSize,
                    _minX,
                    _minY
                );
            }

            CenterMap();
        }

        private void CreateMapCell(
            ICell cell,
            RectTransform content,
            bool hasUnit)
        {
            if (content == null)
                return;

            GameObject cellObject = new GameObject(
                $"TacticalCell_{cell.GridCoordinates.x}_{cell.GridCoordinates.y}",
                typeof(RectTransform),
                typeof(Image)
            );

            cellObject.transform.SetParent(
                content,
                false
            );

            RectTransform rect =
                cellObject.GetComponent<RectTransform>();

            Image image =
                cellObject.GetComponent<Image>();

            if (cell.IsTaken)
            {
                image.color = new Color(
                    cellColor.r * 0.35f,
                    cellColor.g * 0.35f,
                    cellColor.b * 0.35f,
                    1f
                );
            }
            else
            {
                image.color = cellColor;
            }

            int x = cell.GridCoordinates.x - _minX;
            int y = cell.GridCoordinates.y - _minY;

            rect.anchorMin =
                new Vector2(0f, 0f);

            rect.anchorMax =
                new Vector2(0f, 0f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                new Vector2(
                    x * cellSize,
                    y * cellSize
                );

            rect.sizeDelta =
                new Vector2(
                    cellSize - 1f,
                    cellSize - 1f
                );

            if (cell.IsTaken && !hasUnit)
            {
                CreateObstacleLine(cellObject, rect.sizeDelta);
            }

            _cellObjects.Add(cellObject);
        }

        private void CreateObstacleLine(
            GameObject cellObject,
            Vector2 cellSize)
        {
            CreateObstacleLinePart(
                cellObject.transform,
                cellSize,
                45f
            );

            CreateObstacleLinePart(
                cellObject.transform,
                cellSize,
                -45f
            );
        }

        private void CreateObstacleLinePart(
            Transform parent,
            Vector2 cellSize,
            float angle)
        {
            GameObject lineObject =
                new GameObject(
                    "ObstacleLine",
                    typeof(RectTransform),
                    typeof(Image)
                );

            lineObject.transform.SetParent(
                parent,
                false
            );

            RectTransform rect =
                lineObject.GetComponent<RectTransform>();

            Image image =
                lineObject.GetComponent<Image>();

            image.color =
                new Color(0f, 0f, 0f, 0.65f);

            rect.anchorMin =
                new Vector2(0.5f, 0.5f);

            rect.anchorMax =
                new Vector2(0.5f, 0.5f);

            rect.pivot =
                new Vector2(0.5f, 0.5f);

            rect.anchoredPosition =
                Vector2.zero;

            rect.sizeDelta =
                new Vector2(
                    cellSize.x * 1.4f,
                    2f
                );

            rect.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );
        }

        private void CenterMap()
        {
            CenterContent(miniMapContent);
            CenterContent(fullMapContent);
        }

        private void CenterContent(RectTransform content)
        {
            if (content == null)
                return;

            content.anchorMin =
                new Vector2(0.5f, 0.5f);

            content.anchorMax =
                new Vector2(0.5f, 0.5f);

            content.pivot =
                new Vector2(0.5f, 0.5f);

            content.anchoredPosition =
                Vector2.zero;
        }

        private void BuildMapContent(
            IEnumerable<ICell> cells,
            int width,
            int height,
            RectTransform content)
        {
            if (content == null)
                return;

            content.sizeDelta = new Vector2(
                width * cellSize,
                height * cellSize
            );

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
                    content,
                    false
                );

                RectTransform rectTransform =
                    cellObject.GetComponent<RectTransform>();

                rectTransform.sizeDelta =
                    new Vector2(cellSize, cellSize);

                int x =
                    cell.GridCoordinates.x - _minX;

                int y =
                    cell.GridCoordinates.y - _minY;

                rectTransform.anchoredPosition =
                    new Vector2(
                        x * cellSize,
                        y * cellSize
                    );

                Image image =
                    cellObject.GetComponent<Image>();

                image.color = cellColor;

                _cellObjects.Add(cellObject);
            }
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

            // 現在ユニットを中心にして45度回転
            miniMapContent.localRotation =
                Quaternion.Euler(0f, 0f, 45f);

            fullMapContent.localRotation =
                Quaternion.Euler(0f, 0f, 45f);

            CenterContentOnPosition(
                miniMapContent,
                targetPosition
            );

            CenterContentOnPosition(
                fullMapContent,
                targetPosition
            );
        }

        public void CenterFullMapOnUnit(IUnit unit)
        {
            if (unit == null ||
                unit.CurrentCell == null ||
                fullMapContent == null)
            {
                return;
            }

            int x =
                unit.CurrentCell.GridCoordinates.x - _minX;

            int y =
                unit.CurrentCell.GridCoordinates.y - _minY;

            Vector2 targetPosition =
                new Vector2(
                    x * cellSize,
                    y * cellSize
                );

            // 現在ユニットを回転中心にする
            fullMapContent.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    fullMapRotation
                );

            CenterContentOnPosition(
                fullMapContent,
                targetPosition
            );
        }

        private void CenterContentOnPosition(
            RectTransform content,
            Vector2 targetPosition)
        {
            if (content == null)
                return;

            Vector2 mapCenter = new Vector2(
                content.sizeDelta.x * 0.5f,
                content.sizeDelta.y * 0.5f
            );

            // Contentの中心を基準にしたユニット位置
            Vector2 offsetFromCenter =
                targetPosition - mapCenter;

            // Contentの回転を考慮して位置を補正
            Vector2 rotatedOffset =
                content.localRotation * offsetFromCenter;

            // ユニットがViewportの中心に来るようにする
            content.anchoredPosition =
                -rotatedOffset;
        }

        public void UpdateAllyUnitMarkers(
            GridController gridController,
            IUnit currentUnit)
        {
            if (unitManager == null)
                return;

            _currentUnit = currentUnit;

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

        public void OpenFullMap()
        {
            if (fullMapPanel == null)
                return;

            fullMapPanel.SetActive(true);

            UpdateEnemyMarkers();

            CenterFullMapOnUnit(_currentUnit);
        }
        
        public void CloseFullMap()
        {
            if (fullMapPanel == null)
                return;

            fullMapPanel.SetActive(false);
        }

        private void Update()
        {
            if (fullMapPanel == null ||
                !fullMapPanel.activeSelf ||
                fullMapContent == null)
            {
                return;
            }

            HandleFullMapInput();
        }

        private void HandleFullMapInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }

            if (_isDragging)
            {
                Vector2 currentMousePosition =
                    Input.mousePosition;

                Vector2 delta =
                    currentMousePosition - _lastMousePosition;

                fullMapContent.anchoredPosition += delta;

                ClampFullMapPosition();

                _lastMousePosition = currentMousePosition;
            }

            float scroll =
                Input.mouseScrollDelta.y;

            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 scale =
                    fullMapContent.localScale;

                float newScale =
                    Mathf.Clamp(
                        scale.x + scroll * fullMapZoomSpeed,
                        fullMapMinScale,
                        fullMapMaxScale
                    );

                fullMapContent.localScale =
                    new Vector3(
                        newScale,
                        newScale,
                        1f
                    );
            }
        }

        private void ClampFullMapPosition()
        {
            if (fullMapContent == null)
                return;

            Vector2 mapCenter = new Vector2(
                fullMapContent.sizeDelta.x * 0.5f,
                fullMapContent.sizeDelta.y * 0.5f
            );

            Quaternion rotation =
                Quaternion.Euler(0f, 0f, 45f);

            Vector2 offset =
                -(Quaternion.Inverse(rotation) *
                fullMapContent.anchoredPosition);

            Vector2 targetPosition =
                mapCenter + offset;

            float minX =
                (fullMapMoveMinCell.x - _minX) * cellSize;

            float minY =
                (fullMapMoveMinCell.y - _minY) * cellSize;

            float maxX =
                (fullMapMoveMaxCell.x - _minX) * cellSize;

            float maxY =
                (fullMapMoveMaxCell.y - _minY) * cellSize;

            targetPosition.x =
                Mathf.Clamp(
                    targetPosition.x,
                    minX,
                    maxX
                );

            targetPosition.y =
                Mathf.Clamp(
                    targetPosition.y,
                    minY,
                    maxY
                );

            Vector2 newOffset =
                targetPosition - mapCenter;

            fullMapContent.anchoredPosition =
                -(rotation * newOffset);
        }
    }
}