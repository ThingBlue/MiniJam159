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
        public float contructionTime;

        public List<CommandType> commands;

        public Sprite displayIcon;

        #endregion

        public float health;
        public float constructionProgress;

        protected virtual void Awake()
        {
            // Initialization
            commands = new List<CommandType>();
        }

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(commands);
        }
    }

    public class NestStructure : Structure
    {

    }

    public class WombStructure : Structure
    {

    }

}
