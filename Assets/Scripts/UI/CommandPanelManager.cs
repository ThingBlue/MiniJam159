using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MiniJam159.CommandCore;
using MiniJam159.UICore;

namespace MiniJam159.UI
{
    public class CommandPanelManager : CommandPanelManagerBase
    {
        #region Inspector members

        public GameObject commandPanel;

        public GameObject commandButtonPrefab;
        public float commandButtonSize;

        public Sprite moveCommandSprite;
        public Sprite attackCommandSprite;
        public Sprite stopCommandSprite;
        public Sprite harvestCommandSprite;
        public Sprite openBuildMenuCommandSprite;
        public Sprite cancelBuildMenuCommandSprite;

        public Sprite buildNestCommandSprite;
        public Sprite buildWombCommandSprite;

        #endregion

        public List<GameObject> commandButtons = new List<GameObject>();

        public override void populateCommandButtons()
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
                float xOffset = (i % 4) * commandButtonSize;
                float yOffset = (Mathf.Floor(i / 4.0f)) * -commandButtonSize;
                newButtonObject.transform.localPosition = new Vector2(-96.0f + xOffset, 64.0f + yOffset);

                // Attach command texture to new button
                switch (activeCommand.commandType)
                {
                    case CommandType.STOP:
                        newButtonObject.GetComponent<Image>().sprite = stopCommandSprite;
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

        public override void clearCommandButtons()
        {
            for (int i = 0; i < commandButtons.Count; i++)
            {
                Destroy(commandButtons[i]);
            }
            commandButtons.Clear();
        }

    }
}
