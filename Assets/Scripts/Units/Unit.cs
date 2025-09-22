using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

using MiniJam159.UnitCore;
using MiniJam159.CommandCore;
using MiniJam159.StructureCore;
using MiniJam159.Common;
using MiniJam159.EntityCore;
using MiniJam159.MapCore;

namespace MiniJam159.Units
{
    public class Unit : UnitBase
    {
        #region Inspector members

        public HealthBar healthBar;

        public float moveSpeed;
        public float pathfindingRadius;

        public string targetTag = "Enemy"; // Tag to identify targets

        public int attackDamage = 10; // Damage dealt by each attack
        public float attackCooldown = 1.0f; // Time between attacks
        public float detectionRadius = 5.0f; // Radius to detect the nearest target
        public float attackRange = 2.0f; // Range within which the AI will attack

        public float surroundDistance = 1.5f; // Distance to maintain around the target when surrounding
        public float coordinationRadius = 5.0f; // Radius within which AIs coordinate their actions

        #endregion

        protected Action lastAction = null;

        protected float attackTimer = 0f;

        // Pathfinding
        public Queue<Vector3> path = new Queue<Vector3>();
        protected bool pathNeedsUpdate = true;

        // Collisions
        public List<Collider> collisions = new List<Collider>();

        protected Vector3 movement = Vector3.zero;

        protected override void Start()
        {
            base.Start();

            // TEMP
            EntityManagerBase.instance.playerUnitObjects.Add(gameObject);
            EntityManagerBase.instance.playerEntityObjects.Add(gameObject);

            // Start at max health
            health = maxHealth;

            // Subscribe to events
            GridManagerBase.instance.mapChangedEvent.AddListener(onMapChangedCallback);
        }

        protected virtual void OnDestroy()
        {
            EntityManagerBase.instance.playerUnitObjects.Remove(gameObject);
        }

        protected override void Update()
        {
            // Increment timers
            attackTimer += Time.deltaTime;
        }

        protected override void FixedUpdate()
        {
            // Handle actions (and movement)
            if (actionQueue.Count > 0) handleActions();

            // Handle collisions
            handleCollisions();

            // Raycast ahead to prevent moving through objects
            RaycastHit hitInfo;
            if (Physics.Linecast(transform.position, transform.position + movement, out hitInfo)) movement = hitInfo.point - transform.position;

            // Do movement and reset
            transform.position += movement;
            movement = Vector3.zero;

            // Set y position and remove velocity
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Set mesh position
            transform.Find("Mesh").position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }

        protected virtual bool handlePathing(Vector3 targetPosition, Vector3 targetSize, List<TileIgnoreData> tileIgnoreData)
        {
            // Check if current path is still valid
            if (pathNeedsUpdate)
            {
                //path = GridManagerBase.instance.getPathQueue(transform.position, targetPosition, pathfindingRadius, new List<TileIgnoreData>());
                PathRequest request = new PathRequest(transform.position, targetPosition, pathfindingRadius, new List<TileIgnoreData>(), onPathfindingCompleteCallback);
                PathfinderBase.instance.addPathRequest(request);
                pathNeedsUpdate = false;

                // Temporary path list
                path = new Queue<Vector3>(new[] { targetPosition });
            }

            // Return true if path ended
            if (path.Count == 0) return true;
            // Return true if target reached
            if (transform.position.x >= (targetPosition.x - targetSize.x / 2.0f) - pathfindingRadius &&
                transform.position.x <= (targetPosition.x + targetSize.x / 2.0f) + pathfindingRadius &&
                transform.position.z >= (targetPosition.z - targetSize.z / 2.0f) - pathfindingRadius &&
                transform.position.z <= (targetPosition.z + targetSize.z / 2.0f) + pathfindingRadius)
            {
                // Target reached so we can stop pathing
                path.Clear();
                return true;
            }

            // Stop moving to waypoint if reached
            if (Vector3.Distance(transform.position, path.Peek()) <= pathfindingRadius)
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
            //return (path.Count == 0);
            return false;
        }

        #region Action handlers

        protected virtual void handleActions()
        {
            if (actionQueue.Count == 0) return;

            // Handle current action
            Action currentAction = actionQueue.Peek();

            if (currentAction != null && currentAction != lastAction)
            {
                lastAction = currentAction;
                pathNeedsUpdate = true; // Reset path flag on new action
            }

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
            bool movementResult = handlePathing(action.targetPosition, Vector3.zero, new List<TileIgnoreData>());

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
                if (pathNeedsUpdate)
                {
                    //path = GridManagerBase.instance.getPathQueue(transform.position, action.targetObject.transform.position, pathfindingRadius, new List<TileIgnoreData>());
                    PathRequest request = new PathRequest(transform.position, action.targetObject.transform.position, pathfindingRadius, new List<TileIgnoreData>(), onPathfindingCompleteCallback);
                    PathfinderBase.instance.addPathRequest(request);
                    pathNeedsUpdate = false;
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
            if (GridManagerBase.instance.isTileOccupied(MathUtilities.floorVector3(targetPosition)))
            {
                // Find closest free position to move to
                targetPosition = GridManagerBase.instance.calculateClosestFreeTile(targetPosition, transform.position);
            }

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

        #region Callbacks

        private void onMapChangedCallback()
        {
            pathNeedsUpdate = true;
        }

        private void onPathfindingCompleteCallback(Queue<Vector3> path)
        {
            this.path = path;
            return;
        }

        #endregion

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
