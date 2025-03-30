using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.GameCore;

namespace MiniJam159.CommandCore
{
    public class CommandManagerBase : MonoBehaviour
    {
        public List<Command> activeCommands = new List<Command>();

        // Singleton
        public static CommandManagerBase instance;

        protected void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public virtual void executeCommand(int index)
        {
            // See CommandManager::executeCommand(int index)
        }

        public virtual void clearCommands()
        {
            // See CommandManager::clearCommands()
        }

        public virtual void populateCommands(List<CommandType> newCommandTypes)
        {
            // See CommandManager::populateCommands(List<CommandType> newCommandTypes)
        }

    }
}
