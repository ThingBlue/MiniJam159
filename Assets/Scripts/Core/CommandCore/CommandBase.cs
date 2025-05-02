using System;
using UnityEngine;

namespace MiniJam159.CommandCore
{
    [Serializable]
    public class CommandBase
    {
        public string tooltip = "DEFAULT COMMAND TOOLTIP";

        public virtual void execute()
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }

        public virtual CommandBase clone()
        {
            return new CommandBase { tooltip = this.tooltip };
        }
    }
}
