using UnityEngine;
using UnityEngine.UI;
using TMPro;

using TurnBasedStrategyFramework.Common.Units;

namespace XenoSteel.Units
{
    public class XenoSteelSkillSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject slotPrefab;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private XenoSteelSkillDetailUI skillDetailUI;


        private XenoSteel.Combat.XenoSteelAttackAbility _attackAbility;

        public void Show(
            XenoSteel.Combat.XenoSteelAttackAbility attackAbility)
        {
            Debug.Log("SkillSelectionUI.Show");

            _attackAbility = attackAbility;

            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            var unit =
                attackAbility.UnitReference as
                TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unit == null)
            {
                return;
            }

            var initiative =
                unit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            if (initiative == null ||
                initiative.UnitData == null)
            {
                return;
            }

            var skills = initiative.UnitData.skills;

            if (skills == null || skills.Length == 0)
            {
                return;
            }

            panel.SetActive(true);

            foreach (var skill in skills)
            {
                if (skill == null)
                {
                    continue;
                }

                var slot =
                    Instantiate(slotPrefab, content);

                var texts =
                    slot.GetComponentsInChildren<TMP_Text>();

                if (texts.Length > 0)
                {
                    texts[0].text = skill.skillName;
                }

                var skillButton =
                    slot.GetComponent<Button>();

                if (skillButton != null)
                {
                    skillButton.onClick.AddListener(() =>
                    {
                        _attackAbility.SetCurrentSkill(skill);
                    });
                }
            }
        }

        public void SetCurrentUnit(IUnit unit, bool isPlayerUnit)
        {
            var unityUnit =
                unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unityUnit == null)
            {
                Hide();
                return;
            }

            if (!isPlayerUnit)
            {
                Hide();
                return;
            }

            var initiative =
                unityUnit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            if (initiative == null || initiative.UnitData == null)
            {
                Hide();
                return;
            }

            // 現在のユニットのAttackAbilityを取得
            _attackAbility =
                unityUnit.GetComponent<XenoSteel.Combat.XenoSteelAttackAbility>();

            ShowSkills(initiative.UnitData.skills);
        }

        private void ShowSkills(XenoSteel.Combat.SkillData[] skills)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (skills == null || skills.Length == 0)
            {
                Hide();
                return;
            }

            foreach (var skill in skills)
            {
                if (skill == null)
                {
                    continue;
                }

                var slot = Instantiate(slotPrefab, content);

                var texts = slot.GetComponentsInChildren<TMP_Text>();

                if (texts.Length > 0)
                {
                    texts[0].text = skill.skillName;
                }

                var skillButton = slot.GetComponent<Button>();

                if (skillButton != null)
                {
                    skillButton.onClick.AddListener(() =>
                    {
                        _attackAbility.SetCurrentSkill(skill);
                    });
                }

                var detailButton =
                    slot.transform.Find("DetailButton")?.GetComponent<Button>();

                if (detailButton != null)
                {
                    Debug.Log(
                        "DetailButton listener registered: " +
                        skill.skillName
                    );

                    detailButton.onClick.AddListener(() =>
                    {
                        Debug.Log(
                            "DetailButton clicked: " +
                            skill.skillName
                        );

                        if (skillDetailUI != null)
                        {
                            skillDetailUI.Show(skill);
                        }
                    });
                }
                else
                {
                    Debug.LogError(
                        "DetailButton not found: " +
                        skill.skillName
                    );
                }
            }
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}