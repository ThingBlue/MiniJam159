using MiniJam159.EntityCore;
using MiniJam159.GameCore;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.CommandCore
{
    public class CommandManagerBase : SerializedMonoBehaviour
    {
        [ReadOnly]
        public List<CommandBase> activeCommands = new List<CommandBase>();

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

        public virtual void populateCommands(List<CommandBase> commands)
        {
            // See CommandManager::populateCommands(List<CommandBase> commands)
        }

        public virtual Sprite getCommandSprite(CommandBase command)
        {
            // See CommandManager::getCommandSprite(CommandBase command)
            return null;
        }

    }
}
