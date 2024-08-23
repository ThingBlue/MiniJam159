using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.GameCore;

namespace MiniJam159.CommandCore
{
    public class CommandManagerBase : MonoBehaviour
    {
        public List<Command> activeCommands;

        // Singleton
        public static CommandManagerBase instance;

        protected void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        protected void Start()
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
            for (int i = 0; i < 12; i++) activeCommands.Add(null);
        }

        public virtual void populateCommands(List<CommandType> newCommandTypes)
        {
            // See CommandManager::populateCommands(List<CommandType> newCommandTypes)
        }

    }
}
