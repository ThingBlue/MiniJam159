using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.GameCore;

namespace MiniJam159.AI
{
    public class CommandManager : MonoBehaviour
    {
        public List<Command> activeCommands;

        // Singleton
        public static CommandManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            activeCommands = new List<Command>();
        }

        public void executeCommand(int index)
        {
            Debug.Log("Executing command: " + activeCommands[index]);
            if (activeCommands[index] == null) return;

            activeCommands[index].execute();
        }

        public void clearCommands()
        {
            activeCommands.Clear();
        }

        public void populateCommands(List<CommandType> newCommandTypes)
        {
            activeCommands.Clear();

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
                    case CommandType.MOVE:
                        newCommand = new MoveCommand();
                        break;
                    case CommandType.ATTACK:
                        newCommand = new AttackCommand();
                        break;
                    case CommandType.HOLD:
                        newCommand = new HoldCommand();
                        break;
                    case CommandType.BUILD:
                        newCommand = new BuildCommand();
                        break;
                    case CommandType.HARVEST:
                        newCommand = new HarvestCommand();
                        break;
                }
                newCommand.initialize();
                newCommand.commandType = newCommandTypes[i];
                activeCommands.Add(newCommand);
            }
        }
    }
}
