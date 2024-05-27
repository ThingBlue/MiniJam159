using MiniJam159.GameCore;
using MiniJam159.Structures;
using MiniJam159.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;

namespace MiniJam159
{
    public class StructureManager : MonoBehaviour
    {
        #region Inspector memberes

        public GameObject placementGuide;

        public MeshRenderer gridTilesRenderer;
        public MeshRenderer placementGuideRenderer;

        public Material gridTilesMaterial;
        public Material placementGuideMaterial;
        public Material blockedTilesMaterial;

        public GameObject structurePrefab;

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

            if (inPlacementMode)
            {
                Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

                gridTilesMaterial.SetVector("_PlacementPosition", mousePosition);

                // Transform placement guide
                placementGuide.transform.localScale = new Vector3(placementStructureData.size.x / 10.0f, 1, placementStructureData.size.y / 10.0f);

                Vector3 roundedPosition = mousePosition;
                roundedPosition.x = Mathf.Round(mousePosition.x);
                roundedPosition.z = Mathf.Round(mousePosition.z);

                Vector3 snappedPosition = roundedPosition;
                if (placementStructureData.size.x % 2 == 1) snappedPosition.x += 0.5f;
                if (placementStructureData.size.y % 2 == 1) snappedPosition.z += 0.5f;
                placementGuide.transform.SetPositionAndRotation(snappedPosition, Quaternion.identity);

                // Check if placement is outside of the grid
                Vector2 startPosition = new Vector2(
                    roundedPosition.x - Mathf.Floor(placementStructureData.size.x / 2.0f),
                    roundedPosition.z - Mathf.Floor(placementStructureData.size.y / 2.0f)
                );
                Vector2 endPosition = startPosition + placementStructureData.size;
                if (startPosition.x < 0 || startPosition.y < 0 ||
                    endPosition.x > GridManager.instance.mapXLength || endPosition.y > GridManager.instance.mapZLength)
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
            placementStructureData = structureData;
            placementStructureData.commands = new List<CommandType>(structureData.commands);
            placementStructureData.displaySprite = structureData.displaySprite;
            PlayerModeManager.instance.playerMode = PlayerMode.STRUCTURE_PLACEMENT;
        }

        public void finishPlacement()
        {
            // Get start position
            Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

            Vector3 roundedPosition = mousePosition;
            roundedPosition.x = Mathf.Round(mousePosition.x);
            roundedPosition.z = Mathf.Round(mousePosition.z);

            Vector3 snappedPosition = roundedPosition;
            if (placementStructureData.size.x % 2 == 1) snappedPosition.x += 0.5f;
            if (placementStructureData.size.y % 2 == 1) snappedPosition.z += 0.5f;

            // Check for blocked tiles
            Vector2 startPosition = new Vector2(
                roundedPosition.x - Mathf.Floor(placementStructureData.size.x / 2.0f),
                roundedPosition.z - Mathf.Floor(placementStructureData.size.y / 2.0f)
            );

            // Check if placement location is valid
            if (!placementBlocked(startPosition))
            {
                // Complete placement
                PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
                GridManager.instance.occupyCells(startPosition, placementStructureData.size);

                // Instantiate strucutre
                GameObject newStructureObject = Instantiate(structurePrefab, new Vector3(snappedPosition.x, 0.5f, snappedPosition.z), Quaternion.identity);
                GameObject newBlockedTilesObject = newStructureObject.transform.Find("BlockedTiles").gameObject;

                // Set scale
                newBlockedTilesObject.transform.localScale = new Vector3(placementStructureData.size.x / 10.0f, 1, placementStructureData.size.y / 10.0f);

                // Create duplicate material to fix shader graph weirdness
                Renderer renderer = newBlockedTilesObject.GetComponent<MeshRenderer>();
                renderer.material = new Material(renderer.material);

                // Set properties in structure data class
                StructureData newStructureData = newStructureObject.GetComponent<Structure>().structureData;
                newStructureData.position = new Vector2(snappedPosition.x, snappedPosition.z);
                newStructureData.size = placementStructureData.size;
                newStructureData.commands = new List<CommandType>(placementStructureData.commands);
                newStructureData.displaySprite = placementStructureData.displaySprite;

                structures.Add(newStructureObject);
            }
            else
            {
                Debug.Log("Blocked");
            }
        }

        public void cancelPlacement()
        {
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        private bool placementBlocked(Vector2 startPosition)
        {
            for (int i = 0; i < placementStructureData.size.x; i++)
            {
                for (int j = 0; j < placementStructureData.size.y; j++)
                {
                    if ((int)startPosition.x + i < 0 || (int)startPosition.y + j < 0 ||
                        (int)startPosition.x + i >= GridManager.instance.mapXLength || (int)startPosition.y + j >= GridManager.instance.mapXLength ||
                        GridManager.instance.isCellOccupied((int)startPosition.x + i, (int)startPosition.y + j))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
