using UnityEngine;

using MiniJam159.GameCore;

namespace MiniJam159.UnitCore
{
    public class RangedUnit : Unit
    {
        public string targetTag = "Enemy"; // Tag to identify targets
        public float detectionRadius = 20.0f; // Radius to detect the nearest target
        public float attackRange = 10.0f; // Range within which the AI will attack
        public float attackCooldown = 2.0f; // Time between attacks
        public int attackDamage = 5; // Damage dealt by each attack
        public float moveSpeed = 2.0f; // Movement speed of the AI
        public GameObject projectilePrefab; // Prefab for the projectile

        private float attackTimer;

        protected override void Start()
        {
            base.Start();
            attackTimer = 0f;
        }

        private void Update()
        {
            if (attackTimer > 0) attackTimer -= Time.deltaTime;
            //if (moveIgnoreTargetTimer > 0) moveIgnoreTargetTimer -= Time.deltaTime;
        }

        #region Action handling

        protected override void handleActions()
        {
            // We remove actions from the queue after completing them

            if (actionQueue.Count == 0) return;

            // Handle current action
            Action currentAction = actionQueue.Peek();
            switch (currentAction.type)
            {
                case ActionType.ATTACK:
                    handleAttackAction(currentAction as AttackAction);
                    break;
                case ActionType.ATTACK_MOVE:
                    handleAttackMoveAction(currentAction as AttackMoveAction);
                    break;
                case ActionType.IDLE:
                    /*
                    // Search for target
                    if (target == null) FindNearestTarget();
                    if (target == null) return; // No target found, do nothing

                    // Target found
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);
                    if (distanceToTarget <= attackRange)
                    {
                        Attack();
                    }
                    else
                    {
                        MoveTowardsTarget();
                    }
                    */
                    break;
            }

            base.handleActions();
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

        public virtual void attackCommand(bool addToQueue, GameObject targetObject)
        {
            if (!addToQueue) actionQueue.Clear();
            actionQueue.Enqueue(new AttackAction(targetObject));
        }

        public virtual void attackMoveCommand(bool addToQueue, Vector3 targetPosition)
        {
            if (!addToQueue) actionQueue.Clear();
            actionQueue.Enqueue(new AttackMoveAction(targetPosition));
        }

        #endregion

        protected override GameObject FindNearestTarget()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
            float nearestDistance = Mathf.Infinity;
            GameObject nearestTarget = null;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag(targetTag))
                {
                    float distanceToTarget = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distanceToTarget < nearestDistance)
                    {
                        nearestDistance = distanceToTarget;
                        nearestTarget = hitCollider.gameObject;
                    }
                }
            }

            return nearestTarget;
        }

        /*
        void MoveTowardsTarget()
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        }

        void Attack()
        {
            if (attackTimer <= 0)
            {
                Debug.Log("Attacking the target for " + attackDamage + " damage.");
                // Instantiate and shoot the projectile towards the target
                if (projectilePrefab != null)
                {
                    GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    projectile.GetComponent<Projectile>().Initialize(target.position, attackDamage);
                }

                attackTimer = attackCooldown;
            }
        }
        */

        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position to visualize detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
