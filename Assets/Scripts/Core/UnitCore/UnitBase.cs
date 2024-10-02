using UnityEngine;
using System.Collections.Generic;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;
using System.Linq;
using Codice.CM.Common;
using System.IO;

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

        // Worker
        public virtual void harvestCommand(bool addToQueue, GameObject targetObject) { }
        public virtual void buildStructureCommand(bool addToQueue, GameObject targetObject) { }
        public virtual void openBuildMenuCommand() { }

        #endregion

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }

    }

}
