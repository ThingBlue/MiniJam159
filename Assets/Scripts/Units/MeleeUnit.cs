using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;

namespace MiniJam159.Units
{
    public class MeleeUnit : Unit
    {
        #region Inspector members

        #endregion

        protected bool isLeader;

        protected override void Start()
        {
            base.Start();
            isLeader = false;
        }

        #region Action handling

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

        #region Command handling

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

            // Assign leader dynamically
            if (target != null)
            {
                AssignLeader();
            }
        }
        */

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
