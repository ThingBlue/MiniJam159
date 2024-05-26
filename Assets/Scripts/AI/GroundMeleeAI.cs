using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GroundMeleeAI : MonoBehaviour
{
    public string targetTag = "Enemy"; // Tag to identify targets
    public float detectionRadius = 15.0f; // Radius to detect the nearest target
    public float attackRange = 1.0f; // Range within which the AI will attack
    public float attackCooldown = 1.0f; // Time between attacks
    public int attackDamage = 10; // Damage dealt by each attack
    public float moveSpeed = 3.0f; // Movement speed of the AI
    public float surroundDistance = 1.5f; // Distance to maintain around the target when surrounding
    public float coordinationRadius = 5.0f; // Radius within which AIs coordinate their actions

    private static List<GroundMeleeAI> activeAIs = new List<GroundMeleeAI>(); // List of active AIs
    private Transform target;
    private Vector2 moveToPosition;
    private bool isMovingToPosition;
    private float attackTimer;
    private bool isLeader;
    private float moveIgnoreTargetTimer; // Timer to ignore targets while moving
    private const float moveIgnoreTargetDuration = 10f; // Duration to ignore targets while moving

    void Start()
    {
        activeAIs.Add(this);
        attackTimer = 0f;
        isLeader = false;
        isMovingToPosition = false;
        moveIgnoreTargetTimer = 0f;
    }

    void OnDestroy()
    {
        activeAIs.Remove(this);
    }

    void Update()
    {
        if (isMovingToPosition)
        {
            MoveTowardsPosition();
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

            float distanceToTarget = Vector2.Distance(transform.position, target.position);

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

    void FindNearestTarget()
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

        target = nearestTarget;

        // Assign leader dynamically
        if (target != null)
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

    void MoveTowardsPosition()
    {
        Vector2 direction = (moveToPosition - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, moveToPosition, moveSpeed * Time.deltaTime);

        if (moveIgnoreTargetTimer <= 0)
        {
            // Check for targets while moving if the ignore timer is not active
            FindNearestTarget();

            if (target != null)
            {
                isMovingToPosition = false;
                return; // Stop moving to position if a target is found
            }
        }

        // Stop moving to position if reached
        if (Vector2.Distance(transform.position, moveToPosition) < 0.1f)
        {
            isMovingToPosition = false;
        }
    }

    public void MoveTo(Vector2 position)
    {
        moveToPosition = position;
        isMovingToPosition = true;
        target = null; // Reset target
        moveIgnoreTargetTimer = moveIgnoreTargetDuration; // Start ignore target timer
    }

    Vector2 GetSurroundPosition()
    {
        if (target == null)
        {
            return transform.position;
        }

        // Filter the list of activeAIs to include only those targeting the same target
        List<GroundMeleeAI> sameTargetAIs = activeAIs.Where(ai => ai.target == this.target).ToList();
        int aiIndex = sameTargetAIs.IndexOf(this);
        int totalAIs = sameTargetAIs.Count;

        float angleIncrement = 270f / (totalAIs - 1); // 270 degrees spread over number of AIs
        float angle = -135f + (aiIndex * angleIncrement); // Start at -135 degrees to +135 degrees

        Vector2 offset = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * surroundDistance;

        return (Vector2)target.position + offset;
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
        foreach (var ai in activeAIs)
        {
            if (ai.target == this.target && Vector2.Distance(transform.position, ai.transform.position) <= coordinationRadius)
            {
                ai.Attack();
            }
        }
    }

    void AssignLeader()
    {
        float minDistance = Mathf.Infinity;
        GroundMeleeAI leader = null;

        foreach (var ai in activeAIs)
        {
            if (ai.target == this.target)
            {
                float distanceToTarget = Vector2.Distance(ai.transform.position, target.position);
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
            foreach (var ai in activeAIs)
            {
                if (ai != leader)
                {
                    ai.isLeader = false;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position to visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
