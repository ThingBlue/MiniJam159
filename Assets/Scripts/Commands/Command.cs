using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MiniJam159.GameCore;

namespace MiniJam159.Commands
{
    public enum CommandType
    {
        NULL = 0,
        MOVE,
        ATTACK,
        HOLD,
        HARVEST,
        OPEN_BUILD_MENU,
        CANCEL_BUILD_MENU,

        BUILD_NEST,
        BUILD_WOMB
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

    #region General commands

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
            EventManager.instance.holdCommandEvent.Invoke();
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
    public class OpenBuildMenuCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Build</b>\nOpens the build menu";
        }

        public override void execute()
        {
            EventManager.instance.openBuildMenuCommandEvent.Invoke();
        }
    }
    public class CancelBuildMenuCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Cancel</b>\nCloses the build menu";
        }

        public override void execute()
        {
            EventManager.instance.cancelBuildMenuCommandEvent.Invoke();
        }
    }

    #endregion

    #region Building specific commands

    public class BuildNestCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Nest</b>\nThe core of the colony.";
        }

        public override void execute()
        {
            if (PlayerModeManager.instance.playerMode != PlayerMode.NORMAL) return;
        }
    }
    public class BuildWombCommand : Command
    {
        public override void initialize()
        {
            tooltip = "<b>Womb</b>\nBreeds basic offensive units.";
        }

        public override void execute()
        {
            // Open build menu
            if (PlayerModeManager.instance.playerMode != PlayerMode.NORMAL) return;
        }
    }

    #endregion
}
