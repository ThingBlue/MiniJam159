using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MiniJam159.AICore;
using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using MiniJam159.Player;
using MiniJam159.PlayerCore;
using MiniJam159.Structures;
using PlasticPipe.PlasticProtocol.Messages;

namespace MiniJam159.UI
{
    public class UIManager : MonoBehaviour
    {
        #region Inspector members

        public GameObject commandPanel;

        public GameObject commandButtonPrefab;

        public Sprite moveCommandSprite;
        public Sprite attackCommandSprite;
        public Sprite holdCommandSprite;
        public Sprite harvestCommandSprite;
        public Sprite openBuildMenuCommandSprite;
        public Sprite cancelBuildMenuCommandSprite;

        public Sprite buildNestCommandSprite;
        public Sprite buildWombCommandSprite;

        public GameObject displayPanel;
        public GameObject displayBoxPrefab;
        public float displayCenterHeight;
        public float displayBoxDefaultSize;
        public float displayBoxHoveredSize;

        #endregion

        public List<GameObject> commandButtons;
        public List<List<GameObject>> displayBoxes;

        // Singleton
        public static UIManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // Intialize lists
            commandButtons = new List<GameObject>();
            displayBoxes = new List<List<GameObject>>();

            // Hide display panel at the start
            displayPanel.SetActive(false);
        }

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.selectionStartEvent.AddListener(onSelectionStartCallback);
            EventManager.instance.selectionSortedEvent.AddListener(onSelectionSortedCallback);
            EventManager.instance.populateCommandsStartEvent.AddListener(onPopulateCommandsStartCallback);
            EventManager.instance.populateCommandsCompleteEvent.AddListener(onPopulateCommandsCompleteCallback);
            EventManager.instance.setFocusCompleteEvent.AddListener(onSetFocusCompleteCallback);
        }

        private void Update()
        {
            // Calculate display panel background size and position
            RectTransform displayPanelTransform = displayPanel.GetComponent<RectTransform>();
            float newWidth = Screen.width - 256f - 320f;
            float newPosition = 256f + ((Screen.width - 320f) - 256f) / 2f - (Screen.width / 2f);
            displayPanelTransform.localPosition = new Vector3(newPosition, -32f, 0f);
            displayPanelTransform.sizeDelta = new Vector2(newWidth, displayPanelTransform.sizeDelta.y);

            // Check if an update is required for display boxes
            if (InputManager.instance.getKeyDown("TypeSelect") || InputManager.instance.getKeyDown("Deselect") ||
                InputManager.instance.getKeyUp("TypeSelect") || InputManager.instance.getKeyUp("Deselect"))
            {
                updateDisplayBoxes();
            }
        }

        public void clearCommandButtons()
        {
            for (int i = 0; i < commandButtons.Count; i++)
            {
                Destroy(commandButtons[i]);
            }
            commandButtons.Clear();
        }

        public void populateCommandButtons()
        {
            // Create new ui and populate command buttons
            for (int i = 0; i < CommandManagerBase.instance.activeCommands.Count; i++)
            {
                Command activeCommand = CommandManagerBase.instance.activeCommands[i];

                // Skip null commands
                if (activeCommand == null) continue;

                // Create new button
                GameObject newButtonObject = Instantiate(commandButtonPrefab, commandPanel.transform);
                CommandButton newCommandButton = newButtonObject.GetComponent<CommandButton>();

                // Assign command to button
                newCommandButton.command = activeCommand;
                newCommandButton.commandIndex = i;
                newButtonObject.GetComponent<Button>().onClick.AddListener(() => CommandManagerBase.instance.executeCommand(newCommandButton.commandIndex));

                // Set button position
                float xOffset = (i % 4) * 64.0f;
                float yOffset = (Mathf.Floor(i / 4.0f)) * -64.0f;
                newButtonObject.transform.localPosition = new Vector2(-96.0f + xOffset, 64.0f + yOffset);

                // Attach command texture to new button
                switch (activeCommand.commandType)
                {
                    case CommandType.MOVE:
                        newButtonObject.GetComponent<Image>().sprite = moveCommandSprite;
                        break;
                    case CommandType.ATTACK:
                        newButtonObject.GetComponent<Image>().sprite = attackCommandSprite;
                        break;
                    case CommandType.HOLD:
                        newButtonObject.GetComponent<Image>().sprite = holdCommandSprite;
                        break;
                    case CommandType.HARVEST:
                        newButtonObject.GetComponent<Image>().sprite = harvestCommandSprite;
                        break;
                    case CommandType.OPEN_BUILD_MENU:
                        newButtonObject.GetComponent<Image>().sprite = openBuildMenuCommandSprite;
                        break;
                    case CommandType.CANCEL_BUILD_MENU:
                        newButtonObject.GetComponent<Image>().sprite = cancelBuildMenuCommandSprite;
                        break;

                    case CommandType.BUILD_NEST:
                        newButtonObject.GetComponent<Image>().sprite = buildNestCommandSprite;
                        break;
                    case CommandType.BUILD_WOMB:
                        newButtonObject.GetComponent<Image>().sprite = buildWombCommandSprite;
                        break;
                }
                commandButtons.Add(newButtonObject);
            }
        }

        public void clearSelectedObjects()
        {
            foreach (List<GameObject> row in displayBoxes)
            {
                foreach (GameObject displayBox in row)
                {
                    Destroy(displayBox);
                }
                row.Clear();
            }
            displayBoxes.Clear();
        }

        public void showSelectedObjects()
        {
            List<GameObject> selectedObjects = SelectionManager.instance.selectedObjects;
            if (selectedObjects.Count == 0)
            {
                // Hide display panel
                displayPanel.SetActive(false);
                return;
            }

            // Show display panel
            displayPanel.SetActive(true);

            // Calculate rows and columns
            float displayPanelWidth = displayPanel.GetComponent<RectTransform>().sizeDelta.x - displayBoxDefaultSize;
            int columns = Mathf.FloorToInt(displayPanelWidth / displayBoxDefaultSize);
            int rows = Mathf.CeilToInt((float)selectedObjects.Count / (float)columns);

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
                    GameObject newDisplayBox = Instantiate(displayBoxPrefab, displayPanel.transform);
                    newDisplayBox.GetComponent<Image>().sprite = selectedObjects[selectedIndex].GetComponent<Entity>().displayIcon;

                    // Set up button
                    SelectionDisplayButton newDisplayButton = newDisplayBox.GetComponent<SelectionDisplayButton>();
                    newDisplayButton.selectedObjectName = selectedObjects[selectedIndex].name;
                    newDisplayButton.selectedIndex = selectedIndex;
                    newDisplayButton.defaultSize = displayBoxDefaultSize;
                    newDisplayButton.hoveredSize = displayBoxHoveredSize;
                    newDisplayBox.GetComponent<Button>().onClick.AddListener(() => onDisplayBoxClicked(newDisplayButton.selectedIndex));

                    row.Add(newDisplayBox);
                }
                displayBoxes.Add(row);
            }

            // Set starting positions for each box
            updateDisplayBoxes(true, true);
        }

        // doPositionUpdate false means we skip setting position and target position of display boxes, and only update frame colour
        // setPosition flag is only true for initialization to immediately set target position of boxes
        public void updateDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false)
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
            for (int r = 0; r < displayBoxes.Count; r++)
            {
                float rowWidth = 0;

                // Loop through columns
                for (int c = 0; c < displayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = displayBoxes[r][c].GetComponent<SelectionDisplayButton>();

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
                    float sizeDifference = displayBoxes[r][0].GetComponent<SelectionDisplayButton>().hoveredSize - displayBoxDefaultSize;
                    if (r < hoveredRow) rowYPositions[r] += sizeDifference / 2f;
                    if (r > hoveredRow) rowYPositions[r] -= sizeDifference / 2f;
                }
            }

            // Second pass to set positions/sizes/frame colours
            // Loop through rows
            for (int r = 0; r < displayBoxes.Count; r++)
            {
                // Loop through columns
                float currentPosition = 0f - (rowWidths[r] / 2f);
                for (int c = 0; c < displayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = displayBoxes[r][c].GetComponent<SelectionDisplayButton>();

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
                        RectTransform boxTransform = displayBoxes[r][c].GetComponent<RectTransform>();
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

        public void onDisplayBoxClicked(int index)
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
                EventManager.instance.setFocusCompleteEvent.Invoke();
            }
            // Reselect single if already focused
            else SelectionController.instance.reselectSingle(index);
        }

        private void onSelectionStartCallback()
        {
            clearSelectedObjects();
            clearCommandButtons();
        }

        private void onSelectionSortedCallback()
        {
            showSelectedObjects();
        }

        private void onPopulateCommandsStartCallback()
        {
            clearCommandButtons();
        }

        private void onPopulateCommandsCompleteCallback()
        {
            populateCommandButtons();
        }

        private void onSetFocusCompleteCallback()
        {
            updateDisplayBoxes(false);
        }

    }
}
