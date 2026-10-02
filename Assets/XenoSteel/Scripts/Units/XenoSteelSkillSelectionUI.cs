using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TurnBasedStrategyFramework.Common.Units;


namespace XenoSteel.Units
{
    public class XenoSteelSkillSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;

        [Header("Skill Slots")]
        [SerializeField] private GameObject[] skillSlots = new GameObject[4];

        private XenoSteel.Combat.XenoSteelAttackAbility _attackAbility;
        private XenoSteel.Combat.SkillData[] _skills;

        private Button[] _iconButtons = new Button[4];
        private Image[] _icons = new Image[4];
        private Button[] _yesButtons = new Button[4];

        private Color[] _normalColors = new Color[4];

        private Transform[] _slotTransforms = new Transform[4];
        private Vector3[] _slotDefaultScales = new Vector3[4];
        private CanvasGroup[] _slotCanvasGroups = new CanvasGroup[4];
        private bool[] _slotHovered = new bool[4];

        private int _selectedSlot = -1;

        private const float SelectedScale = 1.15f;
        private const float HoverScale = 1.08f;
        private const float UnselectedAlpha = 0.45f;

        public void SetCurrentUnit(IUnit unit, bool isPlayerUnit)
        {
            ResetSlotVisuals();

            var unityUnit =
                unit as TurnBasedStrategyFramework.Unity.Units.Unit;

            if (unityUnit == null || !isPlayerUnit)
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

            _attackAbility =
                unityUnit.GetComponent<
                    XenoSteel.Combat.XenoSteelAttackAbility>();

            if (_attackAbility == null)
            {
                Hide();
                return;
            }

            _attackAbility.ClearCurrentSkill();

            _skills = initiative.UnitData.skills;

            SetupSlots();
        }

        private void SetupSlots()
        {
            _selectedSlot = -1;

            for (int i = 0; i < 4; i++)
            {
                _slotHovered[i] = false;

                if (skillSlots[i] != null)
                {
                    skillSlots[i].SetActive(false);
                }
            }

            panel.SetActive(true);

            for (int i = 0; i < 4; i++)
            {
                SetupSlot(i);
            }

            UpdateSlotVisuals();
        }

        private void SetupSlot(int index)
        {
            Debug.Log(
                $"SetupSlot {index + 1}: skill = " +
                (_skills != null && index < _skills.Length && _skills[index] != null
                    ? _skills[index].skillName
                    : "NULL")
            );

            if (skillSlots[index] == null)
            {
                return;
            }

            var slot = skillSlots[index];

            // 前回の状態をリセット
            slot.SetActive(true);

            _slotTransforms[index] = slot.transform;
            _slotDefaultScales[index] = slot.transform.localScale;

            var canvasGroup = slot.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = slot.AddComponent<CanvasGroup>();
            }

            _slotCanvasGroups[index] = canvasGroup;

            // var pointer =
            //     iconButtonTransform.GetComponent<XenoSteelSkillSlotPointer>();

            // if (pointer == null)
            // {
            //     pointer =
            //         iconButtonTransform.gameObject
            //             .AddComponent<XenoSteelSkillSlotPointer>();
            // }

            // pointer.Initialize(this, index);

            // _slotHovered[index] = false;

            var iconButtonTransform =
                slot.transform.Find("IconButton");

            var iconTransform =
                slot.transform.Find("IconButton/Icon");

            var yesButtonTransform =
                slot.transform.Find("YesButton");

            if (iconButtonTransform == null ||
                iconTransform == null ||
                yesButtonTransform == null)
            {
                Debug.LogError(
                    $"SkillSlot {index + 1} の構成が正しくありません。");

                return;
            }

            var pointer =
                iconButtonTransform.GetComponent<XenoSteelSkillSlotPointer>();

            if (pointer == null)
            {
                pointer =
                    iconButtonTransform.gameObject
                        .AddComponent<XenoSteelSkillSlotPointer>();
            }

            pointer.Initialize(this, index);

            _slotHovered[index] = false;

            _iconButtons[index] =
                iconButtonTransform.GetComponent<Button>();

            _icons[index] =
                iconTransform.GetComponent<Image>();

            _yesButtons[index] =
                yesButtonTransform.GetComponent<Button>();

            if (_iconButtons[index] == null ||
                _icons[index] == null ||
                _yesButtons[index] == null)
            {
                Debug.LogError(
                    $"SkillSlot {index + 1} のButton/Imageが見つかりません。");

                return;
            }

            _iconButtons[index].onClick.RemoveAllListeners();
            _yesButtons[index].onClick.RemoveAllListeners();

            int capturedIndex = index;

            _iconButtons[index].onClick.AddListener(
                () => OnSkillButtonClicked(capturedIndex));

            _yesButtons[index].onClick.AddListener(
                () => OnYesButtonClicked(capturedIndex));

            _yesButtons[index].gameObject.SetActive(false);

            var image =
                _iconButtons[index].GetComponent<Image>();

            _normalColors[index] = image.color;

            if (_skills == null ||
                index >= _skills.Length ||
                _skills[index] == null)
            {
                slot.SetActive(false);
                return;
            }

            var skill = _skills[index];

            slot.SetActive(true);

            _icons[index].sprite = skill.icon;
            _icons[index].enabled = skill.icon != null;

            image.color = _normalColors[index];
            _iconButtons[index].interactable = true;

            _slotTransforms[index].localScale =
                _slotDefaultScales[index];

            _slotCanvasGroups[index].alpha = 1f;
        }

        private void OnSkillButtonClicked(int index)
        {
            if (_skills == null ||
                index >= _skills.Length ||
                _skills[index] == null ||
                _attackAbility == null)
            {
                return;
            }

            var skill = _skills[index];

            // 同じスキルを再クリック → 選択解除
            if (_attackAbility.CurrentSkill == skill)
            {
                _attackAbility.ClearCurrentSkill();
                ResetSlotVisuals();
                return;
            }

            // 別のスキルをクリック → そのスキルへ切り替え
            _attackAbility.SetCurrentSkill(skill);
            SetSelectedSlot(index);

            if (skill.targetType == XenoSteel.Combat.SkillTargetType.None)
            {
                _yesButtons[index].gameObject.SetActive(true);
            }
        }

        private void OnYesButtonClicked(int index)
        {
            if (_skills == null ||
                index >= _skills.Length ||
                _skills[index] == null ||
                _attackAbility == null)
            {
                return;
            }

            if (_attackAbility.CurrentSkill != _skills[index])
            {
                return;
            }

            _yesButtons[index].gameObject.SetActive(false);

            _attackAbility.ConfirmCurrentSkill();

            ResetSlotVisuals();
        }

        private void SetSelectedSlot(int selectedIndex)
        {
            _selectedSlot = selectedIndex;

            UpdateSlotVisuals();

            for (int i = 0; i < 4; i++)
            {
                if (_yesButtons[i] == null)
                {
                    continue;
                }

                if (i != selectedIndex)
                {
                    _yesButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void ResetSlotVisuals()
        {
            _selectedSlot = -1;

            for (int i = 0; i < 4; i++)
            {
                _slotHovered[i] = false;

                if (_slotTransforms[i] != null)
                {
                    _slotTransforms[i].localScale =
                        _slotDefaultScales[i];
                }

                if (_slotCanvasGroups[i] != null)
                {
                    _slotCanvasGroups[i].alpha = 1f;
                }

                if (_yesButtons[i] != null)
                {
                    _yesButtons[i].gameObject.SetActive(false);
                }

                if (_iconButtons[i] != null)
                {
                    _iconButtons[i].interactable = true;
                }
            }
        }

        public void Hide()
        {
            panel.SetActive(false);
        }

        public void OnSlotPointerEnter(int index)
        {
            if (index < 0 || index >= 4)
            {
                return;
            }

            _slotHovered[index] = true;
            UpdateSlotVisuals();
        }

        public void OnSlotPointerExit(int index)
        {
            if (index < 0 || index >= 4)
            {
                return;
            }

            _slotHovered[index] = false;
            UpdateSlotVisuals();
        }

        private void UpdateSlotVisuals()
        {
            for (int i = 0; i < 4; i++)
            {
                if (_slotTransforms[i] == null ||
                    _slotCanvasGroups[i] == null)
                {
                    continue;
                }

                bool selected = i == _selectedSlot;

                // スキル未選択
                if (_selectedSlot == -1)
                {
                    _slotCanvasGroups[i].alpha = 1f;

                    // カーソルが乗っているスロットだけ拡大
                    if (_slotHovered[i])
                    {
                        _slotTransforms[i].localScale =
                            _slotDefaultScales[i] * HoverScale;
                    }
                    else
                    {
                        _slotTransforms[i].localScale =
                            _slotDefaultScales[i];
                    }

                    continue;
                }

                // 選択中のスロット
                if (selected)
                {
                    _slotCanvasGroups[i].alpha = 1f;

                    _slotTransforms[i].localScale =
                        _slotDefaultScales[i] * SelectedScale;
                }
                else
                {
                    // 選択されていないスロットは暗くする
                    _slotCanvasGroups[i].alpha =
                        UnselectedAlpha;

                    // カーソルを合わせた場合だけ少し拡大
                    if (_slotHovered[i])
                    {
                        _slotTransforms[i].localScale =
                            _slotDefaultScales[i] * HoverScale;
                    }
                    else
                    {
                        _slotTransforms[i].localScale =
                            _slotDefaultScales[i];
                    }
                }
            }
        }
    }
}