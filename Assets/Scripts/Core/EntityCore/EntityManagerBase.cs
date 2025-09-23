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
        public bool playerOwned;
    }

    public class EntityManagerBase : MonoBehaviour
    {
        #region Inspector members

        public SerializedTrainingDictionary trainingDictionary;

        #endregion

        public List<GameObject> entityObjects = new List<GameObject>();

        // TODO: Remove objects from above lists on destroy

        // Singleton
        public static EntityManagerBase instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See implementations in EntityManager
        public virtual List<GameObject> getEntityObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getUnitObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getStructureObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getPlayerEntityObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getPlayerUnitObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getPlayerStructureObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getNonPlayerEntityObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getNonPlayerUnitObjects() { return new List<GameObject>(); }
        public virtual List<GameObject> getNonPlayerStructureObjects() { return new List<GameObject>(); }

        public virtual void removeEntityObject(GameObject entityObject) { entityObjects.Remove(entityObject); }

        public virtual GameObject CreateEntity(EntityCreationData entityCreationData)
        {
            // See EntityManager::CreateEntity()
            return null;
        }
    }
}
