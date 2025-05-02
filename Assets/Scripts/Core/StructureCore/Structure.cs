using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using MiniJam159.GameCore;
using MiniJam159.Common;
using MiniJam159.CommandCore;
using MiniJam159.EntityCore;

namespace MiniJam159.StructureCore
{
    public enum StructureType
    {
        NULL = 0,
        NEST,
        WOMB,

        TEST_SQUARE
    }

    public class Structure : Entity
    {
        #region Inspector members

        public StructureType structureType;
        public Vector3 size;
        public float maxBuildProgress;

        public HealthBar healthBar;

        [SerializeReference]
        public List<CommandBase> commands = new List<CommandBase>();

        #endregion

        public Vector3 startPosition;

        public float buildProgress = 0;

        protected override void Start()
        {
            // Set health to 1 on start - Needs construction
            health = 1;
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
        }
    }

    public class NestStructure : Structure
    {

    }

    public class WombStructure : Structure
    {

    }

    public class TestSquareStructure : Structure
    {

    }

}
