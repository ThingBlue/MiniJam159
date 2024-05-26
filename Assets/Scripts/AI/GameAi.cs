using UnityEngine;
using System.Collections.Generic;

namespace MiniJam159.AI
{
    public abstract class GameAI : MonoBehaviour
    {
        public static List<GameAI> allAIs = new List<GameAI>(); // List of all AI instances
        protected Vector2 moveToPosition;
        protected bool isMovingToPosition;
        protected float moveIgnoreTargetTimer; // Timer to ignore targets while moving
        protected const float moveIgnoreTargetDuration = 10f; // Duration to ignore targets while moving
        public List<CommandType> commandTypes;

        protected virtual void Start()
        {
            allAIs.Add(this);
            isMovingToPosition = false;
            moveIgnoreTargetTimer = 0f;
        }

        protected virtual void OnDestroy()
        {
            allAIs.Remove(this);
        }

        protected void MoveTowardsPosition(float moveSpeed)
        {
            Vector2 direction = (moveToPosition - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, moveToPosition, moveSpeed * Time.deltaTime);

            if (moveIgnoreTargetTimer <= 0)
            {
                // Check for targets while moving if the ignore timer is not active
                FindNearestTarget();

                if (Target != null)
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
            Target = null; // Reset target
            moveIgnoreTargetTimer = moveIgnoreTargetDuration; // Start ignore target timer
        }

        public virtual void moveAICommand(Vector2 position)
        {
            MoveTo(position);
        }

        public virtual void holdAICommand()
        {
            Target = null; // Reset target
            
            // Stop moving to position if hold command is issued
            isMovingToPosition = false;
            moveToPosition = transform.position;
        }

        protected abstract void FindNearestTarget();

        protected Transform Target { get; set; } // Property to be implemented by subclasses
    }
}
