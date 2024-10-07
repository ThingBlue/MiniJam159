using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.CommandCore
{
    public enum ActionType
    {
        // Common jobs
        IDLE = 0,
        MOVE,
        ATTACK,
        ATTACK_MOVE,

        // Worker jobs
        HARVEST,
        BUILD
    }

    public class Action
    {
        public ActionType actionType;

        public virtual Vector3 getTargetPosition() { return Vector3.zero; }
    }

}
