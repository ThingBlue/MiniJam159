using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.EntityCore;
using MiniJam159.StructureCore;
using MiniJam159.UnitCore;



namespace MiniJam159.Entity
{
    public class EntityManager : EntityManagerBase
    {
        public override void CreateEntity(EntityCreationData entityCreationData)
        {
            // Create new object from prefab
            GameObject newObject = Instantiate(entityCreationData.prefab, entityCreationData.position, entityCreationData.rotation);

            // Add to correct list
            if (entityCreationData.playerOwned)
            {
                if (newObject.GetComponent<UnitBase>())
                {
                    playerUnitObjects.Add(newObject);
                }
                else if (newObject.GetComponent<Structure>())
                {
                    playerStructureObjects.Add(newObject);
                }
                playerEntityObjects.Add(newObject);
            }
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
