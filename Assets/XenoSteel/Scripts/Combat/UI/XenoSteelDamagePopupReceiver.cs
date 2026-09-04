using UnityEngine;
using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Combat.UI
{
    public class XenoSteelDamagePopupReceiver : MonoBehaviour
    {
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private Canvas damageCanvas;
        [SerializeField] private Transform popupPoint;

        private IUnit _unit;

        private void Awake()
        {
            _unit = GetComponent<IUnit>();
        }

        private void OnEnable()
        {
            if (_unit != null)
            {
                _unit.UnitAttacked += OnUnitAttacked;
            }
        }

        private void OnDisable()
        {
            if (_unit != null)
            {
                _unit.UnitAttacked -= OnUnitAttacked;
            }
        }

        private void OnUnitAttacked(UnitAttackedEventArgs eventArgs)
        {
            if (damagePopupPrefab == null)
            {
                Debug.LogWarning(
                    $"Damage Popup Prefab is not assigned on {gameObject.name}.");
                return;
            }

            if (damageCanvas == null)
            {
                Debug.LogWarning(
                    $"Damage Canvas is not assigned on {gameObject.name}.");
                return;
            }

            Vector3 worldPosition = popupPoint != null
                ? popupPoint.position
                : transform.position + Vector3.up;

            Vector2 screenPosition =
                Camera.main.WorldToScreenPoint(worldPosition);

            RectTransform canvasRect =
                damageCanvas.GetComponent<RectTransform>();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                damageCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : damageCanvas.worldCamera,
                out Vector2 localPosition);

            GameObject popup =
                Instantiate(
                    damagePopupPrefab,
                    damageCanvas.transform);

            RectTransform popupRect =
                popup.GetComponent<RectTransform>();

            popupRect.anchoredPosition = localPosition;

            XenoSteelDamagePopup damagePopup =
                popup.GetComponent<XenoSteelDamagePopup>();

            if (damagePopup != null)
            {
                damagePopup.Initialize(
                    Mathf.RoundToInt(eventArgs.DamageDealt));
            }
        }
    }
}