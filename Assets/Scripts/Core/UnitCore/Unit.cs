using UnityEngine;
using System.Collections.Generic;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;

namespace MiniJam159.UnitCore
{
    public abstract class Unit : Entity
    {
        #region Inspector members

        public HealthBar healthBar;
        public List<CommandType> commands = new List<CommandType>();

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

        public Queue<Action> actionQueue = new Queue<Action>();
        public Action currentAction = null;

        public Queue<Vector3> path = new Queue<Vector3>();
        protected float pathUpdateTimer;

        protected float attackTimer = 0f;

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

            // Subscribe to events
            EventManager.instance.stopCommandEvent.AddListener(onStopCommandCallback);
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
            // Handle actions
            if (actionQueue.Count > 0) handleActions();

            // Set y position and remove velocity
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Set mesh position
            transform.Find("Mesh").position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }

        #region Action handling

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

        protected virtual void endAction()
        {
            // Remove self from action indicator list
            ActionIndicatorManagerBase.instance.completeAction(this, actionQueue.Peek());

            // Pop current action
            actionQueue.Dequeue();
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
                transform.position = moveTowardsDestination;
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
                    transform.position = moveTowardsDestination;
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

        protected abstract GameObject FindNearestTarget();

        protected virtual void clearActionQueue()
        {
            // Remove all actions from queue
            while (actionQueue.Count > 0)
            {
                // Update action indicators
                Action action = actionQueue.Dequeue();
                ActionIndicatorManagerBase.instance.completeAction(this, action);
            }
        }

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }

        private void OnDrawGizmos()
        {
            if (path.Count > 0)
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
