using System;
using UnityEngine;

using MiniJam159.EntityCore;

namespace MiniJam159.CommandCore
{
    [Serializable]
    public class CommandBase
    {
        public string tooltip = "DEFAULT COMMAND TOOLTIP";

        public virtual void execute(Entity instigator)
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }

        public virtual CommandBase clone()
        {
            return new CommandBase { tooltip = this.tooltip };
        }
    }
}
