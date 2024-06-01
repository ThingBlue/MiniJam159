using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MiniJam159.AI
{
    public class GroundMeleeAI : GameAI
    {
        public string targetTag = "Enemy"; // Tag to identify targets
        public float detectionRadius = 15.0f; // Radius to detect the nearest target
        public float attackRange = 2.0f; // Range within which the AI will attack
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

        private void Update()
        {
            if (attackTimer > 0) attackTimer -= Time.deltaTime;
            if (moveIgnoreTargetTimer > 0) moveIgnoreTargetTimer -= Time.deltaTime;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            offensiveMovementUpdate();
        }

        protected virtual void offensiveMovementUpdate()
        {
            if (isMovingToPosition)
            {
                MoveTowardsPosition(moveSpeed);
            }
            else
            {
                if (target == null)
                {
                    FindNearestTarget();
                }

                if (target == null)
                {
                    return; // No target found, do nothing
                }

                float distanceToTarget = Vector3.Distance(transform.position, target.position);

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

            // EDIT EDIT EDIT
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            float nearestDistance = Mathf.Infinity;
            Transform nearestTarget = null;

            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag(targetTag))
                {
                    float distanceToTarget = Vector3.Distance(transform.position, hitCollider.transform.position);
                    if (distanceToTarget < nearestDistance)
                    {
                        nearestDistance = distanceToTarget;
                        nearestTarget = hitCollider.transform;
                    }
                }
            }

            target = nearestTarget;

            // Assign leader dynamically
            if (target != null)
            {
                AssignLeader();
            }
        }

        void MoveTowardsSurroundPosition()
        {
            Vector3 surroundPosition = GetSurroundPosition();
            Vector3 direction = (surroundPosition - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, surroundPosition, moveSpeed * Time.deltaTime);
        }

        Vector3 GetSurroundPosition()
        {
            if (target == null) return transform.position;

            // Filter the list of activeAIs to include only those targeting the same target
            List<GroundMeleeAI> sameTargetAIs = GameAI.allAIs.OfType<GroundMeleeAI>().Where(ai => ai.target == this.target).ToList();

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
                Debug.Log("Attacking the target for " + attackDamage + " damage.");
                // Implement health reduction on the target here

                attackTimer = attackCooldown;
            }
        }

        void CoordinatedAttack()
        {
            foreach (var ai in GameAI.allAIs.OfType<GroundMeleeAI>())
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
            GroundMeleeAI leader = null;

            foreach (var ai in GameAI.allAIs.OfType<GroundMeleeAI>())
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
            target = newTarget;
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
