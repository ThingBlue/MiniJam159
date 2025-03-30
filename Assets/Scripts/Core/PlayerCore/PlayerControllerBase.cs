using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;
using MiniJam159.UnitCore;

namespace MiniJam159.PlayerCore
{
    public enum PlayerMode
    {
        NORMAL = 0,
        MASS_SELECT,
        STRUCTURE_PLACEMENT,
        ATTACK_TARGET
    }

    public class PlayerControllerBase : MonoBehaviour
    {
        public PlayerMode playerMode = PlayerMode.NORMAL;

        // Singleton
        public static PlayerControllerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        #region Command executors

        public virtual void executeStopCommand() { }
        public virtual void executeMoveCommand(Vector3 targetPosition) { }
        public virtual void executeAttackMoveCommand(Vector3 targetPosition) { }
        public virtual void executeAttackCommand(GameObject target) { }
        public virtual void executeHarvestCommand(GameObject target) { }
        public virtual void executeBuildCommand(GameObject target) { }
        public virtual void executeOpenBuildMenuCommand() { }
        public virtual void executeCancelBuildMenuCommand() { }

        #endregion

    }
}
