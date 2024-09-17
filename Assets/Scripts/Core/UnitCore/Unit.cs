using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

using MiniJam159.CommandCore;
using MiniJam159.GameCore;

namespace MiniJam159.UnitCore
{
    public enum UnitJobType
    {
        // Common jobs
        IDLE = 0,
        MOVE_TO_POSITION,
        ATTACK,

        // Worker jobs
        HARVEST_RESOURCE,
        BUILD
    }

    public abstract class Unit : Entity
    {
        #region Inspector members

        public HealthBar healthBar;

        #endregion

        public static List<Unit> allAIs = new List<Unit>(); // List of all AI instances

        public UnitJobType currentAIJob = UnitJobType.IDLE;
        public List<CommandType> commands = new List<CommandType>();

        public float health;

        protected Vector3 moveToPosition;
        protected bool moveIgnoreEnemies;

        //protected float moveIgnoreTargetTimer; // Timer to ignore targets while moving
        protected const float moveIgnoreTargetDuration = 10f; // Duration to ignore targets while moving
        protected Transform target { get; set; } // Property to be implemented by subclasses

        private void Awake()
        {
            // TEMP
            allAIs.Add(this);
        }

        protected virtual void Start()
        {
            // TEMP
            EntityManager.instance.playerUnitObjects.Add(gameObject);
            EntityManager.instance.playerEntityObjects.Add(gameObject);

            currentAIJob = UnitJobType.IDLE;
            moveIgnoreEnemies = false;

            // Start at max health
            health = maxHealth;

            // Set health bar values
            healthBar.setMaxHealth(maxHealth);
            healthBar.setHealth(health);
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
                    currentAIJob = UnitJobType.IDLE;
                    moveIgnoreEnemies = false;
                    return; // Stop moving to position if a target is found
                }
            }

            // Stop moving to position if reached
            if (Vector3.Distance(transform.position, moveToPosition) <= 0.5f)
            {
                currentAIJob = UnitJobType.IDLE;
                moveIgnoreEnemies = false;
            }

            Vector3 moveTowardsDestination = Vector3.MoveTowards(transform.position, moveToPosition, moveSpeed * Time.deltaTime);
            transform.position = moveTowardsDestination;
        }

        public void MoveTo(Vector3 position, bool ignoreEnemies)
        {
            moveToPosition = position;
            currentAIJob = UnitJobType.MOVE_TO_POSITION;
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
            currentAIJob = UnitJobType.IDLE;
            moveToPosition = transform.position;
        }

        protected abstract void FindNearestTarget();

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }
    }

}
