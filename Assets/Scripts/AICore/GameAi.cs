using UnityEngine;
using System.Collections.Generic;
using MiniJam159.Commands;

namespace MiniJam159.AICore
{
    public abstract class GameAI : MonoBehaviour
    {
        #region Inspector members

        public Sprite displaySprite;

        #endregion

        public static List<GameAI> allAIs = new List<GameAI>(); // List of all AI instances
        public List<CommandType> commands;

        protected Vector3 moveToPosition;
        protected bool isMovingToPosition;
        protected float moveIgnoreTargetTimer; // Timer to ignore targets while moving
        protected const float moveIgnoreTargetDuration = 10f; // Duration to ignore targets while moving
        protected Transform target { get; set; } // Property to be implemented by subclasses

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
            Vector3 moveTowardsDestination = Vector3.MoveTowards(transform.position, moveToPosition, moveSpeed * Time.deltaTime);
            transform.position = moveTowardsDestination;

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
            if (Vector3.Distance(transform.position, moveToPosition) <= 0.5f)
            {
                isMovingToPosition = false;
            }
        }

        public void MoveTo(Vector3 position)
        {
            moveToPosition = position;
            isMovingToPosition = true;
            target = null; // Reset target
            moveIgnoreTargetTimer = moveIgnoreTargetDuration; // Start ignore target timer
        }

        public virtual void moveAICommand(Vector3 position)
        {
            MoveTo(position);
        }

        public virtual void holdAICommand()
        {
            target = null; // Reset target
            
            // Stop moving to position if hold command is issued
            isMovingToPosition = false;
            moveToPosition = transform.position;
        }

        protected abstract void FindNearestTarget();

        public virtual void populateCommands()
        {
            CommandManager.instance.populateCommands(commands);
        }

    }
}
