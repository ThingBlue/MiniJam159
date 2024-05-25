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
        public Material blockedTilesMaterial;

        public GameObject structurePrefab;

        #endregion

        // Singleton
        public static StructureManager instance;

        public List<Structure> structures;
        public bool inPlacementMode = false;
        private Structure placementStructure;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Update()
        {
            // DEBUG DEBUG DEBUG TEST TEST TEST
            if (InputManager.instance.getKeyDown("PlacementTest"))
            {
                Structure newStructure = new Structure();
                newStructure.size = new Vector2(2, 3);
                beginPlacement(newStructure);
            }

            // Toggle grid guides
            gridTilesRenderer.enabled = inPlacementMode ? true : false;
            placementGuideRenderer.enabled = inPlacementMode ? true : false;
            blockedTilesMaterial.SetFloat("_Enabled", inPlacementMode ? 1 : 0);

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
                GameObject newStructure = GameObject.Instantiate(structurePrefab, new Vector3(snappedPosition.x, 0.5f, snappedPosition.z), Quaternion.identity);
                newStructure.transform.localScale = new Vector3(placementStructure.size.x, 1, placementStructure.size.y);
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
