using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.UnitCore
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
        public ActionType type;
    }

    public class MoveAction : Action
    {
        public Vector3 targetPosition;

        public MoveAction()
        {
            this.type = ActionType.MOVE;
            this.targetPosition = Vector3.zero;
        }
        public MoveAction(Vector3 targetPosition)
        {
            this.type = ActionType.MOVE;
            this.targetPosition = targetPosition;
        }
    }

    public class AttackAction : Action
    {
        public GameObject targetObject;

        public AttackAction()
        {
            this.type = ActionType.ATTACK;
            this.targetObject = null;
        }
        public AttackAction(GameObject targetObject)
        {
            this.type = ActionType.ATTACK;
            this.targetObject = targetObject;
        }
    }

    public class AttackMoveAction : Action
    {
        public Vector3 targetPosition;

        public AttackMoveAction()
        {
            this.type = ActionType.ATTACK_MOVE;
            this.targetPosition = Vector3.zero;
        }
        public AttackMoveAction(Vector3 targetPosition)
        {
            this.type = ActionType.ATTACK_MOVE;
            this.targetPosition = targetPosition;
        }
    }

    public class HarvestAction : Action
    {
        public GameObject targetObject;

        public HarvestAction()
        {
            this.type = ActionType.HARVEST;
            this.targetObject = null;
        }
        public HarvestAction(GameObject targetResourceObject)
        {
            this.type = ActionType.HARVEST;
            this.targetObject = targetResourceObject;
        }
    }

    public class BuildAction : Action
    {
        public GameObject targetStructureObject;

        public BuildAction()
        {
            this.type = ActionType.BUILD;
            this.targetStructureObject = null;
        }
        public BuildAction(GameObject targetStructureObject)
        {
            this.type = ActionType.BUILD;
            this.targetStructureObject = targetStructureObject;
        }

    }

}
