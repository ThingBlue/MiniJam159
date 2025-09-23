using MiniJam159.CommandCore;
using MiniJam159.EntityCore;
using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.StructureCore;
using MiniJam159.UICore;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiniJam159.Commands
{
    public class CommandManager : CommandManagerBase
    {
        #region Inspector members

        public Sprite attackCommandSprite;
        public Sprite stopCommandSprite;
        public Sprite openBuildMenuCommandSprite;
        public Sprite cancelBuildMenuCommandSprite;

        #endregion

        public override void executeCommand(int index)
        {
            if (activeCommands[index] == null) return;

            // Set command instigator to current focused entity
            int focusIndex = SelectionManager.instance.getFocusIndex();
            if (focusIndex == -1) return;
            Entity instigator = SelectionManager.instance.selectedObjects[focusIndex].GetComponent<Entity>();
            if (!instigator) return;

            // Execute command
            Debug.Log("Executing command: " + activeCommands[index] + ", with instigator: " + instigator.gameObject.name);
            activeCommands[index].execute(instigator);
        }

        public override void clearCommands()
        {
            activeCommands.Clear();
            for (int i = 0; i < 12; i++) activeCommands.Add(null);
        }

        public override void populateCommands(List<CommandBase> commands)
        {
            activeCommands.Clear();

            // Clear UI
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Create new ui by deep copying commands list, preserving nulls
            activeCommands = commands.Select(item => item != null ? item.clone() : null).ToList();

            // Update UI
            CommandPanelManagerBase.instance.populateCommandButtons();
        }

        public override Sprite getCommandSprite(CommandBase command)
        {
            if (command is AttackCommand) return attackCommandSprite;
            if (command is StopCommand) return stopCommandSprite;
            if (command is OpenBuildMenuCommand) return openBuildMenuCommandSprite;
            if (command is CancelBuildMenuCommand) return cancelBuildMenuCommandSprite;
            if (command is PlaceStructureCommand)
            {
                // Return sprite based on structure sprite
                PlaceStructureCommand placeStructureCommand = command as PlaceStructureCommand;
                return StructureManagerBase.instance.getStructureSprite(placeStructureCommand.structureType);
            }
            Debug.LogError("Forgot to add a command sprite for command: " + command.GetType().Name);
            return null;
        }

    }
}
