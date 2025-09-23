using MiniJam159.CommandCore;
using MiniJam159.EntityCore;
using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.StructureCore;
using MiniJam159.UnitCore;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace MiniJam159.Commands
{
    [Serializable]
    public class StopCommand : CommandBase
    {
        public StopCommand()
        {
            tooltip = "<b>Stop</b>\nSelected units will stop moving and attack enemies in range";
        }

        public override void execute(Entity instigator)
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

        public override CommandBase clone()
        {
            return new StopCommand { tooltip = this.tooltip };
        }
    }

    [Serializable]
    public class AttackCommand : CommandBase
    {
        public AttackCommand()
        {
            tooltip = "<b>Attack</b>\nAttacks target enemy unit";
        }

        public override void execute(Entity instigator)
        {
            if (PlayerControllerBase.instance.playerMode == PlayerMode.NORMAL) PlayerControllerBase.instance.playerMode = PlayerMode.ATTACK_TARGET;
        }

        public override CommandBase clone()
        {
            return new AttackCommand { tooltip = this.tooltip };
        }
    }

    [Serializable]
    public class OpenBuildMenuCommand : CommandBase
    {
        public OpenBuildMenuCommand()
        {
            tooltip = "<b>Build</b>\nOpens the build menu";
        }

        public override void execute(Entity instigator)
        {
            // First selected unit must be a worker
            if (SelectionManager.instance.selectedObjects.Count == 0) return;

            GameObject selectedObject = SelectionManager.instance.selectedObjects[SelectionManager.instance.getFocusIndex()];
            if (selectedObject == null) return;

            UnitBase unit = selectedObject.GetComponent<UnitBase>();
            if (unit == null) return;

            unit.openBuildMenuCommand();
        }

        public override CommandBase clone()
        {
            return new OpenBuildMenuCommand { tooltip = this.tooltip };
        }
    }

    [Serializable]
    public class CancelBuildMenuCommand : CommandBase
    {
        public CancelBuildMenuCommand()
        {
            tooltip = "<b>Cancel</b>\nCloses the build menu";
        }

        public override void execute(Entity instigator)
        {
            SelectionControllerBase.instance.populateCommands();
        }

        public override CommandBase clone()
        {
            return new CancelBuildMenuCommand { tooltip = this.tooltip };
        }
    }

    [Serializable]
    public class PlaceStructureCommand : CommandBase
    {
        public StructureType structureType;

        public override void execute(Entity instigator)
        {
            if (PlayerControllerBase.instance.playerMode != PlayerMode.NORMAL) return;
            StructureManagerBase.instance.beginPlacement(structureType);
        }

        public override CommandBase clone()
        {
            return new PlaceStructureCommand { tooltip = this.tooltip, structureType = this.structureType };
        }
    }

    [Serializable]
    public class TrainUnitCommand : CommandBase
    {
        [ValueDropdown(nameof(GetUnitOptions))]
        public string unitName;
        private static string[] GetUnitOptions()
        {
            // Odin will only allow selection of the following options for unitName
            return new[]
            {
                "Worker",
                "Warrior",
            };
        }

        public override void execute(Entity instigator)
        {
            // Add info to training queue of instigator from training data dictionary
            instigator.AddToTrainingQueue(EntityManagerBase.instance.trainingDictionary.data[unitName]);
            return;
        }

        public override CommandBase clone()
        {
            return new TrainUnitCommand { tooltip = this.tooltip, unitName = this.unitName };
        }
    }

}
