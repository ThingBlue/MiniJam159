using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniJam159.UI
{
    public class SelectedDisplayButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string selectedObjectName;
        public int selectedIndex;

        public void OnPointerEnter(PointerEventData eventData)
        {
            //TooltipManager.instance.toggleTooltip(selectedObjectName, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            //TooltipManager.instance.toggleTooltip("", false);
        }
    }
}
