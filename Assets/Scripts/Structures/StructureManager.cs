using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.StructureCore;

namespace MiniJam159.Structures
{
    public class StructureManager : StructureManagerBase
    {
        #region Inspector members

        public GameObject placementGuide;

        public MeshRenderer gridTilesRenderer;
        public MeshRenderer placementGuideRenderer;

        public Material gridTilesMaterial;
        public Material placementGuideMaterial;
        public Material blockedTilesMaterial;

        public GameObject nestStructurePrefab;
        public GameObject wombStructurePrefab;
        public GameObject testStructurePrefab;

        public List<StructureType> depositPointStructureTypes;

        #endregion

        private StructurePlacementData placementData;
        private bool previousInPlacementMode = false;

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.buildNestCommandEvent.AddListener(onBuildNestCommandCallback);
            EventManager.instance.buildWombCommandEvent.AddListener(onBuildWombCommandCallback);
        }

        private void FixedUpdate()
        {
            bool inPlacementMode = (PlayerControllerBase.instance.playerMode == PlayerMode.STRUCTURE_PLACEMENT);
            // Toggle grid guides
            if (previousInPlacementMode != inPlacementMode)
            {
                gridTilesRenderer.enabled = inPlacementMode ? true : false;
                placementGuideRenderer.enabled = inPlacementMode ? true : false;
                foreach (GameObject structure in EntityManager.instance.playerStructureObjects)
                {
                    structure.transform.Find("BlockedTiles").GetComponent<MeshRenderer>().enabled = inPlacementMode ? true : false;
                    //structure.transform.Find("BlockedTiles").GetComponent<MeshRenderer>().enabled = true;
                }

                previousInPlacementMode = inPlacementMode;
            }

            // Update during placement mode
            if (inPlacementMode)
            {
                Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

                gridTilesMaterial.SetVector("_PlacementPosition", mousePosition);

                // Transform placement guide
                placementGuide.transform.localScale = new Vector3(placementData.size.x / 10.0f, 1, placementData.size.z / 10.0f);

                Vector3 roundedPosition = mousePosition;
                roundedPosition.x = Mathf.Round(mousePosition.x);
                roundedPosition.z = Mathf.Round(mousePosition.z);

                Vector3 snappedPosition = roundedPosition;
                if (placementData.size.x % 2 == 1) snappedPosition.x += 0.5f;
                if (placementData.size.z % 2 == 1) snappedPosition.z += 0.5f;
                placementGuide.transform.SetPositionAndRotation(snappedPosition, Quaternion.identity);

                // Check if placement is outside of the grid
                Vector3 startPosition = new Vector3(
                    roundedPosition.x - Mathf.Floor(placementData.size.x / 2.0f),
                    0,
                    roundedPosition.z - Mathf.Floor(placementData.size.z / 2.0f)
                );
                Vector3 endPosition = startPosition + placementData.size;
                if (startPosition.x < 0 || startPosition.z < 0 ||
                    endPosition.x > GridManager.instance.mapXLength || endPosition.z > GridManager.instance.mapZLength)
                {
                    placementGuide.GetComponent<MeshRenderer>().material = blockedTilesMaterial;
                }
                else
                {
                    placementGuide.GetComponent<MeshRenderer>().material = placementGuideMaterial;
                }
            }
        }

        public override void beginPlacement(StructureType structureType, GameObject structurePrefab)
        {
            placementData = new StructurePlacementData(structureType, Vector3.zero, structurePrefab.GetComponent<Structure>().size);

            // Begin placement
            PlayerControllerBase.instance.playerMode = PlayerMode.STRUCTURE_PLACEMENT;
        }

        public override void cancelPlacement()
        {
            PlayerControllerBase.instance.playerMode = PlayerMode.NORMAL;
        }

        public override StructurePlacementData confirmPlacement()
        {
            // Get start position
            Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

            Vector3 roundedPosition = mousePosition;
            roundedPosition.x = Mathf.Round(mousePosition.x);
            roundedPosition.z = Mathf.Round(mousePosition.z);

            Vector3 snappedPosition = roundedPosition;
            if (placementData.size.x % 2 == 1) snappedPosition.x += 0.5f;
            if (placementData.size.z % 2 == 1) snappedPosition.z += 0.5f;

            // Check for blocked tiles
            Vector3 startPosition = new Vector3(
                roundedPosition.x - Mathf.Floor(placementData.size.x / 2.0f),
                0,
                roundedPosition.z - Mathf.Floor(placementData.size.z / 2.0f)
            );

            // Check if placement location is valid
            if (!isPlacementBlocked(startPosition, placementData.size))
            {
                // Complete placement
                PlayerControllerBase.instance.playerMode = PlayerMode.NORMAL;
                placementData.position = new Vector3(snappedPosition.x, 0, snappedPosition.z);
                return placementData;
            }

            // Default to null when structure placement is blocked
            return null;
        }

        private bool isPlacementBlocked(Vector3 startPosition, Vector3 size)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.z; j++)
                {
                    if ((int)startPosition.x + i < 0 || (int)startPosition.z + j < 0 ||
                        (int)startPosition.x + i >= GridManager.instance.mapXLength || (int)startPosition.z + j >= GridManager.instance.mapZLength ||
                        GridManager.instance.isTileOccupied((int)startPosition.x + i, (int)startPosition.z + j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override GameObject createStructure(StructurePlacementData structurePlacementData)
        {
            // Occupy tiles for new structure
            Vector3 startPosition = new Vector3(
                structurePlacementData.position.x - Mathf.Floor(placementData.size.x / 2.0f),
                0,
                structurePlacementData.position.z - Mathf.Floor(placementData.size.z / 2.0f)
            );
            GridManager.instance.occupyTiles(startPosition, structurePlacementData.size, TileType.BUILDING);

            // Instantiate strucutre
            GameObject newStructureObject = null;
            switch (structurePlacementData.structureType)
            {
                case StructureType.NEST:
                    newStructureObject = Instantiate(nestStructurePrefab, structurePlacementData.position, Quaternion.identity);
                    break;
                case StructureType.WOMB:
                    newStructureObject = Instantiate(wombStructurePrefab, structurePlacementData.position, Quaternion.identity);
                    break;
                case StructureType.NULL:
                    newStructureObject = Instantiate(testStructurePrefab, structurePlacementData.position, Quaternion.identity);
                    break;
            }

            GameObject newBlockedTilesObject = newStructureObject.transform.Find("BlockedTiles").gameObject;

            // Set transform of blocked tiles indicator
            newBlockedTilesObject.transform.position = new Vector3(newBlockedTilesObject.transform.position.x, 0, newBlockedTilesObject.transform.position.z);
            newBlockedTilesObject.transform.localScale = new Vector3(structurePlacementData.size.x / 10.0f, 1, structurePlacementData.size.z / 10.0f);

            // Create duplicate material to fix shader graph weirdness
            Renderer renderer = newBlockedTilesObject.GetComponent<MeshRenderer>();
            renderer.material = new Material(renderer.material);

            // Add to structures
            EntityManager.instance.playerStructureObjects.Add(newStructureObject);
            EntityManager.instance.playerEntityObjects.Add(newStructureObject);

            // Add to deposit points if new structure is a deposit point
            if (depositPointStructureTypes.Contains(structurePlacementData.structureType)) depositPointStructures.Add(newStructureObject);

            return newStructureObject;
        }

        #region Command callbacks

        private void onBuildNestCommandCallback()
        {
            beginPlacement(StructureType.NEST, nestStructurePrefab);
        }

        private void onBuildWombCommandCallback()
        {
            beginPlacement(StructureType.WOMB, wombStructurePrefab);
        }

        #endregion


    }
}
