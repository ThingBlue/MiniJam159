using MiniJam159.GameCore;
using MiniJam159.Structures;
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

        public GameObject structurePrefab;

        #endregion

        public List<GameObject> structures;

        private Structure placementStructure;
        public bool inPlacementMode = false;
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
            // Toggle grid guides
            if (previousInPlacementMode != inPlacementMode)
            {
                gridTilesRenderer.enabled = inPlacementMode ? true : false;
                placementGuideRenderer.enabled = inPlacementMode ? true : false;
                foreach (GameObject structure in structures)
                {
                    structure.transform.Find("BlockedTiles").GetComponent<MeshRenderer>().enabled = inPlacementMode ? true : false;
                }

                previousInPlacementMode = inPlacementMode;
            }

            if (inPlacementMode)
            {
                Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

                gridTilesMaterial.SetVector("_PlacementPosition", mousePosition);

                // Transform placement guide
                placementGuide.transform.localScale = new Vector3(placementStructure.size.x / 10.0f, 1, placementStructure.size.y / 10.0f);

                Vector3 roundedPosition = mousePosition;
                roundedPosition.x = Mathf.Round(mousePosition.x);
                roundedPosition.z = Mathf.Round(mousePosition.z);

                Vector3 snappedPosition = roundedPosition;
                if (placementStructure.size.x % 2 == 1) snappedPosition.x += 0.5f;
                if (placementStructure.size.y % 2 == 1) snappedPosition.z += 0.5f;
                placementGuide.transform.SetPositionAndRotation(snappedPosition, Quaternion.identity);
            }
        }

        public void beginPlacement(Structure structure)
        {
            placementStructure = structure;
            inPlacementMode = true;
        }

        public void finishPlacement()
        {
            // Get start position
            Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

            Vector3 roundedPosition = mousePosition;
            roundedPosition.x = Mathf.Round(mousePosition.x);
            roundedPosition.z = Mathf.Round(mousePosition.z);

            Vector3 snappedPosition = roundedPosition;
            if (placementStructure.size.x % 2 == 1) snappedPosition.x += 0.5f;
            if (placementStructure.size.y % 2 == 1) snappedPosition.z += 0.5f;

            // Check for blocked tiles
            Vector2 startPosition = new Vector2(
                roundedPosition.x - Mathf.Floor(placementStructure.size.x / 2.0f),
                roundedPosition.z - Mathf.Floor(placementStructure.size.y / 2.0f)
            );

            // Check if placement location is valid
            if (!placementBlocked(startPosition))
            {
                // Complete placement
                inPlacementMode = false;
                GridManager.instance.occupyCells(startPosition, placementStructure.size);

                // Instantiate strucutre
                GameObject newStructure = Instantiate(structurePrefab, new Vector3(snappedPosition.x, 0.5f, snappedPosition.z), Quaternion.identity);
                GameObject newBlockedTilesObject = newStructure.transform.Find("BlockedTiles").gameObject;

                // Set scale
                newBlockedTilesObject.transform.localScale = new Vector3(placementStructure.size.x / 10.0f, 1, placementStructure.size.y / 10.0f);

                // Create duplicate material to fix shader graph weirdness
                Renderer renderer = newBlockedTilesObject.GetComponent<MeshRenderer>();
                renderer.material = new Material(renderer.material);

                structures.Add(newStructure);
            }
            else
            {
                Debug.Log("Blocked");
            }
        }

        public void cancelPlacement()
        {
            inPlacementMode = false;
        }

        private bool placementBlocked(Vector2 startPosition)
        {
            for (int i = 0; i < placementStructure.size.x; i++)
            {
                for (int j = 0; j < placementStructure.size.y; j++)
                {
                    if ((int)startPosition.x + i < 0 || (int)startPosition.y + j < 0 ||
                        (int)startPosition.x + i >= 20 || (int)startPosition.y + j >= 20 ||
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
