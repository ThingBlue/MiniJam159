using UnityEngine;
using System.Collections.Generic;
using MiniJam159.CommandCore;
using System.Runtime.InteropServices.WindowsRuntime;

namespace MiniJam159.AICore
{
    public enum AIJob
    {
        // Common jobs
        IDLE = 0,
        MOVE_TO_POSITION,
        ATTACK,

        // Worker jobs
        HARVEST_RESOURCE,
        BUILD
    }

    public abstract class GameAI : MonoBehaviour
    {
        #region Inspector members

        public Sprite displaySprite;
        public int sortPriority = 0;

        #endregion

        public static List<GameAI> allAIs = new List<GameAI>(); // List of all AI instances

        public AIJob currentAIJob = AIJob.IDLE;
        public List<CommandType> commands;

        protected Vector3 moveToPosition;
        protected bool moveIgnoreEnemies;

        //protected float moveIgnoreTargetTimer; // Timer to ignore targets while moving
        protected const float moveIgnoreTargetDuration = 10f; // Duration to ignore targets while moving
        protected Transform target { get; set; } // Property to be implemented by subclasses

        protected virtual void Start()
        {
            allAIs.Add(this);
            currentAIJob = AIJob.IDLE;
            moveIgnoreEnemies = false;
        }

        protected virtual void OnDestroy()
        {
            allAIs.Remove(this);
        }

        protected virtual void FixedUpdate()
        {
            // Set y position and remove velocity
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Set mesh position
            transform.Find("Mesh").position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }

        protected virtual void handleMoveJob(float moveSpeed)
        {
            if (!moveIgnoreEnemies)// || moveIgnoreTargetTimer <= 0)
            {
                // Check for targets while moving if the ignore timer is not active
                FindNearestTarget();

                if (target != null)
                {
                    currentAIJob = AIJob.IDLE;
                    moveIgnoreEnemies = false;
                    return; // Stop moving to position if a target is found
                }
            }

            // Stop moving to position if reached
            if (Vector3.Distance(transform.position, moveToPosition) <= 0.5f)
            {
                currentAIJob = AIJob.IDLE;
                moveIgnoreEnemies = false;
            }

            Vector3 moveTowardsDestination = Vector3.MoveTowards(transform.position, moveToPosition, moveSpeed * Time.deltaTime);
            transform.position = moveTowardsDestination;
        }

        public void MoveTo(Vector3 position, bool ignoreEnemies)
        {
            moveToPosition = position;
            currentAIJob = AIJob.MOVE_TO_POSITION;
            target = null; // Reset target
            moveIgnoreEnemies = ignoreEnemies;
        }

        public virtual void moveAICommand(Vector3 position)
        {
            MoveTo(position, true);
        }

        public virtual void attackMoveAICommand(Vector3 position)
        {
            MoveTo(position, false);
        }

        public virtual void holdAICommand()
        {
            target = null; // Reset target
            
            // Stop moving to position if hold command is issued
            currentAIJob = AIJob.IDLE;
            moveToPosition = transform.position;
        }

        protected abstract void FindNearestTarget();

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }
    }

    // Custom comparer class for units
    public class GameAIComparer : IComparer<GameAI>
    {
        public int Compare(GameAI gameAI1, GameAI gameAI2)
        {
            if (gameAI1 == null || gameAI2 == null) return 0;

            // Sort in descending order
            return gameAI2.sortPriority.CompareTo(gameAI1.sortPriority);
        }
    }

    public class GameAIGameObjectComparer : IComparer<GameObject>
    {
        public int Compare(GameObject gameObject1, GameObject gameObject2)
        {
            GameAI gameAI1 = gameObject1.GetComponent<GameAI>();
            GameAI gameAI2 = gameObject2.GetComponent<GameAI>();

            // Make sure we have GameAIs attached
            if (gameAI1 == null && gameAI2 == null) return 0;
            if (gameAI1 == null) return -1;
            if (gameAI2 == null) return 1;

            // Sort in descending order
            return gameAI2.sortPriority.CompareTo(gameAI1.sortPriority);
        }
    }

}
