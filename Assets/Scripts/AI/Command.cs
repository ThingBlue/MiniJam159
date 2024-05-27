using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MiniJam159.GameCore;

namespace MiniJam159.AI
{
    public enum CommandType
    {
        NULL = 0,
        MOVE,
        ATTACK,
        HOLD,
        BUILD,
        HARVEST
    }

    public class Command
    {
        public CommandType commandType;
        public string tooltip = "DEFAULT COMMAND TOOLTIP";

        public virtual void initialize() { }

        public virtual void execute()
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }
    }
    public class MoveCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Move</b>\nMoves the selected units to target location";
        }

        public override void execute()
        {
            if (PlayerModeManager.instance.playerMode == PlayerMode.NORMAL) PlayerModeManager.instance.playerMode = PlayerMode.MOVE_TARGET;
        }
    }
    public class AttackCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Attack</b>\nAttacks target enemy unit";
        }

        public override void execute()
        {
            if (PlayerModeManager.instance.playerMode == PlayerMode.NORMAL) PlayerModeManager.instance.playerMode = PlayerMode.ATTACK_TARGET;
        }
    }
    public class HoldCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Hold</b>\nSelected units will stop moving and attack enemies in range";
        }

        public override void execute()
        {
            if (PlayerModeManager.instance.playerMode == PlayerMode.NORMAL) PlayerModeManager.instance.playerMode = PlayerMode.HOLD_COMMAND;
        }
    }
    public class BuildCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Build</b>\nOpens the build menu";
        }

        public override void execute()
        {
            // Open build menu
        }
    }
    public class HarvestCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Harvest</b>\nHarvests the target resource";
        }

        public override void execute()
        {
            if (PlayerModeManager.instance.playerMode == PlayerMode.NORMAL) PlayerModeManager.instance.playerMode = PlayerMode.HARVEST_TARGET;
        }
    }
}
