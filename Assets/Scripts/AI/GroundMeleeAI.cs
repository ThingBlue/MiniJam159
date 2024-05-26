using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MiniJam159.AI
{
    public class GroundMeleeAI : GameAI
    {
        public string targetTag = "Enemy"; // Tag to identify targets
        public float detectionRadius = 15.0f; // Radius to detect the nearest target
        public float attackRange = 1.0f; // Range within which the AI will attack
        public float attackCooldown = 1.0f; // Time between attacks
        public int attackDamage = 10; // Damage dealt by each attack
        public float moveSpeed = 3.0f; // Movement speed of the AI
        public float surroundDistance = 1.5f; // Distance to maintain around the target when surrounding
        public float coordinationRadius = 5.0f; // Radius within which AIs coordinate their actions

        protected float attackTimer;
        protected bool isLeader;

        protected override void Start()
        {
            base.Start();
            attackTimer = 0f;
            isLeader = false;
        }

        protected virtual void Update()
        {
            if (isMovingToPosition)
            {
                MoveTowardsPosition(moveSpeed);
            }
            else
            {
                if (Target == null)
                {
                    FindNearestTarget();
                }

                if (Target == null)
                {
                    return; // No target found, do nothing
                }

                float distanceToTarget = Vector2.Distance(transform.position, Target.position);

                if (distanceToTarget <= attackRange)
                {
                    if (isLeader)
                    {
                        CoordinatedAttack();
                    }
                    else
                    {
                        Attack();
                    }
                }
                else
                {
                    MoveTowardsSurroundPosition();
                }
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            if (moveIgnoreTargetTimer > 0)
            {
                moveIgnoreTargetTimer -= Time.deltaTime;
            }
        }

        public void SetTargetTag(string newTargetTag)
        {
            targetTag = newTargetTag;
        }

        protected override void FindNearestTarget()
        {
            if (moveIgnoreTargetTimer > 0)
            {
                return; // Ignore finding targets if timer is active
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            float nearestDistance = Mathf.Infinity;
            Transform nearestTarget = null;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag(targetTag))
                {
                    float distanceToTarget = Vector2.Distance(transform.position, hitCollider.transform.position);
                    if (distanceToTarget < nearestDistance)
                    {
                        nearestDistance = distanceToTarget;
                        nearestTarget = hitCollider.transform;
                    }
                }
            }

            Target = nearestTarget;

            // Assign leader dynamically
            if (Target != null)
            {
                AssignLeader();
            }
        }

        void MoveTowardsSurroundPosition()
        {
            Vector2 surroundPosition = GetSurroundPosition();
            Vector2 direction = (surroundPosition - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, surroundPosition, moveSpeed * Time.deltaTime);
        }

        Vector2 GetSurroundPosition()
        {
            if (Target == null)
            {
                return transform.position;
            }

            // Filter the list of activeAIs to include only those targeting the same target
            List<GroundMeleeAI> sameTargetAIs = GameAI.allAIs.OfType<GroundMeleeAI>().Where(ai => ai.Target == this.Target).ToList();
            int aiIndex = sameTargetAIs.IndexOf(this);
            int totalAIs = sameTargetAIs.Count;

            float angleIncrement = 270f / (totalAIs - 1); // 270 degrees spread over number of AIs
            float angle = -135f + (aiIndex * angleIncrement); // Start at -135 degrees to +135 degrees

            Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * surroundDistance;

            return (Vector2)Target.position + offset;
        }

        void Attack()
        {
            if (attackTimer <= 0)
            {
                Debug.Log("Attacking the target for " + attackDamage + " damage.");
                // Implement health reduction on the target here

                attackTimer = attackCooldown;
            }
        }

        void CoordinatedAttack()
        {
            foreach (var ai in GameAI.allAIs.OfType<GroundMeleeAI>())
            {
                if (ai.Target == this.Target && Vector2.Distance(transform.position, ai.transform.position) <= coordinationRadius)
                {
                    ai.Attack();
                }
            }
        }

        void AssignLeader()
        {
            float minDistance = Mathf.Infinity;
            GroundMeleeAI leader = null;

            foreach (var ai in GameAI.allAIs.OfType<GroundMeleeAI>())
            {
                if (ai.Target == this.Target)
                {
                    float distanceToTarget = Vector2.Distance(ai.transform.position, Target.position);
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
                foreach (var ai in GameAI.allAIs.OfType<GroundMeleeAI>())
                {
                    if (ai != leader)
                    {
                        ai.isLeader = false;
                    }
                }
            }
        }

        public virtual void attackAICommand(Transform newTarget)
        {
            Target = newTarget;
            isMovingToPosition = false;
        }

        void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position to visualize detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}
