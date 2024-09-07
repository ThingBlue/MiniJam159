using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;
using MiniJam159.CommandCore;
using UnityEngine.UI;

namespace MiniJam159.UICore
{
    public class UIManagerBase : MonoBehaviour
    {
        #region Inspector members

        public GameObject minimapPanel;
        public GameObject displayPanel;
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

        public GameObject displayBoxPrefab;
        public float displayCenterHeight;
        public float displayBoxDefaultSize;
        public float displayBoxHoveredSize;

        #endregion

        public List<GameObject> commandButtons;
        public List<List<GameObject>> displayBoxes;

        // Singleton
        public static UIManagerBase instance;

        protected virtual void Awake()
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

        protected virtual void Update()
        {
            // Calculate display panel background size and position
            RectTransform displayPanelTransform = displayPanel.GetComponent<RectTransform>();

            float minimapPanelWidth = minimapPanel.GetComponent<RectTransform>().sizeDelta.x;
            float commandPanelWidth = commandPanel.GetComponent<RectTransform>().sizeDelta.x;

            float newWidth = Screen.width - minimapPanelWidth - commandPanelWidth;
            float newPosition = minimapPanelWidth + ((Screen.width - commandPanelWidth) - minimapPanelWidth) / 2f - (Screen.width / 2f);
            displayPanelTransform.localPosition = new Vector3(newPosition, displayPanelTransform.localPosition.y, 0f);
            displayPanelTransform.sizeDelta = new Vector2(newWidth, displayPanelTransform.sizeDelta.y);

            // Check if an update is required for display boxes
            if (InputManager.instance.getKeyDown("TypeSelect") || InputManager.instance.getKeyDown("Deselect") ||
                InputManager.instance.getKeyUp("TypeSelect") || InputManager.instance.getKeyUp("Deselect"))
            {
                updateDisplayBoxes();
            }
        }

        public virtual void clearCommandButtons()
        {
            for (int i = 0; i < commandButtons.Count; i++)
            {
                Destroy(commandButtons[i]);
            }
            commandButtons.Clear();
        }

        public virtual void populateCommandButtons()
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

        public virtual void clearDisplayBoxes()
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

        public virtual void showDisplayBoxes()
        {
            // See UIManager::showDisplayBoxes()
        }

        public virtual void updateDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false)
        {
            // See UIManager::updateDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false)
        }

        public virtual void onDisplayBoxClicked(int index)
        {
            // See UIManager::onDisplayBoxClicked(int index)
        }
    }
}
