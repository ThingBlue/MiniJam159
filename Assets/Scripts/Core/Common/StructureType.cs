using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Common
{
    public enum StructureType
    {
        NULL = 0,
        NEST,
        WOMB
    }

    public class StructurePlacementData
    {
        public StructureType structureType;
        public Vector3 position;
        public Vector3 size;

        public StructurePlacementData() { }
        public StructurePlacementData(StructureType structureType, Vector3 position, Vector3 size)
        {
            this.structureType = structureType;
            this.position = position;
            this.size = size;
        }
    }
}
