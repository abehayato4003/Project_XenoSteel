using System.Collections.Generic;
using UnityEngine;
using TurnBasedStrategyFramework.Common.Cells;

namespace XenoSteel.Information
{
    public class XenoSteelVisionFog : MonoBehaviour
    {
        [SerializeField]
        private Renderer fogRenderer;

        [SerializeField]
        private int textureSize = 256;

        [SerializeField]
        private float edgeSoftness = 2.0f;

        [SerializeField]
        private float fogDensity = 1.0f;

        private Texture2D _fogMask;

        private void Awake()
        {
            if (fogRenderer == null)
                fogRenderer = GetComponent<Renderer>();

            _fogMask = new Texture2D(
                textureSize,
                textureSize,
                TextureFormat.R8,
                false);

            _fogMask.wrapMode = TextureWrapMode.Clamp;
            _fogMask.filterMode = FilterMode.Bilinear;

            if (fogRenderer != null)
            {
                fogRenderer.material.SetTexture(
                    "_FogMask",
                    _fogMask);

                fogRenderer.material.SetFloat(
                    "_FogDensity",
                    fogDensity);
            }
        }

        public void UpdateFog(
            ICell originCell,
            int visionRange,
            IEnumerable<ICell> allCells)
        {
            if (_fogMask == null ||
                fogRenderer == null ||
                originCell == null ||
                allCells == null)
            {
                return;
            }

            List<ICell> cells =
                new List<ICell>(allCells);

            if (cells.Count == 0)
                return;

            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (ICell cell in cells)
            {
                if (cell == null)
                    continue;

                minX = Mathf.Min(
                    minX,
                    cell.GridCoordinates.x);

                maxX = Mathf.Max(
                    maxX,
                    cell.GridCoordinates.x);

                minY = Mathf.Min(
                    minY,
                    cell.GridCoordinates.y);

                maxY = Mathf.Max(
                    maxY,
                    cell.GridCoordinates.y);
            }

            Color32[] pixels =
                new Color32[
                    textureSize * textureSize];

            float width =
                Mathf.Max(maxX - minX + 1, 1);

            float height =
                Mathf.Max(maxY - minY + 1, 1);

            Vector2 origin =
                new Vector2(
                    originCell.GridCoordinates.x + 0.5f,
                    originCell.GridCoordinates.y + 0.5f);

            float range =
                Mathf.Max(visionRange, 0.001f);

            for (int py = 0;
                 py < textureSize;
                 py++)
            {
                float gridY =
                    minY +
                    ((float)py / (textureSize - 1)) *
                    height;

                for (int px = 0;
                     px < textureSize;
                     px++)
                {
                    float gridX =
                        minX +
                        ((float)px / (textureSize - 1)) *
                        width;

                    float distance =
                        Vector2.Distance(
                            new Vector2(
                                gridX,
                                gridY),
                            origin);

                    float visibility;

                    if (distance >= range)
                    {
                        visibility = 0.0f;
                    }
                    else
                    {
                        float edgeStart =
                            Mathf.Max(
                                range - edgeSoftness,
                                0.0f);

                        if (distance <= edgeStart)
                        {
                            visibility = 1.0f;
                        }
                        else
                        {
                            visibility =
                                1.0f -
                                Mathf.InverseLerp(
                                    edgeStart,
                                    range,
                                    distance);
                        }
                    }

                    byte value =
                        (byte)(visibility * 255f);

                    pixels[
                        py * textureSize + px] =
                        new Color32(
                            value,
                            value,
                            value,
                            255);
                }
            }

            _fogMask.SetPixels32(pixels);
            _fogMask.Apply();

            fogRenderer.material.SetTexture(
                "_FogMask",
                _fogMask);

            fogRenderer.material.SetVector(
                "_GridMin",
                new Vector4(
                    minX,
                    minY,
                    0f,
                    0f));

            fogRenderer.material.SetVector(
                "_GridMax",
                new Vector4(
                    maxX + 1,
                    maxY + 1,
                    0f,
                    0f));
        }
    }
}