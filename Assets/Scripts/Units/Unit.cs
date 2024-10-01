using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.UnitCore;
using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using System.Linq;
using MiniJam159.StructureCore;

namespace MiniJam159.Units
{
    public class Unit : UnitBase
    {
        protected override void Start()
        {
            // Subscribe to events
            EventManager.instance.stopCommandEvent.AddListener(onStopCommandCallback);

            base.Start();
        }

        protected virtual void Update()
        {
            // Increment timers
            pathUpdateTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;
        }

        protected override void FixedUpdate()
        {
            // Handle actions (and movement)
            if (actionQueue.Count > 0) handleActions();

            // Handle collisions
            handleCollisions();

            // Do movement and reset
            transform.position += movement;
            movement = Vector3.zero;

            base.FixedUpdate();
        }

        #region Action handling

        protected override void handleActions()
        {
            if (actionQueue.Count == 0) return;

            // Handle current action
            Action currentAction = actionQueue.Peek();
            switch (currentAction.actionType)
            {
                case ActionType.MOVE:
                    handleMoveAction(currentAction as MoveAction);
                    break;
                case ActionType.ATTACK:
                    handleAttackAction(currentAction as AttackAction);
                    break;
                case ActionType.ATTACK_MOVE:
                    handleAttackMoveAction(currentAction as AttackMoveAction);
                    break;
                case ActionType.IDLE:
                    break;
            }

            // We remove actions from the queue after completing them
        }

        public virtual int getActionIndex(Action action)
        {
            for (int i = 0; i < actionQueue.Count; i++)
            {
                if (actionQueue.ElementAt(i) == action) return i;
            }
            return -1;
        }

        protected virtual void endAction()
        {
            // Pop current action
            Action completedAction = actionQueue.Dequeue();

            // Remove self from action indicator list
            ActionIndicatorManagerBase.instance.completeAction(this, completedAction);
        }

        protected virtual void handleMoveAction(MoveAction action)
        {
            // Check if current path is still valid
            if (pathUpdateTimer > pathUpdateInterval)
            {
                path = GridManager.instance.getPathQueue(transform.position, action.targetPosition, pathfindingRadius);
                pathUpdateTimer = 0f;
            }

            // Stop action if path ended
            if (path.Count == 0)
            {
                endAction();
                return;
            }

            // Stop moving to position if reached
            if (Vector3.Distance(transform.position, path.Peek()) <= 0.1f)
            {
                // Pop current waypoint
                path.Dequeue();
            }
            else
            {
                Vector3 moveTowardsDestination = Vector3.MoveTowards(transform.position, path.Peek(), moveSpeed * Time.fixedDeltaTime);
                movement += moveTowardsDestination - transform.position;
                //transform.position = moveTowardsDestination;
            }
        }

        protected virtual void handleAttackAction(AttackAction action)
        {
            // Target within attack range, can attack
            if (Vector3.Distance(transform.position, action.targetObject.transform.position) <= attackRange)
            {
                if (attackTimer >= attackCooldown)
                {
                    // Implement health reduction on the target here
                    Debug.Log("Attacking target");

                    // Reset attack timer
                    attackTimer = 0;
                }
            }
            // Target outside attack range, move towards target
            else
            {
                // Calculate path
                if (pathUpdateTimer > pathUpdateInterval || path.Count == 0)
                {
                    path = GridManager.instance.getPathQueue(transform.position, action.targetObject.transform.position, pathfindingRadius);
                    pathUpdateTimer = 0f;
                }

                if (Vector3.Distance(transform.position, path.Peek()) <= 0.5f)
                {
                    // Pop current waypoint
                    path.Dequeue();
                }
                else
                {
                    // Move towards current waypoint
                    Vector3 moveTowardsDestination = Vector3.MoveTowards(transform.position, path.Peek(), moveSpeed * Time.deltaTime);
                    movement += moveTowardsDestination - transform.position;
                    //transform.position = moveTowardsDestination;
                }
            }
        }

        protected virtual void handleAttackMoveAction(AttackMoveAction action)
        {

        }

        #endregion

        #region Command handling

        public virtual void moveCommand(bool addToQueue, Vector3 targetPosition)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new MoveAction(targetPosition);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(this, newAction);
        }

        public virtual void attackCommand(bool addToQueue, GameObject targetObject)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new AttackAction(targetObject);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(this, newAction);
        }

        public virtual void attackMoveCommand(bool addToQueue, Vector3 targetPosition)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new AttackMoveAction(targetPosition);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(this, newAction);
        }

        protected virtual void onStopCommandCallback()
        {
            // Remove all actions from queue
            clearActionQueue();
        }

        #endregion

        protected override void handleCollisions()
        {
            List<Collider> structureColliders = new List<Collider>();
            foreach (Collider collider in collisions)
            {
                UnitBase unit = collider.GetComponent<UnitBase>();
                Structure structure = collider.GetComponent<Structure>();

                // Save structure colliders for processing after everything else
                if (structure != null)
                {
                    structureColliders.Add(collider);
                    continue;
                }

                if (unit != null)
                {
                    // Don't apply forces from other idle units if self is not idle
                    if (actionQueue.Count != 0 && unit.actionQueue.Count == 0)
                    {
                        continue;
                    }

                    // Collision with soft collider, add force to self
                    CapsuleCollider selfCapsuleCollider = GetComponent<CapsuleCollider>();
                    CapsuleCollider otherCapsuleCollider = collider as CapsuleCollider;
                    if (selfCapsuleCollider == null || otherCapsuleCollider == null)
                    {
                        Debug.LogError("Collision between non-capsule colliders");
                        continue;
                    }

                    // Distance to push = Desired distance (Sum of radii) - Actual distance between midpoints
                    float radiiSum = selfCapsuleCollider.radius + otherCapsuleCollider.radius;
                    float midpointDistance = Vector3.Distance(transform.position, collider.transform.position);
                    float distance = radiiSum - midpointDistance;

                    // Calculate force to apply to self
                    Vector3 direction = (transform.position - collider.transform.position).normalized;
                    movement += direction * distance * 0.1f;
                }
            }
            /*
            // Process structure colliders
            foreach (Collider collider in structureColliders)
            {
                // Collision with hard collider, immediately push self out
                CapsuleCollider selfCapsuleCollider = GetComponent<CapsuleCollider>();
                CapsuleCollider otherCapsuleCollider = collider as CapsuleCollider;
                if (selfCapsuleCollider == null || otherCapsuleCollider == null)
                {
                    Debug.LogError("Collision between non-capsule colliders");
                    continue;
                }

                // Distance to push = Desired distance (Sum of radii) - Actual distance between midpoints
                float radiiSum = selfCapsuleCollider.radius + otherCapsuleCollider.radius;
                float midpointDistance = Vector3.Distance(transform.position, collider.transform.position);
                float distance = radiiSum - midpointDistance;

                // Calculate force to apply to self
                Vector3 direction = (transform.position - collider.transform.position).normalized;
                movement += direction * distance * 0.5f;
            }
            */
        }

        private void OnDrawGizmos()
        {
            if (path.Count > 0 && false)
            {
                Queue<Vector3> debugPath = new Queue<Vector3>(path);

                Gizmos.color = Color.red;
                Vector3 previousPosition = transform.position;
                while (debugPath.Count > 0)
                {
                    Vector3 targetPosition = debugPath.Dequeue();
                    Gizmos.DrawLine(previousPosition, targetPosition);
                    previousPosition = targetPosition;
                }
            }
        }

    }
}
