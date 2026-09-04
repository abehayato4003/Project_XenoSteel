using UnityEngine;
using TMPro;

namespace XenoSteel.Combat.UI
{
    public class XenoSteelDamagePopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private float moveSpeed = 50f;
        [SerializeField] private float duration = 0.8f;

        private float _timer;

        public void Initialize(int damage)
        {
            damageText.text = damage.ToString();
            _timer = duration;
        }

        private void Update()
        {
            RectTransform rectTransform =
                GetComponent<RectTransform>();

            rectTransform.anchoredPosition +=
                Vector2.up * moveSpeed * Time.deltaTime;

            _timer -= Time.deltaTime;

            if (_timer <= 0f)
            {
                Destroy(gameObject);
            }
        }
    }
}