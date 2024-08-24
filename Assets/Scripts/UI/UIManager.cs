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
            // Loop through rows
            for (int r = 0; r < displayBoxes.Count; r++)
            {
                // Loop through columns
                for (int c = 0; c < displayBoxes[r].Count; c++)
                {
                    // Calculate position
                    float leftBoxXPosition = 0f - (displayBoxes[r].Count - 1) * 32f / 2f;
                    Vector3 boxLocalPosition = new Vector3(0f, 0f, 0f);
                    boxLocalPosition.x = leftBoxXPosition + (c * 32f);

                    Debug.Log(boxLocalPosition.x);

                    // Set position
                    RectTransform boxTransform = displayBoxes[r][c].GetComponent<RectTransform>();
                    boxTransform.localPosition = boxLocalPosition;

                    // Set target position and size
                    SelectionDisplayButton newDisplayButton = displayBoxes[r][c].GetComponent<SelectionDisplayButton>();
                    newDisplayButton.targetLocalPosition = boxLocalPosition;
                    newDisplayButton.targetSize = new Vector2(32f, 32f);
                }
            }

        }

        public void onDisplayBoxClicked(int index)
        {
            // Clear commands
            CommandManagerBase.instance.clearCommands();

            // Select new object
            if (InputManager.instance.getKey("Deselect"))
            {
                // Deselect
                if (InputManager.instance.getKey("TypeSelect"))
                {
                    // Type
                    SelectionController.instance.deselectType(index);
                }
                else
                {
                    // Single
                    SelectionController.instance.deselectSingle(index);
                }
            }
            else
            {
                // Reselect
                if (InputManager.instance.getKey("TypeSelect"))
                {
                    // Type
                    SelectionController.instance.reselectType(index);
                }
                else
                {
                    // Single
                    SelectionController.instance.reselectSingle(index);
                }
            }
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
