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

    [Serializable]
    public class StructureData
    {
        public StructureType structureType;

        public Vector3 position;
        public Vector3 size;

        public float maxHealth;
        public float contructionTime;

        public List<CommandType> commands;

        public Sprite displayIcon;

        public StructureData()
        {
            commands = new List<CommandType>();
        }

        public StructureData(StructureData other)
        {
            structureType = other.structureType;
            position = other.position;
            size = other.size;
            maxHealth = other.maxHealth;
            contructionTime = other.contructionTime;
            commands = new List<CommandType>(other.commands);
            displayIcon = other.displayIcon;
        }
    }

    public class Structure : MonoBehaviour
    {
        #region Inspector members

        public StructureData structureData;

        #endregion

        public float health;

        protected virtual void Awake()
        {
            structureData = new StructureData();

            // Resize blocked tiles
        }

        public virtual void populateCommands()
        {
            CommandManagerBase.instance.populateCommands(structureData.commands);
        }
    }

    public class NestStructure : Structure
    {

    }

    public class WombStructure : Structure
    {

    }

}
