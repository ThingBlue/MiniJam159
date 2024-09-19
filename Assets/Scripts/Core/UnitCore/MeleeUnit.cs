using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using MiniJam159.GameCore;
using UnityEngine.UIElements;

namespace MiniJam159.UnitCore
{
    public class MeleeUnit : Unit
    {
        #region Inspector members

        public string targetTag = "Enemy"; // Tag to identify targets

        public int attackDamage = 10; // Damage dealt by each attack
        public float attackCooldown = 1.0f; // Time between attacks
        public float detectionRadius = 15.0f; // Radius to detect the nearest target
        public float attackRange = 2.0f; // Range within which the AI will attack
        
        public float surroundDistance = 1.5f; // Distance to maintain around the target when surrounding
        public float coordinationRadius = 5.0f; // Radius within which AIs coordinate their actions

        #endregion

        protected float attackTimer;
        protected bool isLeader;

        protected override void Start()
        {
            base.Start();
            attackTimer = 0f;
            isLeader = false;
        }

        protected override void Update()
        {
            attackTimer += Time.deltaTime;

            base.Update();
        }

        #region Action handling

        protected override void handleActions()
        {
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
                default:
                    base.handleActions();
                    break;
            }

            // We remove actions from the queue after completing them
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

            /*
            // Assign leader dynamically
            if (target != null)
            {
                AssignLeader();
            }
            */
        }

        /*
        protected void MoveTowardsSurroundPosition()
        {
            Vector3 surroundPosition = GetSurroundPosition();
            Vector3 direction = (surroundPosition - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, surroundPosition, moveSpeed * Time.deltaTime);
        }

        protected Vector3 GetSurroundPosition()
        {
            if (target == null) return transform.position;

            // Filter the list of activeAIs to include only those targeting the same target
            List<MeleeUnit> sameTargetAIs = Unit.allAIs.OfType<MeleeUnit>().Where(ai => ai.target == this.target).ToList();

            // Default offset for single attacker
            Vector3 offset = new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)) * surroundDistance;
            if (sameTargetAIs.Count > 1)
            {
                // Calculate offset for multiple attackers
                int aiIndex = sameTargetAIs.IndexOf(this);
                int totalAIs = sameTargetAIs.Count;

                float angleIncrement = 270f / (totalAIs - 1); // 270 degrees spread over number of AIs
                float angle = -135f + (aiIndex * angleIncrement); // Start at -135 degrees to +135 degrees

                offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * surroundDistance;
            }
            return target.position + offset;
        }

        void Attack()
        {
            if (attackTimer <= 0)
            {
                // Implement health reduction on the target here

                attackTimer = attackCooldown;
            }
        }

        void CoordinatedAttack()
        {
            foreach (var ai in Unit.allAIs.OfType<MeleeUnit>())
            {
                if (ai.target == this.target && Vector3.Distance(transform.position, ai.transform.position) <= coordinationRadius)
                {
                    ai.Attack();
                }
            }
        }

        void AssignLeader()
        {
            float minDistance = Mathf.Infinity;
            MeleeUnit leader = null;

            foreach (var ai in Unit.allAIs.OfType<MeleeUnit>())
            {
                if (ai.target == this.target)
                {
                    float distanceToTarget = Vector3.Distance(ai.transform.position, target.position);
                    if (distanceToTarget < minDistance)
                    {
                        minDistance = distanceToTarget;
                        leader = ai;
                    }
                }
            }

            if (leader != null)
            {
                leader.isLeader = true;
                foreach (var ai in Unit.allAIs.OfType<MeleeUnit>())
                {
                    if (ai != leader)
                    {
                        ai.isLeader = false;
                    }
                }
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
