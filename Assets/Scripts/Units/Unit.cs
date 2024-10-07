using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.UnitCore;
using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using System.Linq;
using MiniJam159.StructureCore;
using TMPro;

namespace MiniJam159.Units
{
    public class Unit : UnitBase
    {
        #region Inspector members

        public HealthBar healthBar;

        public float moveSpeed;
        public float pathUpdateInterval;
        public float pathfindingRadius;

        public string targetTag = "Enemy"; // Tag to identify targets

        public int attackDamage = 10; // Damage dealt by each attack
        public float attackCooldown = 1.0f; // Time between attacks
        public float detectionRadius = 5.0f; // Radius to detect the nearest target
        public float attackRange = 2.0f; // Range within which the AI will attack

        public float surroundDistance = 1.5f; // Distance to maintain around the target when surrounding
        public float coordinationRadius = 5.0f; // Radius within which AIs coordinate their actions

        #endregion

        public float health;
        protected float attackTimer = 0f;

        // Pathfinding
        public Queue<Vector3> path = new Queue<Vector3>();
        protected float pathUpdateTimer;

        // Collisions
        public List<Collider> collisions = new List<Collider>();

        protected Vector3 movement = Vector3.zero;

        protected virtual void Start()
        {
            // TEMP
            EntityManager.instance.playerUnitObjects.Add(gameObject);
            EntityManager.instance.playerEntityObjects.Add(gameObject);

            // Start at max health
            health = maxHealth;

            // Set health bar values
            healthBar.setMaxHealth(maxHealth);
            healthBar.setHealth(health);
        }

        protected virtual void OnDestroy()
        {
            EntityManager.instance.playerUnitObjects.Remove(gameObject);
        }

        protected virtual void Update()
        {
            // Increment timers
            pathUpdateTimer += Time.deltaTime;
            attackTimer += Time.deltaTime;
        }

        protected virtual void FixedUpdate()
        {
            // Handle actions (and movement)
            if (actionQueue.Count > 0) handleActions();

            // Handle collisions
            handleCollisions();

            // Do movement and reset
            transform.position += movement;
            movement = Vector3.zero;

            // Set y position and remove velocity
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Set mesh position
            transform.Find("Mesh").position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }

        protected virtual bool handlePathfinding(Vector3 targetPosition)
        {
            // Check if current path is still valid
            if (pathUpdateTimer > pathUpdateInterval)
            {
                path = GridManagerBase.instance.getPathQueue(transform.position, targetPosition, pathfindingRadius);
                pathUpdateTimer = 0f;
            }

            // Return true if path ended
            if (path.Count == 0) return true;

            // Stop moving to waypoint if reached
            if (Vector3.Distance(transform.position, path.Peek()) <= 0.1f)
            {
                // Pop current waypoint
                path.Dequeue();
            }
            else
            {
                // Move towards current waypoint
                Vector3 moveTowardsDestination = Vector3.MoveTowards(transform.position, path.Peek(), moveSpeed * Time.fixedDeltaTime);
                movement += moveTowardsDestination - transform.position;
            }

            // Return true if path ended
            // Return false while path is still ongoing
            return (path.Count == 0);
        }

        #region Action handlers

        protected virtual void handleActions()
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

        protected virtual void handleMoveAction(MoveAction action)
        {
            bool movementResult = handlePathfinding(action.targetPosition);

            // Stop action if path ended
            if (movementResult) endAction();
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
                    path = GridManagerBase.instance.getPathQueue(transform.position, action.targetObject.transform.position, pathfindingRadius);
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
            ActionIndicatorManagerBase.instance.completeAction(completedAction, this);
        }

        protected virtual void clearActionQueue()
        {
            // Remove all actions from queue
            while (actionQueue.Count > 0)
            {
                // Update action indicators
                Action action = actionQueue.Dequeue();
                ActionIndicatorManagerBase.instance.completeAction(action, this);
            }
        }

        #endregion

        #region Command handlers

        public override void stopCommand()
        {
            // Remove all actions from queue
            clearActionQueue();
        }

        public override void moveCommand(bool addToQueue, Vector3 targetPosition)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Check if target position is occupied
            if (GridManagerBase.instance.isTileOccupied(GridManagerBase.instance.getTileFromPosition(targetPosition)))
            {
                // Find closest free position to move to
                targetPosition = GridManagerBase.instance.getClosestFreeTilePosition(targetPosition, transform.position);
            }

            // Check if target position is accessible

            // Enqueue new action
            Action newAction = new MoveAction(targetPosition);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(newAction, this);
        }

        public override void attackCommand(bool addToQueue, GameObject targetObject)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new AttackAction(targetObject);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(newAction, this);
        }

        public override void attackMoveCommand(bool addToQueue, Vector3 targetPosition)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new AttackMoveAction(targetPosition);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(newAction, this);
        }

        #endregion

        protected virtual void handleCollisions()
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

        protected virtual void OnTriggerEnter(Collider other)
        {
            collisions.Add(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            collisions.Remove(other);
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
