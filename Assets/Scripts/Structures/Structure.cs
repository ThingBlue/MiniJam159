using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.AI;

namespace MiniJam159.Structures
{
    public class StructureData
    {
        public Vector2 position;
        public Vector2 size;

        public float maxHealth;
        public float health;

        public float contructionTime;

        public List<CommandType> commands;
    }

    public class Structure : MonoBehaviour
    {
        public StructureData structureData;

        private void Awake()
        {
            structureData = new StructureData();
        }
    }
}
