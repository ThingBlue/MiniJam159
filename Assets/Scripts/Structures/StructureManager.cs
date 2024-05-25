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

        //public GameObject placementGuide;
        public GameObject freeTilePrefab;
        public GameObject blockedTilePrefab;

        public Material gridTilesMaterial;
        //public Material placementGuideMaterial;

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
        }

        private void FixedUpdate()
        {
            // Toggle grid guides
            gridTilesMaterial.SetFloat("_Enabled", inPlacementMode ? 1 : 0);
            //placementGuideMaterial.SetFloat("_Enabled", inPlacementMode ? 1 : 0);
            //placementGuideMaterial.SetFloat("_Enabled", 0);

            if (inPlacementMode)
            {
                Vector3 mousePosition = InputManager.instance.getMousePositionInWorld();

                gridTilesMaterial.SetVector("_PlacementPosition", mousePosition);

                /*
                // Transform placement guide
                placementGuide.transform.localScale = new Vector3(placementStructure.size.x / 10.0f, 1, placementStructure.size.y / 10.0f);

                Vector3 roundedPosition = mousePosition;
                roundedPosition.x = Mathf.Round(mousePosition.x);
                roundedPosition.z = Mathf.Round(mousePosition.z);

                Vector3 snappedPosition = roundedPosition;
                if (placementStructure.size.x % 2 == 1) snappedPosition.x += 0.5f;
                if (placementStructure.size.y % 2 == 1) snappedPosition.z += 0.5f;
                placementGuide.transform.SetPositionAndRotation(snappedPosition, Quaternion.identity);
                */

                Vector2 startPosition = getStartPosition(mousePosition);
                for (int i = 0; i < placementStructure.size.x; i++)
                {
                    for (int j = 0; j < placementStructure.size.y; j++)
                    {
                        if ((int)startPosition.x + i < 0 || (int)startPosition.y + j < 0 ||
                            (int)startPosition.x + i >= 20 || (int)startPosition.y + j >= 20 ||
                            GridManager.instance.isCellOccupied((int)startPosition.x + i, (int)startPosition.y + j))
                        {
                            // Create red indicator at blocked tile
                            GameObject.Instantiate(blockedTilePrefab, new Vector3(startPosition.x + i + 0.5f, 0, startPosition.y + j + 0.5f), Quaternion.identity);
                        }
                        else
                        {
                            // Create green indicator at blocked tile
                            GameObject.Instantiate(freeTilePrefab, new Vector3(startPosition.x + i + 0.5f, 0, startPosition.y + j + 0.5f), Quaternion.identity);
                        }
                    }
                }
            }
            else
            {

            }



            // Input
            if (InputManager.instance.getKeyDown("FinishPlacement"))
            {
            }

            if (InputManager.instance.getKeyDown("CancelPlacement"))
            {
            }
        }

        public void beginPlacement(Structure structure)
        {
            placementStructure = structure;
            inPlacementMode = true;
        }

        public void finishPlacement()
        {
            if (!inPlacementMode) return;

            // Check if placement location is valid
            Vector2 startPosition = getStartPosition(InputManager.instance.getMousePositionInWorld());
            if (!checkBlocked(startPosition))
            {
                // Complete placement
                inPlacementMode = false;
                GridManager.instance.occupyCells(startPosition, placementStructure.size);
            }
            else
            {
                Debug.Log("Blocked");
            }
        }

        public void cancelPlacement()
        {
            if (!inPlacementMode) return;

            // Cancel placement
            inPlacementMode = false;
        }

        private Vector2 getStartPosition(Vector3 mousePosition)
        {
            Vector3 roundedPosition = mousePosition;
            roundedPosition.x = Mathf.Round(mousePosition.x);
            roundedPosition.z = Mathf.Round(mousePosition.z);

            Vector3 snappedPosition = roundedPosition;
            if (placementStructure.size.x % 2 == 1) snappedPosition.x += 0.5f;
            if (placementStructure.size.y % 2 == 1) snappedPosition.z += 0.5f;

            Vector2 startPosition = new Vector2(
                roundedPosition.x - Mathf.Floor(placementStructure.size.x / 2.0f),
                roundedPosition.z - Mathf.Floor(placementStructure.size.y / 2.0f)
            );

            return startPosition;
        }

        private bool checkBlocked(Vector2 startPosition)
        {
            bool blocked = false;
            for (int i = 0; i < placementStructure.size.x; i++)
            {
                for (int j = 0; j < placementStructure.size.y; j++)
                {
                    if ((int)startPosition.x + i < 0 || (int)startPosition.y + j < 0 ||
                        (int)startPosition.x + i >= 20 || (int)startPosition.y + j >= 20 ||
                        GridManager.instance.isCellOccupied((int)startPosition.x + i, (int)startPosition.y + j))
                    {
                        blocked = true;

                        // Create red indicator at blocked tile
                        GameObject.Instantiate(blockedTilePrefab, new Vector3(startPosition.x + i + 0.5f, 0, startPosition.y + j + 0.5f), Quaternion.identity);
                    }
                }
            }
            return blocked;
        }
    }
}
