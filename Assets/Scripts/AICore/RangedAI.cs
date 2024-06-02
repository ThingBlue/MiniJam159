using UnityEngine;
using MiniJam159.GameCore;

namespace MiniJam159.AICore
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

        private void Update()
        {
            if (attackTimer > 0) attackTimer -= Time.deltaTime;
            //if (moveIgnoreTargetTimer > 0) moveIgnoreTargetTimer -= Time.deltaTime;
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
                    Attack();
                }
                else
                {
                    MoveTowardsTarget();
                }
            }
        }

        public void SetTargetTag(string newTargetTag)
        {
            targetTag = newTargetTag;
        }

        protected override void FindNearestTarget()
        {
            if (moveIgnoreEnemies)// moveIgnoreTargetTimer > 0)
            {
                return; // Ignore finding targets if timer is active
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
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
        }

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
