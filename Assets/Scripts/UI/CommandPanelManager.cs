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

        #endregion

        public List<GameObject> commandButtons = new List<GameObject>();

        public override void populateCommandButtons()
        {
            // Create new ui and populate command buttons
            for (int i = 0; i < CommandManagerBase.instance.activeCommands.Count; i++)
            {
                CommandBase activeCommand = CommandManagerBase.instance.activeCommands[i];

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
                newButtonObject.GetComponent<Image>().sprite = CommandManagerBase.instance.getCommandSprite(activeCommand);

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
