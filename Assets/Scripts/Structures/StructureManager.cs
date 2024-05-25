using MiniJam159.GameCore;
using MiniJam159.Structures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159
{
    public class StructureManager : MonoBehaviour
    {
        #region Inspector memberes

        public Material gridTilesMaterial;

        #endregion

        // Singleton
        public static StructureManager instance;

        public List<Structure> structures;
        public bool inPlacementMode = false;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Update()
        {
            gridTilesMaterial.SetFloat("_Enabled", inPlacementMode ? 1 : 0);

            if (inPlacementMode)
            {
                gridTilesMaterial.SetVector("_PlacementPosition", InputManager.instance.getMousePositionInWorld());

                if (InputManager.instance.getKeyDown("FinishPlacement"))
                {
                    // Check if placement location is valid
                    if (isPlacementPositionValid(InputManager.instance.getMousePositionInWorld()))
                    {
                        // Complete placement
                        inPlacementMode = false;
                    }
                }

                if (InputManager.instance.getKeyDown("CancelPlacement"))
                {
                    // Cancel placement
                    inPlacementMode = false;
                }
            }
        }

        private bool isPlacementPositionValid(Vector2 placementPosition)
        {
            return true;
        }
    }
}
