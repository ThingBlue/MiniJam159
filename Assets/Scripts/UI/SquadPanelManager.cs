using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.UICore;
using MiniJam159.GameCore;
using MiniJam159.PlayerCore;

namespace MiniJam159.UI
{
    public class SquadPanelManager : SquadPanelManagerBase
    {
        #region Inspector members

        public GameObject squadPanelObject;
        public GameObject unbindBox;
        public GameObject deleteBox;

        public GameObject squadDisplayBoxPrefab;

        public Vector2 squadDisplayBoxSize;
        public Vector2 firstBoxPosition;
        public int squadDisplayBoxRowCount;
        public int squadDisplayBoxColumnCount;

        public float panelSmoothTime;
        public float showPosition;
        public float hidePosition;

        #endregion

        protected bool showPanel = false;
        protected float targetPanelPosition;
        protected float panelVelocity;

        protected override void Awake()
        {
            base.Awake();

            // Initialize target position to current position
            targetPanelPosition = hidePosition;
        }

        protected void Update()
        {
            // Handle panel toggle
            if (InputManager.instance.getKeyDown("ToggleSquadPanel")) togglePanel(!showPanel);

            // May need new frame colours for boxes on deselect key state change
            if (InputManager.instance.getKeyDown("Deselect") || InputManager.instance.getKeyUp("Deselect")) updateSquadDisplayBoxes();
        }

        protected void FixedUpdate()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(
                Mathf.SmoothDamp(rectTransform.anchoredPosition.x, targetPanelPosition, ref panelVelocity, panelSmoothTime),
                rectTransform.anchoredPosition.y
            );
        }

        public override GameObject createSquadDisplayBox(Squad squad)
        {
            // Create new box and add to list
            GameObject newSquadDisplayBox = Instantiate(squadDisplayBoxPrefab, squadPanelObject.transform);
            newSquadDisplayBox.GetComponent<SquadDisplayBox>().squad = squad;
            squadDisplayBoxes.Add(newSquadDisplayBox);

            // Update positions of all boxes
            updateSquadDisplayBoxes();

            return newSquadDisplayBox;
        }

        // Gets the display box matching given squad
        // Returns null if none found
        public override GameObject getSquadDisplayBox(Squad squad)
        {
            foreach (GameObject squadDisplayBoxObject in squadDisplayBoxes)
            {
                if (squadDisplayBoxObject.GetComponent<SquadDisplayBox>().squad == squad) return squadDisplayBoxObject;
            }
            return null;
        }

        public override void updateSquadDisplayBoxes()
        {
            // Get unbound boxes and set frame colours
            List<GameObject> unboundBoxes = new List<GameObject>();
            foreach (GameObject boxObject in squadDisplayBoxes)
            {
                // Get unbound boxes
                SquadDisplayBox squadDisplayBox = boxObject.GetComponent<SquadDisplayBox>();
                bool bound = SelectionManager.instance.boundSquads.Contains(squadDisplayBox.squad);
                if (!bound) unboundBoxes.Add(boxObject);

                // Set frame colours
                if (squadDisplayBox.hovered)
                {
                    if (InputManager.instance.getKey("Deselect")) squadDisplayBox.setFrameColour(HoverStatus.REMOVE);
                    else squadDisplayBox.setFrameColour(HoverStatus.HOVER);
                }
                else
                {
                    if (bound) squadDisplayBox.setFrameColour(HoverStatus.FOCUS);
                    else squadDisplayBox.setFrameColour(HoverStatus.DEFAULT);
                }
            }

            // Set positions for each box
            for (int i = 0; i < unboundBoxes.Count; i++)
            {
                unboundBoxes[i].GetComponent<RectTransform>().localPosition = new Vector3(
                    firstBoxPosition.x + (i % squadDisplayBoxColumnCount) * squadDisplayBoxSize.x,
                    firstBoxPosition.y - Mathf.Floor(i / squadDisplayBoxColumnCount) * squadDisplayBoxSize.y,
                    0
                );
            }

            // Set slot box alphas
            for (int i = 0; i < squadSlotBoxes.Count; i++)
            {
                // Show slot box if there's not a squad assigned to that slot
                if (SelectionManager.instance.boundSquads[i] == null) squadSlotBoxes[i].GetComponent<CanvasGroup>().alpha = 1;
                else squadSlotBoxes[i].GetComponent<CanvasGroup>().alpha = 0;
            }
        }

        public override void toggleRaycastBoxes(bool enable)
        {
            unbindBox.GetComponent<CanvasGroup>().blocksRaycasts = enable;
            deleteBox.GetComponent<CanvasGroup>().blocksRaycasts = enable;
        }

        // Return values:
        //     0 - 7 = Corresponding squad slot
        //     -1 = Nothing found
        //     -2 = Unbind
        //     -3 = Delete
        public override int getDropTarget(out GameObject targetObject)
        {
            // Default to null out object
            targetObject = null;

            // Raycast on UI layer
            List<GameObject> mouseRaycastResult = InputManager.instance.mouseRaycastAllUI();

            // Bind boxes
            for (int i = 0; i < squadSlotBoxes.Count; i++)
            {
                if (mouseRaycastResult.Contains(squadSlotBoxes[i]))
                {
                    targetObject = squadSlotBoxes[i];
                    return i;
                }
            }

            // Unbind box
            if (mouseRaycastResult.Contains(unbindBox)) return -2;

            // Deletion box
            if (mouseRaycastResult.Contains(deleteBox)) return -3;

            // Nothing
            return -1;
        }

        public override void togglePanel(bool show)
        {
            targetPanelPosition = show ? showPosition : hidePosition;
            showPanel = show;
        }
    }
}
