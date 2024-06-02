using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MiniJam159.GameCore;
using MiniJam159.AICore;
using MiniJam159.CommandCore;
using MiniJam159.Structures;
using MiniJam159.Player;

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
        public List<GameObject> displayBoxes;

        // Singleton
        public static UIManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // Hide display panel at the start
            displayPanel.SetActive(false);
        }

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.selectionStartEvent.AddListener(onSelectionStartCallback);
            EventManager.instance.selectionCompleteEvent.AddListener(onSelectionCompleteCallback);
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
            foreach (GameObject displayBox in displayBoxes)
            {
                Destroy(displayBox);
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

            // Single selected structure
            if (selectedObjects.Count == 1 && selectedObjects[0].GetComponent<Structure>())
            {
                GameObject newDisplayBox = Instantiate(displayBoxPrefab, displayPanel.transform);
                newDisplayBox.GetComponent<RectTransform>().localPosition = new Vector3(0f, displayCenterHeight, 0f);
                newDisplayBox.GetComponent<Image>().sprite = selectedObjects[0].GetComponent<Structure>().structureData.displayIcon;

                // Set up button
                SelectedDisplayButton newDisplayButton = newDisplayBox.GetComponent<SelectedDisplayButton>();
                newDisplayButton.selectedObjectName = selectedObjects[0].name;
                newDisplayButton.selectedIndex = 0;
                newDisplayBox.GetComponent<Button>().onClick.AddListener(() => SelectionController.instance.singleSelectObjectInList(newDisplayButton.selectedIndex));

                displayBoxes.Add(newDisplayBox);
                return;
            }

            // Units
            int columns = Mathf.FloorToInt(displayPanel.GetComponent<RectTransform>().sizeDelta.x / 32.0f);
            int rows = Mathf.CeilToInt((float)selectedObjects.Count / (float)columns);
            int lastRowColumns = selectedObjects.Count % columns;
            if (lastRowColumns == 0) lastRowColumns = columns;
            float leftBoxXPosition = 0f - (columns - 1) * 32f / 2f;
            float lastRowLeftBoxXPosition = 0f - (lastRowColumns - 1) * 32f / 2f;

            for (int i = 0; i < selectedObjects.Count; i++)
            {
                GameObject selectedUnit = selectedObjects[i];

                Vector3 boxLocalPosition = new Vector3(0f, 0f, 0f);
                if (i < selectedObjects.Count - lastRowColumns)
                {
                    // Not last row
                    boxLocalPosition.x = leftBoxXPosition + (i % columns) * 32f;
                }
                else
                {
                    // Last row
                    boxLocalPosition.x = lastRowLeftBoxXPosition + (i % columns) * 32f;
                }
                //boxLocalPosition.y = displayCenterHeight - Mathf.Floor(i + 1 / (float)columns) * 32f;
                int currentRow = Mathf.FloorToInt(i / columns);
                boxLocalPosition.y = displayCenterHeight - currentRow * 32f;

                GameObject newDisplayBox = Instantiate(displayBoxPrefab, displayPanel.transform);
                newDisplayBox.GetComponent<RectTransform>().localPosition = boxLocalPosition;
                newDisplayBox.GetComponent<Image>().sprite = selectedObjects[i].GetComponent<GameAI>().displaySprite;

                // Set up button
                SelectedDisplayButton newDisplayButton = newDisplayBox.GetComponent<SelectedDisplayButton>();
                newDisplayButton.selectedObjectName = selectedObjects[i].name;
                newDisplayButton.selectedIndex = i;
                newDisplayBox.GetComponent<Button>().onClick.AddListener(() => onDisplayBoxClicked(newDisplayButton.selectedIndex));

                displayBoxes.Add(newDisplayBox);
            }
        }

        public void onDisplayBoxClicked(int index)
        {
            // Clear commands
            CommandManagerBase.instance.clearCommands();

            // Select new object
            SelectionController.instance.singleSelectObjectInList(index);
        }

        private void onSelectionStartCallback()
        {
            clearSelectedObjects();
            clearCommandButtons();
        }

        private void onSelectionCompleteCallback()
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
