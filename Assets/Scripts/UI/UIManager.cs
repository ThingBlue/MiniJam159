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
            // Calculate display panel size and position
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
                GameObject newDisplayBox = Instantiate(displayBoxPrefab);
                newDisplayBox.GetComponent<RectTransform>().position = new Vector3(0, displayCenterHeight, 0);
            }
        }

    }
}
