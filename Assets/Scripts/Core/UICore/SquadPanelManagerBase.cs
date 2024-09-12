using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace MiniJam159.UICore
{
    public class SquadPanelManagerBase : MonoBehaviour
    {
        #region Inspector members

        public GameObject squadPanelObject;
        public GameObject unbindBox;

        public GameObject squadDisplayBoxPrefab;

        public List<GameObject> squadBindBoxes;
        public GameObject squadDeleteBox;

        public float topRowYPosition;
        public Vector2 squadDisplayBoxSize;
        public int squadDisplayBoxRowCount;
        public int squadDisplayBoxColumnCount;

        #endregion

        public List<GameObject> squadDisplayBoxes;

        // Singleton
        public static SquadPanelManagerBase instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public GameObject createSquadDisplayBox(Squad squad)
        {
            // Create new box and add to list
            GameObject newSquadDisplayBox = Instantiate(squadDisplayBoxPrefab, squadPanelObject.transform);
            newSquadDisplayBox.GetComponent<SquadDisplayBox>().squad = squad;
            squadDisplayBoxes.Add(newSquadDisplayBox);

            // Update positions of all boxes
            updateUnboundBoxes();

            return newSquadDisplayBox;
        }

        // Gets the display box matching given squad
        // Returns null if none found
        public GameObject getSquadDisplayBox(Squad squad)
        {
            foreach (GameObject squadDisplayBoxObject in squadDisplayBoxes)
            {
                if (squadDisplayBoxObject.GetComponent<SquadDisplayBox>().squad == squad) return squadDisplayBoxObject;
            }
            return null;
        }

        public void updateUnboundBoxes()
        {
            // Get unbound boxes
            List<GameObject> unboundBoxes = new List<GameObject>();
            foreach (GameObject boxObject in squadDisplayBoxes)
            {
                Squad squad = boxObject.GetComponent<SquadDisplayBox>().squad;
                if (!SelectionManager.instance.boundSquads.Contains(squad)) unboundBoxes.Add(boxObject);
            }

            // Calculate starting x position
            float leftBoxXPosition = -(squadDisplayBoxColumnCount * squadDisplayBoxSize.x);

            // Set positions for each box
            for (int i = 0; i < unboundBoxes.Count; i++)
            {
                unboundBoxes[i].GetComponent<RectTransform>().localPosition = new Vector3(
                    leftBoxXPosition + (i % squadDisplayBoxColumnCount) * squadDisplayBoxSize.x,
                    topRowYPosition - Mathf.Floor(i / squadDisplayBoxColumnCount) * squadDisplayBoxSize.y,
                    0
                );
            }
        }

        public void toggleUnbindBox(bool enable)
        {
            unbindBox.GetComponent<CanvasGroup>().blocksRaycasts = enable;
        }

        // Return values:
        //     0 - 7 = Corresponding squad slot
        //     -1 = Nothing found
        //     -2 = Unbind
        //     -3 = Delete
        public int getDropTarget(out GameObject targetObject)
        {
            // Default to null out object
            targetObject = null;

            // Raycast on UI layer
            List<GameObject> mouseRaycastResult = InputManager.instance.mouseRaycastAllUI();

            // Bind boxes
            for (int i = 0; i < squadBindBoxes.Count; i++)
            {
                if (mouseRaycastResult.Contains(squadBindBoxes[i]))
                {
                    targetObject = squadBindBoxes[i];
                    return i;
                }
            }

            // Unbind box
            if (mouseRaycastResult.Contains(unbindBox)) return -2;

            // Deletion box
            if (mouseRaycastResult.Contains(squadDeleteBox)) return -3;

            // Nothing
            return -1;
        }
    }
}
