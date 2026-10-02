using UnityEngine;
using UnityEngine.EventSystems;

namespace XenoSteel.Units
{
    public class XenoSteelSkillSlotPointer :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private XenoSteelSkillSelectionUI _owner;
        private int _index;

        public void Initialize(
            XenoSteelSkillSelectionUI owner,
            int index
        )
        {
            _owner = owner;
            _index = index;
        }

        public void OnPointerEnter(
            PointerEventData eventData
        )
        {
            if (_owner != null)
            {
                _owner.OnSlotPointerEnter(_index);
            }
        }

        public void OnPointerExit(
            PointerEventData eventData
        )
        {
            if (_owner != null)
            {
                _owner.OnSlotPointerExit(_index);
            }
        }
    }
}