using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.Structures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159
{
    public class StructureManager : MonoBehaviour
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

        public StructureDataList structureDataList;

        #endregion

        public List<GameObject> structures;

        private StructureData placementStructureData;
        private bool previousInPlacementMode = false;

        // Singleton
        public static StructureManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.buildNestCommandEvent.AddListener(onBuildNestCommandCallback);
            EventManager.instance.buildWombCommandEvent.AddListener(onBuildWombCommandCallback);
        }

        private void FixedUpdate()
        {
            bool inPlacementMode = (PlayerModeManager.instance.playerMode == PlayerMode.STRUCTURE_PLACEMENT);
            // Toggle grid guides
            if (previousInPlacementMode != inPlacementMode)
            {
                gridTilesRenderer.enabled = inPlacementMode ? true : false;
                placementGuideRenderer.enabled = inPlacementMode ? true : false;
                foreach (GameObject structure in structures)
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
                placementGuide.transform.localScale = new Vector3(placementStructureData.size.x / 10.0f, 1, placementStructureData.size.z / 10.0f);

                Vector3 roundedPosition = mousePosition;
                roundedPosition.x = Mathf.Round(mousePosition.x);
                roundedPosition.z = Mathf.Round(mousePosition.z);

                Vector3 snappedPosition = roundedPosition;
                if (placementStructureData.size.x % 2 == 1) snappedPosition.x += 0.5f;
                if (placementStructureData.size.z % 2 == 1) snappedPosition.z += 0.5f;
                placementGuide.transform.SetPositionAndRotation(snappedPosition, Quaternion.identity);

                // Check if placement is outside of the grid
                Vector3 startPosition = new Vector3(
                    roundedPosition.x - Mathf.Floor(placementStructureData.size.x / 2.0f),
                    0,
                    roundedPosition.z - Mathf.Floor(placementStructureData.size.z / 2.0f)
                );
                Vector3 endPosition = startPosition + placementStructureData.size;
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

        public void beginPlacement(StructureData structureData)
        {
            placementStructureData = new StructureData(structureData);
            PlayerModeManager.instance.playerMode = PlayerMode.STRUCTURE_PLACEMENT;
        }

        public GameObject finishPlacement()
        {
            // Get start position
            Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

            Vector3 roundedPosition = mousePosition;
            roundedPosition.x = Mathf.Round(mousePosition.x);
            roundedPosition.z = Mathf.Round(mousePosition.z);

            Vector3 snappedPosition = roundedPosition;
            if (placementStructureData.size.x % 2 == 1) snappedPosition.x += 0.5f;
            if (placementStructureData.size.z % 2 == 1) snappedPosition.z += 0.5f;

            // Check for blocked tiles
            Vector3 startPosition = new Vector3(
                roundedPosition.x - Mathf.Floor(placementStructureData.size.x / 2.0f),
                0,
                roundedPosition.z - Mathf.Floor(placementStructureData.size.z / 2.0f)
            );

            // Check if placement location is valid
            if (!isPlacementBlocked(startPosition))
            {
                // Complete placement
                PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
                GridManager.instance.occupyCells(startPosition, placementStructureData.size, CellType.BUILDING);

                // Instantiate strucutre
                GameObject newStructureObject = null;
                switch (placementStructureData.structureType)
                {
                    case StructureType.NEST:
                        newStructureObject = Instantiate(nestStructurePrefab, new Vector3(snappedPosition.x, 0.5f, snappedPosition.z), Quaternion.identity);
                        break;
                    case StructureType.WOMB:
                        newStructureObject = Instantiate(wombStructurePrefab, new Vector3(snappedPosition.x, 0.5f, snappedPosition.z), Quaternion.identity);
                        break;
                    case StructureType.NULL:
                        newStructureObject = Instantiate(testStructurePrefab, new Vector3(snappedPosition.x, 0.5f, snappedPosition.z), Quaternion.identity);
                        break;
                }

                GameObject newBlockedTilesObject = newStructureObject.transform.Find("BlockedTiles").gameObject;

                // Set scale of blocked tiles
                newBlockedTilesObject.transform.localScale = new Vector3(placementStructureData.size.x / 10.0f, 1, placementStructureData.size.z / 10.0f);

                // Create duplicate material to fix shader graph weirdness
                Renderer renderer = newBlockedTilesObject.GetComponent<MeshRenderer>();
                renderer.material = new Material(renderer.material);

                // Set properties in structure data class
                newStructureObject.GetComponent<Structure>().structureData = new StructureData(placementStructureData);

                structures.Add(newStructureObject);

                return newStructureObject;
            }
            else
            {
                Debug.Log("Blocked");
                return null;
            }
        }

        public void cancelPlacement()
        {
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        private bool isPlacementBlocked(Vector3 startPosition)
        {
            for (int i = 0; i < placementStructureData.size.x; i++)
            {
                for (int j = 0; j < placementStructureData.size.z; j++)
                {
                    if ((int)startPosition.x + i < 0 || (int)startPosition.z + j < 0 ||
                        (int)startPosition.x + i >= GridManager.instance.mapXLength || (int)startPosition.z + j >= GridManager.instance.mapZLength ||
                        GridManager.instance.isCellOccupied((int)startPosition.x + i, (int)startPosition.z + j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Command callbacks

        private void onBuildNestCommandCallback()
        {
            beginPlacement(structureDataList.nestStructureData);
        }

        private void onBuildWombCommandCallback()
        {
            beginPlacement(structureDataList.wombStructureData);
        }

        #endregion


    }
}
