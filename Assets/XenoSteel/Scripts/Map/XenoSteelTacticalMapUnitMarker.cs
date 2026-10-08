using UnityEngine;
using UnityEngine.UI;
using TurnBasedStrategyFramework.Common.Units;
using XenoSteel.Units;

namespace XenoSteel.Information
{
    public class XenoSteelTacticalMapUnitMarker : MonoBehaviour
    {
        [SerializeField]
        private Sprite currentUnitSprite;

        [SerializeField]
        private Sprite allyUnitSprite;

        [SerializeField]
        private float markerSize = 24f;

        private Image _markerImage;

        private void Awake()
        {
            _markerImage = GetComponent<Image>();

            if (_markerImage == null)
                _markerImage = gameObject.AddComponent<Image>();
        }

        public void SetUnit(IUnit unit, bool isCurrentUnit)
        {
            if (unit == null)
                return;

            if (_markerImage == null)
            {
                _markerImage = GetComponent<Image>();

                if (_markerImage == null)
                    _markerImage = gameObject.AddComponent<Image>();
            }

            _markerImage.sprite =
                isCurrentUnit
                    ? currentUnitSprite
                    : allyUnitSprite;

            _markerImage.enabled = true;

            RectTransform rectTransform =
                GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                rectTransform.sizeDelta =
                    new Vector2(markerSize, markerSize);

                if (isCurrentUnit)
                {
                    XenoSteelUnitFacing facing =
                        GetFacingComponent(unit);

                    if (facing != null)
                    {
                        rectTransform.localRotation =
                            Quaternion.Euler(
                                0f,
                                0f,
                                GetMarkerRotation(facing.Direction)
                            );
                    }
                }
                else
                {
                    rectTransform.localRotation =
                        Quaternion.identity;
                }
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

        private XenoSteelUnitFacing GetFacingComponent(IUnit unit)
        {
            var unityUnit =
                unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unityUnit == null)
                return null;

            return unityUnit.GetComponent<XenoSteelUnitFacing>();
        }

        private float GetMarkerRotation(
            XenoSteelUnitFacing.FacingDirection direction)
        {
            return direction switch
            {
                XenoSteelUnitFacing.FacingDirection.Up => 0f,
                XenoSteelUnitFacing.FacingDirection.Right => -90f,
                XenoSteelUnitFacing.FacingDirection.Down => 180f,
                XenoSteelUnitFacing.FacingDirection.Left => 90f,
                _ => 0f
            };
        }
    }
}