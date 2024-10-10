using UnityEngine;
using System.Collections.Generic;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;
using MiniJam159.Common;

namespace MiniJam159.UnitCore
{
    public abstract class UnitBase : Entity
    {
        #region Inspector members

        public List<CommandType> commands = new List<CommandType>();

        #endregion

        // Actions
        public Queue<Action> actionQueue = new Queue<Action>();

        #region Command handlers

        // Unit
        public virtual void stopCommand() { }
        public virtual void moveCommand(bool addToQueue, Vector3 targetPosition) { }
        public virtual void attackCommand(bool addToQueue, GameObject targetObject) { }
        public virtual void attackMoveCommand(bool addToQueue, Vector3 targetPosition) { }
        public virtual void interactCommand(bool addToQueue, GameObject targetObject) { }

        // Worker
        public virtual void harvestCommand(bool addToQueue, GameObject targetObject) { }
        public virtual void buildCommand(bool addToQueue, GameObject targetObject) { }
        public virtual void openBuildMenuCommand() { }

        #endregion

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }

    }

}
