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

        #endregion

        public List<GameObject> commandButtons;
        public List<List<GameObject>> displayBoxes;

        private bool displayBoxUpdateNeeded = false;

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
                displayBoxUpdateNeeded = true;
            }
        }

        private void FixedUpdate()
        {
            if (displayBoxUpdateNeeded)
            {
                updateDisplayBoxes();
                displayBoxUpdateNeeded = false;
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
            int columns = Mathf.FloorToInt(displayPanel.GetComponent<RectTransform>().sizeDelta.x / 32.0f);
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
                    newDisplayButton.row = row;
                    newDisplayBox.GetComponent<Button>().onClick.AddListener(() => onDisplayBoxClicked(newDisplayButton.selectedIndex));

                    row.Add(newDisplayBox);
                }
                displayBoxes.Add(row);
            }

            // Set starting positions for each box
            updateDisplayBoxes(true);
        }

        // setPosition flag is only true for initialization to immediately set target position of boxes
        public void updateDisplayBoxes(bool setPosition = false)
        {
            // Reset selection statuses and find hovered entity
            // Loop through rows
            SelectionDisplayButton hoveredDisplayButton = null;
            for (int r = 0; r < displayBoxes.Count; r++)
            {
                // Loop through columns
                for (int c = 0; c < displayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = displayBoxes[r][c].GetComponent<SelectionDisplayButton>();

                    // Reset selection status
                    displayButton.setSelectStatus(SelectStatus.DEFAULT);

                    // Check for hovered status
                    if (hoveredDisplayButton == null && displayButton.hovered) hoveredDisplayButton = displayButton;
                }
            }

            // Get key statuses
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool typeSelectKey = InputManager.instance.getKey("TypeSelect");

            // Set selection status of hovered entity
            if (hoveredDisplayButton != null)
            {
                // Deselect
                if (deselectKey) hoveredDisplayButton.setSelectStatus(SelectStatus.DESELECT);
                // Reselect
                else hoveredDisplayButton.setSelectStatus(SelectStatus.RESELECT);
            }

            // Loop through rows
            for (int r = 0; r < displayBoxes.Count; r++)
            {
                // Calculate total width of row
                float rowWidth = 0;
                for (int c = 0; c < displayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = displayBoxes[r][c].GetComponent<SelectionDisplayButton>();
                    rowWidth += displayButton.hovered ? displayButton.hoveredWidth : displayButton.defaultWidth;
                }

                // Loop through columns
                float currentPosition = 0f - (rowWidth / 2f);
                for (int c = 0; c < displayBoxes[r].Count; c++)
                {
                    SelectionDisplayButton displayButton = displayBoxes[r][c].GetComponent<SelectionDisplayButton>();

                    // Calculate box position
                    Vector3 boxLocalPosition = new Vector3(currentPosition, 0f, 0f);
                    boxLocalPosition.x += (displayButton.hovered ? displayButton.hoveredWidth : displayButton.defaultWidth) / 2f;
                    boxLocalPosition.y = displayCenterHeight - (r * 32f);

                    // Update current position
                    currentPosition += displayButton.hovered ? displayButton.hoveredWidth : displayButton.defaultWidth;

                    // Set target position
                    RectTransform boxTransform = displayBoxes[r][c].GetComponent<RectTransform>();
                    displayButton.targetLocalPosition = boxLocalPosition;

                    // Immediately set position if flag is true
                    if (setPosition) boxTransform.localPosition = boxLocalPosition;

                    // Set selection status of all boxes of the same sorting priority
                    if (hoveredDisplayButton != null)
                    {
                        Entity hoveredEntity = SelectionManager.instance.selectedObjects[hoveredDisplayButton.selectedIndex].GetComponent<Entity>();
                        Entity displayButtonEntity = SelectionManager.instance.selectedObjects[displayButton.selectedIndex].GetComponent<Entity>();

                        // Deselect
                        if (deselectKey && typeSelectKey && displayButtonEntity.sortPriority == hoveredEntity.sortPriority)
                        {
                            displayButton.setSelectStatus(SelectStatus.DESELECT);
                        }
                        // Reselect
                        if (!deselectKey && typeSelectKey && displayButtonEntity.sortPriority == hoveredEntity.sortPriority)
                        {
                            displayButton.setSelectStatus(SelectStatus.RESELECT);
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
            // Reselect single
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

    }
}
