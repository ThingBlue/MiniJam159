using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MiniJam159.CommandCore;

namespace MiniJam159.Structures
{
    public enum StructureType
    {
        NULL = 0,
        NEST,
        WOMB
    }

    public class Structure : MonoBehaviour
    {
        #region Inspector members
    
        public StructureType structureType;

        public Vector3 position;
        public Vector3 size;

        public float maxHealth;
        public float buildTime;

        public List<CommandType> commands;

        public Sprite displayIcon;

        #endregion

        public float health = 1;
        public float buildProgress = 0;

        protected virtual void Awake()
        {
            // Initialization
            commands = new List<CommandType>();
        }

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }

        public virtual void addBuildProgress(float amount)
        {
            buildProgress += amount;

            // Increase health based on amount added
            float percentageProgress = amount / buildTime;
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

}
