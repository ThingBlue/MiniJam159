using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.AI;
using System;

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

        public Vector2 position;
        public Vector2 size;

        public float maxHealth;
        public float contructionTime;

        public List<CommandType> commands;

        public Sprite displaySprite;

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
            displaySprite = other.displaySprite;
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
    }

    public class NestStructure : Structure
    {

    }

    public class WombStructure : Structure
    {

    }

}
