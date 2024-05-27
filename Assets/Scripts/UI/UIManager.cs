using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MiniJam159.GameCore;
using MiniJam159.AI;
using MiniJam159.Structures;

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
        public Sprite buildCommandSprite;
        public Sprite harvestCommandSprite;

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
        }

        public void populateCommandButtons()
        {
            clearCommandButtons();

            // Create new ui
            for (int i = 0; i < CommandManager.instance.activeCommands.Count; i++)
            {
                Command activeCommand = CommandManager.instance.activeCommands[i];

                // Skip null commands
                if (activeCommand == null) continue;

                // Create new button
                GameObject newButtonObject = Instantiate(commandButtonPrefab, commandPanel.transform);
                CommandButton newCommandButton = newButtonObject.GetComponent<CommandButton>();
                newCommandButton.command = activeCommand;

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
                    case CommandType.BUILD:
                        newButtonObject.GetComponent<Image>().sprite = buildCommandSprite;
                        break;
                    case CommandType.HARVEST:
                        newButtonObject.GetComponent<Image>().sprite = harvestCommandSprite;
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

        public void showSelectedObjects(List<GameObject> selectedObjects)
        {
            clearSelectedObjects();

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
                newDisplayBox.GetComponent<Image>().sprite = selectedObjects[0].GetComponent<Structure>().structureData.displaySprite;
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
                newDisplayBox.GetComponent<Image>().sprite = selectedObjects[0].GetComponent<GameAI>().displaySprite;
                displayBoxes.Add(newDisplayBox);
            }
        }

    }
}
