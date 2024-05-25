using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniJam159.UI
{
    public enum CommandType
    {
        NULL = 0,
        MOVE,
        ATTACK,
        HOLD,
        BUILD
    }

    public class Command : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string tooltip;

        public virtual void execute()
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("entered");
            TooltipManager.instance.toggleTooltip(tooltip, true);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("exited");
            TooltipManager.instance.toggleTooltip("", false);
        }
    }
    public class MoveCommand : Command
    {
        private void Start()
        {
            tooltip = "<b>Move</b>\nMoves the selected units to target location";
        }

        public override void execute()
        {

        }
    }
    public class AttackCommand : Command
    {
        private void Start()
        {
            tooltip = "<b>Attack</b>\nAttacks target enemy unit";
        }

        public override void execute()
        {

        }
    }
    public class HoldCommand : Command
    {
        private void Start()
        {
            tooltip = "<b>Hold</b>\nSelected units will stop moving and attack enemies in range";
        }

        public override void execute()
        {

        }
    }
    public class BuildCommand : Command
    {
        private void Start()
        {
            tooltip = "<b>Build</b>\nOpens the build menu";
        }

        public override void execute()
        {

        }
    }
}
