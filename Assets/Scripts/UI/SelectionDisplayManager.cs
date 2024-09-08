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
        public override void showSelectionDisplayBoxes()
        {
            List<GameObject> selectedObjects = SelectionManager.instance.selectedObjects;
            if (selectedObjects.Count == 0)
            {
                // Hide display panel
                selectionDisplayPanel.SetActive(false);
                return;
            }

            // Show display panel
            selectionDisplayPanel.SetActive(true);

            // Calculate rows and columns
            float displayPanelWidth = selectionDisplayPanel.GetComponent<RectTransform>().rect.size.x;
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
                    GameObject newDisplayBox = Instantiate(displayBoxPrefab, selectionDisplayPanel.transform);
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
                float rowYPosition = r > 0 ? rowYPositions[rowYPositions.Count - 1] - displayBoxDefaultSize : 0;
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
                        displayButton.setSelectStatus(SelectStatus.FOCUSED);
                    }
                    else displayButton.setSelectStatus(SelectStatus.DEFAULT);

                    // Set selection status of all boxes of the same sorting priority
                    if (hoveredDisplayButton != null)
                    {
                        Entity hoveredEntity = SelectionManager.instance.selectedObjects[hoveredDisplayButton.selectedIndex].GetComponent<Entity>();

                        // Single reselect
                        if (!deselectKey && !typeSelectKey && displayButton.hovered)
                        {
                            displayButton.setSelectStatus(SelectStatus.RESELECT);
                        }
                        // Single deselect
                        else if (deselectKey && !typeSelectKey && displayButton.hovered)
                        {
                            displayButton.setSelectStatus(SelectStatus.DESELECT);
                        }
                        // Type reselect
                        else if (!deselectKey && typeSelectKey && displayButtonEntity.sortPriority == hoveredEntity.sortPriority)
                        {
                            displayButton.setSelectStatus(SelectStatus.RESELECT);
                        }
                        // Type deselect
                        else if (deselectKey && typeSelectKey && displayButtonEntity.sortPriority == hoveredEntity.sortPriority)
                        {
                            displayButton.setSelectStatus(SelectStatus.DESELECT);
                        }
                    }

                }
            }

        }

        public override void onSelectionDisplayBoxClicked(int index)
        {
            // Clear commands
            CommandManagerBase.instance.clearCommands();

            // Select new object
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool typeSelectKey = InputManager.instance.getKey("TypeSelect");

            // Deselect type
            if (deselectKey && typeSelectKey) SelectionController.instance.deselectType(index);
            // Deselect single
            else if (deselectKey) SelectionController.instance.deselectSingle(index);
            // Reselect type
            else if (typeSelectKey) SelectionController.instance.reselectType(index);
            // Set focus
            else if (SelectionManager.instance.getSortPriorityWithIndex(index) != SelectionManager.instance.focusSortPriority)
            {
                SelectionManager.instance.focusSortPriority = SelectionManager.instance.getSortPriorityWithIndex(index);
                SelectionController.instance.populateCommands(index);

                // Update display boxes
                updateSelectionDisplayBoxes(false);
            }
            // Reselect single if already focused
            else SelectionController.instance.reselectSingle(index);
        }
    }
}
