using UnityEngine;

public class GroundMeleeAI : MonoBehaviour
{
    public string targetTag = "Enemy"; // Tag to identify targets
    public float detectionRadius = 15.0f; // Radius to detect the nearest target
    public float attackRange = 1.0f; // Range within which the AI will attack
    public float attackCooldown = 1.0f; // Time between attacks
    public int attackDamage = 10; // Damage dealt by each attack
    public float moveSpeed = 3.0f; // Movement speed of the AI

    private Transform target;
    private float attackTimer;

    void Start()
    {
        attackTimer = 0f;
    }

    void Update()
    {
        if (target == null)
        {
            FindNearestTarget();
        }

        if (target == null)
        {
            return; // No target found, do nothing
        }

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget <= attackRange)
        {
            Attack();
        }
        else
        {
            MoveTowardsTarget();
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    public void SetTargetTag(string newTargetTag)
    {
        targetTag = newTargetTag;
    }

    void FindNearestTarget()
    {
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

        target = nearestTarget;
    }

    void MoveTowardsTarget()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
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

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position to visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
