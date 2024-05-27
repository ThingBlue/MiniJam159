using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MiniJam159.AI;

namespace MiniJam159.UI
{
    public class CommandButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Command command;
        public int commandIndex;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (command == null) return;
            string tooltip = command.tooltip;
            TooltipManager.instance.toggleTooltip(tooltip, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (command == null) return;
            TooltipManager.instance.toggleTooltip("", false);
        }
    }
}
