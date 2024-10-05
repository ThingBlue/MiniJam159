using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MiniJam159.GameCore;
using MiniJam159.Common;
using MiniJam159.CommandCore;

namespace MiniJam159.StructureCore
{
    public class Structure : Entity
    {
        #region Inspector members

        public StructureType structureType;

        public Vector3 size;

        public HealthBar healthBar;
        public List<CommandType> commands;

        public float maxBuildProgress;

        #endregion

        public float health = 1;
        public float buildProgress = 0;

        protected virtual void Awake()
        {
            // Initialization
            commands = new List<CommandType>();
        }

        protected void Start()
        {
            // Set health bar values
            healthBar.setMaxHealth(maxHealth);
            healthBar.setHealth(health);
        }

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }

        public virtual void addBuildProgress(float amount)
        {
            buildProgress += amount;

            // Clamp build progress
            buildProgress = Mathf.Min(buildProgress, maxBuildProgress);

            // Increase health based on amount added
            float percentageProgress = amount / maxBuildProgress;
            health += percentageProgress * maxHealth;

            // Clamp health value
            health = Mathf.Min(health, maxHealth);

            // Update health bar
            healthBar.setHealth(health);
        }
    }

    public class NestStructure : Structure
    {

    }

    public class WombStructure : Structure
    {

    }

}
