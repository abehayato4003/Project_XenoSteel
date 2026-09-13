using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace XenoSteel.Units
{
    public class XenoSteelSkillDetailUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [Header("表示")]
        [SerializeField] private TMP_Text skillNameText;
        [SerializeField] private TMP_Text powerText;
        [SerializeField] private TMP_Text rangeText;
        [SerializeField] private TMP_Text energyCostText;
        [SerializeField] private TMP_Text attributeText;
        [SerializeField] private TMP_Text areaText;

        [SerializeField] private Button closeButton;

        [SerializeField] private CanvasGroup canvasGroup;

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
            Hide();
        }

        public void Show(XenoSteel.Combat.SkillData skill)
        {
            Debug.Log("SkillDetailUI.Show: " + skill.skillName);
            
            if (skill == null)
            {
                Hide();
                return;
            }

            skillNameText.text = skill.skillName;
            powerText.text = $"威力 {skill.power}";
            rangeText.text = $"射程 {skill.range}";
            energyCostText.text = $"EN {skill.energyCost}";
            attributeText.text = $"属性 {skill.attribute}";
            areaText.text = $"範囲 {skill.area}";

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}