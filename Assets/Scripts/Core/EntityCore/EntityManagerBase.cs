using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.EntityCore
{
    public struct EntityCreationData
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 targetPosition;
        public bool playerOwned;
    }

    public class EntityManagerBase : MonoBehaviour
    {
        public List<GameObject> playerEntityObjects = new List<GameObject>();
        public List<GameObject> playerUnitObjects = new List<GameObject>();
        public List<GameObject> playerStructureObjects = new List<GameObject>();

        // TODO: Remove objects from above lists on destroy

        // Singleton
        public static EntityManagerBase instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public virtual void CreateEntity(EntityCreationData entityCreationData)
        {
            // See EntityManager::CreateEntity()
        }
    }
}
