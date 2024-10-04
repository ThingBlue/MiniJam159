using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.StructureCore
{
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

    public class StructureManagerBase : MonoBehaviour
    {
        public List<GameObject> depositPointStructures;

        // Singleton
        public static StructureManagerBase instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public virtual void beginPlacement(StructureType structureType, GameObject structurePrefab)
        {
            // See StructureManager::beginPlacement(StructureType structureType, GameObject structurePrefab)
        }

        public virtual void cancelPlacement()
        {
            // See StructureManager::cancelPlacement()
        }

        public virtual StructurePlacementData confirmPlacement()
        {
            return null;

            // See StructureManager::confirmPlacement()
        }

        public virtual GameObject createStructure(StructurePlacementData placementData)
        {
            return null;

            // See StructureManager::confirmPlacement()
        }
    }
}
