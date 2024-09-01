using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class EntityManager : MonoBehaviour
    {
        public List<GameObject> playerEntityObjects = new List<GameObject>();
        public List<GameObject> playerUnitObjects = new List<GameObject>();
        public List<GameObject> playerStructureObjects = new List<GameObject>();

        // TODO: Remove objects from above lists on destroy

        // Singleton
        public static EntityManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }
    }
}
