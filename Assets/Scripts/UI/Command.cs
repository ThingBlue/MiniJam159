using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.UI
{
    public enum CommandType
    {
        NULL = 0,
        MOVE,
        ATTACK,
        HOLD,
        BUILD
    }

    public class Command : MonoBehaviour
    {
        #region Inspector members

        #endregion

        public virtual void execute()
        {
            Debug.LogWarning("Attempted to execute a null command!");
        }
    }
    public class MoveCommand : Command
    {
        public override void execute()
        {

        }
    }
    public class AttackCommand : Command
    {
        public override void execute()
        {

        }
    }
    public class HoldCommand : Command
    {
        public override void execute()
        {

        }
    }
    public class BuildCommand : Command
    {
        public override void execute()
        {

        }
    }
}
