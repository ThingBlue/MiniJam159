using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using MiniJam159.Player;
using MiniJam159.PlayerCore;
using MiniJam159.UICore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159.UI
{
    public class SelectionDisplayManager : SelectionDisplayManagerBase
    {
        #region Inspector members

        public GameObject selectionDisplayPanelObject;

        public GameObject displayBoxPrefab;
        public float displayCenterHeight;
        public float displayBoxDefaultSize;
        public float displayBoxHoveredSize;

        #endregion

        protected void Start()
        {
            // Hide display panel at the start
            selectionDisplayPanelObject.SetActive(false);
        }

        protected virtual void Update()
        {
            /*
            // Calculate display panel background size and position
            RectTransform displayPanelTransform = selectionDisplayPanel.GetComponent<RectTransform>();

            float minimapPanelWidth = minimapPanel.GetComponent<RectTransform>().sizeDelta.x;
            float commandPanelWidth = commandPanel.GetComponent<RectTransform>().sizeDelta.x;

            float newWidth = Screen.width - minimapPanelWidth - commandPanelWidth;
            float newPosition = minimapPanelWidth + ((Screen.width - commandPanelWidth) - minimapPanelWidth) / 2f - (Screen.width / 2f);
            displayPanelTransform.localPosition = new Vector3(newPosition, displayPanelTransform.localPosition.y, 0f);
            displayPanelTransform.sizeDelta = new Vector2(newWidth, displayPanelTransform.sizeDelta.y);
            */

            // Check if an update is required for display boxes
            if (InputManager.instance.getKeyDown("TypeSelect") || InputManager.instance.getKeyDown("Deselect") ||
                InputManager.instance.getKeyUp("TypeSelect") || InputManager.instance.getKeyUp("Deselect"))
            {
                updateSelectionDisplayBoxes();
            }
        }

        public override void showSelectionDisplayBoxes()
        {
            List<GameObject> selectedObjects = SelectionManager.instance.selectedObjects;
            if (selectedObjects.Count == 0)
            {
                // Hide display panel
                selectionDisplayPanelObject.SetActive(false);
                return;
            }

            // Show display panel
            selectionDisplayPanelObject.SetActive(true);

            // Calculate rows and columns
            float displayPanelWidth = selectionDisplayPanelObject.GetComponent<RectTransform>().rect.size.x;
            displayPanelWidth -= (displayBoxDefaultSize * 2);

            // Don't create display boxes if the screen is too thin to fit display panel
            int columns = displayPanelWidth > 0 ?  Mathf.FloorToInt(displayPanelWidth / displayBoxDefaultSize) : 0;
            int rows = displayPanelWidth > 0 ? Mathf.CeilToInt((float)selectedObjects.Count / (float)columns) : 0;

            // Create all buttons
            for (int r = 0; r < rows; r++)
            {
                List<GameObject> row = new List<GameObject>();
                for (int c = 0; c < columns; c++)
                {
                    int selectedIndex = (r * columns) + c;

                    // Check if we're done
                    if (selectedIndex >= selectedObjects.Count) break;

                    // Create box
                    GameObject newDisplayBox = Instantiate(displayBoxPrefab, selectionDisplayPanelObject.transform);
                    newDisplayBox.GetComponent<Image>().sprite = selectedObjects[selectedIndex].GetComponent<Entity>().displayIcon;

                    // Set up button
                    SelectionDisplayButton newDisplayButton = newDisplayBox.GetComponent<SelectionDisplayButton>();
                    newDisplayButton.selectedObjectName = selectedObjects[selectedIndex].name;
                    newDisplayButton.selectedIndex = selectedIndex;
                    newDisplayButton.defaultSize = displayBoxDefaultSize;
                    newDisplayButton.hoveredSize = displayBoxHoveredSize;
                    newDisplayBox.GetComponent<Button>().onClick.AddListener(() => onSelectionDisplayBoxClicked(newDisplayButton.selectedIndex));

                    row.Add(newDisplayBox);
                }
                selectionDisplayBoxes.Add(row);
            }

            // Set starting positions for each box
            updateSelectionDisplayBoxes(true, true);
        }

        // doPositionUpdate false means we skip setting position and target position of display boxes, and only update frame colour
        // setPosition flag is only true for initialization to immediately set target position of boxes
        public override void updateSelectionDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false)
        {
            // Get key statuses
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool typeSelectKey = InputManager.instance.getKey("TypeSelect");

            // First pass to gather information
            List<float> rowWidths = new List<float>();
            List<float> rowYPositions = new List<float>();
            SelectionDisplayButton hoveredDisplayButton = null;
            int hoveredRow = -1;

            // Loop through rows
            for (int r = 0; r < selectionDisplayBoxes.Count; r++)
            {
                float rowWidth = 0;

                // Loop through columns
                for (int c = 0; c < selectionDisplayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = selectionDisplayBoxes[r][c].GetComponent<SelectionDisplayButton>();

                    // Check for hovered status (There should only be 1 hovered box)
                    if (hoveredDisplayButton == null && displayButton.hovered)
                    {
                        hoveredDisplayButton = displayButton;
                        hoveredRow = r;
                    }

                    // Do calculations for row width
                    rowWidth += displayButton.hovered ? displayBoxHoveredSize : displayBoxDefaultSize;
                }

                rowWidths.Add(rowWidth);

                // Initialize default row y positions
                float rowYPosition = r > 0 ? rowYPositions[rowYPositions.Count - 1] - displayBoxDefaultSize : displayCenterHeight;
                rowYPositions.Add(rowYPosition);
            }

            // Modify y position based on relative position of hovered row
            if (hoveredRow != -1)
            {
                for (int r = 0; r < rowYPositions.Count; r++)
                {
                    float sizeDifference = selectionDisplayBoxes[r][0].GetComponent<SelectionDisplayButton>().hoveredSize - displayBoxDefaultSize;
                    if (r < hoveredRow) rowYPositions[r] += sizeDifference / 2f;
                    if (r > hoveredRow) rowYPositions[r] -= sizeDifference / 2f;
                }
            }

            // Second pass to set positions/sizes/frame colours
            // Loop through rows
            for (int r = 0; r < selectionDisplayBoxes.Count; r++)
            {
                // Loop through columns
                float currentPosition = 0f - (rowWidths[r] / 2f);
                for (int c = 0; c < selectionDisplayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = selectionDisplayBoxes[r][c].GetComponent<SelectionDisplayButton>();

                    // Do position update
                    if (doPositionUpdate)
                    {
                        // Calculate box position
                        Vector3 boxLocalPosition = new Vector3(currentPosition, 0f, 0f);
                        boxLocalPosition.x += (displayButton.hovered ? displayBoxHoveredSize : displayBoxDefaultSize) / 2f;
                        boxLocalPosition.y = rowYPositions[r];

                        // Update current position
                        currentPosition += displayButton.hovered ? displayBoxHoveredSize : displayBoxDefaultSize;

                        // Set target position
                        RectTransform boxTransform = selectionDisplayBoxes[r][c].GetComponent<RectTransform>();
                        displayButton.targetLocalPosition = boxLocalPosition;

                        // Immediately set position if flag is true
                        if (setPosition) boxTransform.localPosition = boxLocalPosition;
                    }

                    // Set frame colour based on focus status
                    Entity displayButtonEntity = SelectionManager.instance.selectedObjects[displayButton.selectedIndex].GetComponent<Entity>();
                    if (displayButtonEntity.sortPriority == SelectionManager.instance.focusSortPriority)
                    {
                        displayButton.setFrameColour(HoverStatus.FOCUS);
                    }
                    else displayButton.setFrameColour(HoverStatus.DEFAULT);

                    // Set selection status of all boxes of the same sorting priority
                    if (hoveredDisplayButton != null)
                    {
                        Entity hoveredEntity = SelectionManager.instance.selectedObjects[hoveredDisplayButton.selectedIndex].GetComponent<Entity>();

                        // Single reselect
                        if (!deselectKey && !typeSelectKey && displayButton.hovered)
                        {
                            displayButton.setFrameColour(HoverStatus.HOVER);
                        }
                        // Single deselect
                        else if (deselectKey && !typeSelectKey && displayButton.hovered)
                        {
                            displayButton.setFrameColour(HoverStatus.REMOVE);
                        }
                        // Type reselect
                        else if (!deselectKey && typeSelectKey && displayButtonEntity.sortPriority == hoveredEntity.sortPriority)
                        {
                            displayButton.setFrameColour(HoverStatus.HOVER);
                        }
                        // Type deselect
                        else if (deselectKey && typeSelectKey && displayButtonEntity.sortPriority == hoveredEntity.sortPriority)
                        {
                            displayButton.setFrameColour(HoverStatus.REMOVE);
                        }
                    }

                }
            }

        }

        public override void clearSelectionDisplayBoxes()
        {
            foreach (List<GameObject> row in selectionDisplayBoxes)
            {
                foreach (GameObject displayBox in row)
                {
                    Destroy(displayBox);
                }
                row.Clear();
            }
            selectionDisplayBoxes.Clear();
        }

        public override void onSelectionDisplayBoxClicked(int index)
        {
            // Clear commands
            CommandManagerBase.instance.clearCommands();

            // Select new object
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool typeSelectKey = InputManager.instance.getKey("TypeSelect");

            // Deselect type
            if (deselectKey && typeSelectKey) SelectionControllerBase.instance.deselectType(index);
            // Deselect single
            else if (deselectKey) SelectionControllerBase.instance.deselectSingle(index);
            // Reselect type
            else if (typeSelectKey) SelectionControllerBase.instance.reselectType(index);
            // Set focus
            else if (SelectionManager.instance.getSortPriorityWithIndex(index) != SelectionManager.instance.focusSortPriority)
            {
                SelectionManager.instance.focusSortPriority = SelectionManager.instance.getSortPriorityWithIndex(index);
                SelectionControllerBase.instance.populateCommands();

                // Update display boxes
                updateSelectionDisplayBoxes(false);
            }
            // Reselect single if already focused
            else SelectionControllerBase.instance.reselectSingle(index);
        }
    }
}
