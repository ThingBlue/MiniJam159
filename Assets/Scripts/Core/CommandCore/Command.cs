using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.CommandCore
{
    public enum CommandType
    {
        NULL = 0,
        MOVE,
        ATTACK,
        HOLD,
        HARVEST,
        OPEN_BUILD_MENU,
        CANCEL_BUILD_MENU,

        BUILD_NEST,
        BUILD_WOMB
    }

    public class Command
    {
        public CommandType commandType;
        public string tooltip = "DEFAULT COMMAND TOOLTIP";

        public virtual void initialize() { }

        public virtual void execute()
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }
    }
}
