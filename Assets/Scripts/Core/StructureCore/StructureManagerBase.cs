using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.Common;
using UnityEngine.Events;

namespace MiniJam159.StructureCore
{
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

        // See StructureManager for implementations
        public virtual void beginPlacement(StructureType structureType) { }
        public virtual void cancelPlacement() { }
        public virtual GameObject confirmPlacement() { return null; }
        public virtual Sprite getStructureSprite (StructureType structureType) { return null; }
    }
}
