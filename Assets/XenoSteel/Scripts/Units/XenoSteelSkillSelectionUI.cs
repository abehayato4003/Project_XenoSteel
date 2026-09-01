using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XenoSteel.Units
{
    public class XenoSteelSkillSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button buttonPrefab;

        private XenoSteel.Combat.XenoSteelAttackAbility _attackAbility;

        public void Show(XenoSteel.Combat.XenoSteelAttackAbility attackAbility)
        {

            Debug.Log("SkillSelectionUI.Show");

            _attackAbility = attackAbility;

            foreach (Transform child in panel.transform)
            {
                Destroy(child.gameObject);
            }

            var unit = attackAbility.UnitReference as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unit == null)
            {
                return;
            }

            var initiative =
                unit.GetComponent<XenoSteel.Core.XenoSteelInitiative>();

            if (initiative == null || initiative.UnitData == null)
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

                var button = Instantiate(buttonPrefab, panel.transform);

                var text = button.GetComponentInChildren<TMP_Text>();

                if (text != null)
                {
                    text.text = skill.skillName;
                }

                button.onClick.AddListener(() =>
                {
                    _attackAbility.SetCurrentSkill(skill);
                });
            }
        }

        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}