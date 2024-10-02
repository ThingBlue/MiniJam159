using UnityEngine;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;

namespace MiniJam159.Units
{
    public class RangedUnit : Unit
    {
        #region Inspector members

        public GameObject projectilePrefab; // Prefab for the projectile

        #endregion


        #region Action handlers

        protected override void handleActions()
        {
            if (actionQueue.Count == 0) return;

            // Handle current action
            Action currentAction = actionQueue.Peek();
            switch (currentAction.actionType)
            {
                default:
                    base.handleActions();
                    break;
            }

            base.handleActions();

            // We remove actions from the queue after completing them
        }

        protected override void handleAttackAction(AttackAction action)
        {
            base.handleAttackAction(action);
        }

        protected override void handleAttackMoveAction(AttackMoveAction action)
        {
            base.handleAttackMoveAction(action);
        }

        #endregion

        #region Command handlers

        #endregion

        /*
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
        */

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
