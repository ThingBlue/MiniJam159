using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using UnityEngine.UIElements;
using System.IO;
using UnityEditor;

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

        #endregion

        public float health;

        public Queue<Action> actionQueue = new Queue<Action>();
        public Action currentAction = null;

        public Queue<Vector3> path = new Queue<Vector3>();
        protected float pathUpdateTimer;

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
            pathUpdateTimer += Time.deltaTime;
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
            switch (currentAction.type)
            {
                case ActionType.MOVE:
                    handleMoveAction(currentAction as MoveAction);
                    break;
                case ActionType.IDLE:
                    break;
            }

            // We remove actions from the queue after completing them
        }

        protected virtual void endAction()
        {
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

        #endregion

        #region Command handling

        public virtual void moveCommand(bool addToQueue, Vector3 targetPosition)
        {
            if (!addToQueue) actionQueue.Clear();
            actionQueue.Enqueue(new MoveAction(targetPosition));
        }

        protected virtual void onStopCommandCallback()
        {
            // Remove all actions
            actionQueue.Clear();
        }

        #endregion

        protected abstract GameObject FindNearestTarget();

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
