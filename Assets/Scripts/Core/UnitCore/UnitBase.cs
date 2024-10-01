using UnityEngine;
using System.Collections.Generic;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;
using System.Linq;
using Codice.CM.Common;

namespace MiniJam159.UnitCore
{
    public abstract class UnitBase : Entity
    {
        #region Inspector members

        public HealthBar healthBar;
        public List<CommandType> commands = new List<CommandType>();

        public float moveSpeed;
        public float pathUpdateInterval;
        public float pathfindingRadius;

        public string targetTag = "Enemy"; // Tag to identify targets

        public int attackDamage = 10; // Damage dealt by each attack
        public float attackCooldown = 1.0f; // Time between attacks
        public float detectionRadius = 5.0f; // Radius to detect the nearest target
        public float attackRange = 2.0f; // Range within which the AI will attack

        public float surroundDistance = 1.5f; // Distance to maintain around the target when surrounding
        public float coordinationRadius = 5.0f; // Radius within which AIs coordinate their actions

        #endregion

        public float health;
        protected float attackTimer = 0f;

        // Actions
        public Queue<Action> actionQueue = new Queue<Action>();
        public Action currentAction = null;

        // Pathfinding
        public Queue<Vector3> path = new Queue<Vector3>();
        protected float pathUpdateTimer;

        // Collisions
        public List<Collider> collisions = new List<Collider>();

        protected Vector3 movement = Vector3.zero;

        protected virtual void Start()
        {
            // TEMP
            EntityManager.instance.playerUnitObjects.Add(gameObject);
            EntityManager.instance.playerEntityObjects.Add(gameObject);

            // Start at max health
            health = maxHealth;

            // Set health bar values
            healthBar.setMaxHealth(maxHealth);
            healthBar.setHealth(health);
        }

        protected virtual void OnDestroy()
        {
            EntityManager.instance.playerUnitObjects.Remove(gameObject);
        }

        protected virtual void FixedUpdate()
        {
            // Set y position and remove velocity
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Set mesh position
            transform.Find("Mesh").position = new Vector3(transform.position.x, 0.4f, transform.position.z);
        }

        protected virtual void handleActions()
        {
            // See Unit::handleActions
        }

        protected virtual void clearActionQueue()
        {
            // Remove all actions from queue
            while (actionQueue.Count > 0)
            {
                // Update action indicators
                Action action = actionQueue.Dequeue();
                ActionIndicatorManagerBase.instance.completeAction(this, action);
            }
        }

        protected virtual void handleCollisions()
        {
            // See Unit::handleCollisions()
        }

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            collisions.Add(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            collisions.Remove(other);
        }

    }

}
