using UnityEngine;
using System.Collections.Generic;

namespace MiniJam159.AI
{
    public abstract class GameAI : MonoBehaviour
    {
        #region Inspector members

        public Sprite displaySprite;

        #endregion

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

        protected virtual void FixedUpdate()
        {
            // Set y position
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);

            // Remove velocity
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Set mesh position
            transform.Find("Mesh").position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }

        protected void MoveTowardsPosition(float moveSpeed)
        {
            Vector2 transformPosition = new Vector2(transform.position.x, transform.position.z);
            Vector2 direction = (moveToPosition - transformPosition).normalized;
            Vector2 moveTowardsDestination = Vector2.MoveTowards(transformPosition, moveToPosition, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(moveTowardsDestination.x, 0, moveTowardsDestination.y);

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
            if (Vector2.Distance(transformPosition, moveToPosition) <= 0.5f)
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
            moveToPosition = new Vector2(transform.position.x, transform.position.z);
        }

        protected abstract void FindNearestTarget();

        protected Transform Target { get; set; } // Property to be implemented by subclasses
    }
}
