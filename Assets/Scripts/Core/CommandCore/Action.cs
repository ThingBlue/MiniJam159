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
        BUILD,
        PLACE_STRUCTURE
    }

    public class Action
    {
        public ActionType actionType;

        public virtual Vector3 getTargetPosition() { return Vector3.zero; }
    }

    public class MoveAction : Action
    {
        public Vector3 targetPosition;

        public MoveAction()
        {
            this.actionType = ActionType.MOVE;
            this.targetPosition = Vector3.zero;
        }
        public MoveAction(Vector3 targetPosition)
        {
            this.actionType = ActionType.MOVE;
            this.targetPosition = targetPosition;
        }

        public override Vector3 getTargetPosition() { return targetPosition; }
    }

    public class AttackAction : Action
    {
        public GameObject targetObject;

        public AttackAction()
        {
            this.actionType = ActionType.ATTACK;
            this.targetObject = null;
        }
        public AttackAction(GameObject targetObject)
        {
            this.actionType = ActionType.ATTACK;
            this.targetObject = targetObject;
        }

        public override Vector3 getTargetPosition() { return targetObject.transform.position; }
    }

    public class AttackMoveAction : Action
    {
        public Vector3 targetPosition;

        public AttackMoveAction()
        {
            this.actionType = ActionType.ATTACK_MOVE;
            this.targetPosition = Vector3.zero;
        }
        public AttackMoveAction(Vector3 targetPosition)
        {
            this.actionType = ActionType.ATTACK_MOVE;
            this.targetPosition = targetPosition;
        }

        public override Vector3 getTargetPosition() { return targetPosition; }
    }

    public class HarvestAction : Action
    {
        public GameObject targetObject;

        public HarvestAction()
        {
            this.actionType = ActionType.HARVEST;
            this.targetObject = null;
        }
        public HarvestAction(GameObject targetResourceObject)
        {
            this.actionType = ActionType.HARVEST;
            this.targetObject = targetResourceObject;
        }

        public override Vector3 getTargetPosition() { return targetObject.transform.position; }
    }

    public class BuildAction : Action
    {
        public GameObject targetObject;

        public BuildAction()
        {
            this.actionType = ActionType.BUILD;
            this.targetObject = null;
        }
        public BuildAction(GameObject targetObject)
        {
            this.actionType = ActionType.BUILD;
            this.targetObject = targetObject;
        }

        public override Vector3 getTargetPosition() { return targetObject.transform.position; }

    }

}
