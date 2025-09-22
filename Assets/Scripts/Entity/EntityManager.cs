using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.EntityCore;
using MiniJam159.StructureCore;
using MiniJam159.UnitCore;



namespace MiniJam159.Entities
{
    public class EntityManager : EntityManagerBase
    {

        public override List<GameObject> getEntityObjects() { return entityObjects; }
        public override List<GameObject> getUnitObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<UnitBase>()); }
        public override List<GameObject> getStructureObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<Structure>()); }
        public override List<GameObject> getPlayerEntityObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<Entity>()?.playerOwned == true); }
        public override List<GameObject> getPlayerUnitObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<UnitBase>()?.playerOwned == true); }
        public override List<GameObject> getPlayerStructureObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<Structure>()?.playerOwned == true); }
        public override List<GameObject> getNonPlayerEntityObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<Entity>()?.playerOwned == false); }
        public override List<GameObject> getNonPlayerUnitObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<UnitBase>()?.playerOwned == false); }
        public override List<GameObject> getNonPlayerStructureObjects() { return entityObjects.FindAll(entityObject => entityObject.GetComponent<Structure>()?.playerOwned == false); }

        public override GameObject CreateEntity(EntityCreationData entityCreationData)
        {
            // Create new object from prefab
            GameObject newObject = Instantiate(entityCreationData.prefab, entityCreationData.position, entityCreationData.rotation);
            newObject.GetComponent<Entity>().playerOwned = entityCreationData.playerOwned;

            entityObjects.Add(newObject);
            return newObject;
        }

        /* DEBUG
        public SerializedTrainingDictionary trainingDictionary;
        public GameObject testPrefab;
        public Vector3 testPosition;
        public Vector3 testSpawnOffset;
        public Quaternion testQuaternion;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                EntityCreationData newEntityCreationData = new EntityCreationData();
                newEntityCreationData.prefab = trainingDictionary.data["Worker"].prefab; //testPrefab;
                newEntityCreationData.position = testPosition + testSpawnOffset;
                newEntityCreationData.rotation = testQuaternion;
                newEntityCreationData.targetPosition = newEntityCreationData.position;
                newEntityCreationData.playerOwned = true;

                CreateEntity(newEntityCreationData);
            }
        }
        */
    }
}
