using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;
using MiniJam159.PlayerCore;
using MiniJam159.Structures;
using MiniJam159.UnitCore;

namespace MiniJam159.Commands
{
    public class StopCommand : Command
    {
        public StopCommand()
        {
            tooltip = "<b>Stop</b>\nSelected units will stop moving and attack enemies in range";
        }

        public override void execute()
        {
            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a unit component
                UnitBase unit = selectedObject.GetComponent<UnitBase>();
                if (unit == null) continue;

                unit.stopCommand();
            }

            // Finish command
            PlayerControllerBase.instance.playerMode = PlayerMode.NORMAL;
        }
    }

    public class AttackCommand : Command
    {
        public AttackCommand()
        {
            tooltip = "<b>Attack</b>\nAttacks target enemy unit";
        }

        public override void execute()
        {
            if (PlayerControllerBase.instance.playerMode == PlayerMode.NORMAL) PlayerControllerBase.instance.playerMode = PlayerMode.ATTACK_TARGET;
        }
    }

    public class OpenBuildMenuCommand : Command
    {
        public OpenBuildMenuCommand()
        {
            tooltip = "<b>Build</b>\nOpens the build menu";
        }

        public override void execute()
        {
            // First selected unit must be a worker
            if (SelectionManager.instance.selectedObjects.Count == 0) return;

            GameObject selectedObject = SelectionManager.instance.selectedObjects[SelectionManager.instance.getFocusIndex()];
            if (selectedObject == null) return;

            UnitBase unit = selectedObject.GetComponent<UnitBase>();
            if (unit == null) return;

            unit.openBuildMenuCommand();
        }
    }

    public class CancelBuildMenuCommand : Command
    {
        public CancelBuildMenuCommand()
        {
            tooltip = "<b>Cancel</b>\nCloses the build menu";
        }

        public override void execute()
        {
            SelectionControllerBase.instance.populateCommands();
        }
    }

    public class BuildNestCommand : Command
    {
        public BuildNestCommand()
        {
            tooltip = "<b>Nest</b>\nThe core of the colony.";
        }

        public override void execute()
        {
            if (PlayerControllerBase.instance.playerMode != PlayerMode.NORMAL) return;

            EventManager.instance.buildNestCommandEvent.Invoke();
        }
    }

    public class BuildWombCommand : Command
    {
        public BuildWombCommand()
        {
            tooltip = "<b>Womb</b>\nCreates basic melee units.";
        }

        public override void execute()
        {
            if (PlayerControllerBase.instance.playerMode != PlayerMode.NORMAL) return;

            EventManager.instance.buildWombCommandEvent.Invoke();
        }
    }

}
