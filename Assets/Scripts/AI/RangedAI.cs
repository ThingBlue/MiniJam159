using UnityEngine;
using MiniJam159.GameCore;

namespace MiniJam159.AI
{
    public class RangedAI : GameAI
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

        void Update()
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
                    Attack();
                }
                else
                {
                    MoveTowardsTarget();
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
        }

        void MoveTowardsTarget()
        {
            Vector2 direction = (Target.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, Target.position, moveSpeed * Time.deltaTime);
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
                    projectile.GetComponent<Projectile>().Initialize(Target.position, attackDamage);
                }

                attackTimer = attackCooldown;
            }
        }

        public void attackAICommand(Transform newTarget)
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
