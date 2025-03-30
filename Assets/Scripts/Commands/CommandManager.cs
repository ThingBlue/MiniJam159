using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.GameCore;
using MiniJam159.CommandCore;
using MiniJam159.UICore;

namespace MiniJam159.Commands
{
    public class CommandManager : CommandManagerBase
    {
        public override void executeCommand(int index)
        {
            Debug.Log("Executing command: " + activeCommands[index]);
            if (activeCommands[index] == null) return;

            activeCommands[index].execute();
        }

        public override void clearCommands()
        {
            activeCommands.Clear();
            for (int i = 0; i < 12; i++) activeCommands.Add(null);
        }

        public override void populateCommands(List<CommandType> newCommandTypes)
        {
            activeCommands.Clear();

            // Clear UI
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Create new ui
            for (int i = 0; i < newCommandTypes.Count; i++)
            {
                // Skip null commands
                if (newCommandTypes[i] == CommandType.NULL)
                {
                    // Add null command to command manager
                    activeCommands.Add(null);
                    continue;
                }

                Command newCommand = null;

                // Attach command script and texture to new button
                switch (newCommandTypes[i])
                {
                    case CommandType.STOP:
                        newCommand = new StopCommand();
                        break;

                    case CommandType.OPEN_BUILD_MENU:
                        newCommand = new OpenBuildMenuCommand();
                        break;
                    case CommandType.CANCEL_BUILD_MENU:
                        newCommand = new CancelBuildMenuCommand();
                        break;

                    case CommandType.BUILD_NEST:
                        newCommand = new BuildNestCommand();
                        break;
                    case CommandType.BUILD_WOMB:
                        newCommand = new BuildWombCommand();
                        break;
                }
                newCommand.commandType = newCommandTypes[i];
                activeCommands.Add(newCommand);
            }

            // Update UI
            CommandPanelManagerBase.instance.populateCommandButtons();
        }

    }
}
