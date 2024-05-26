using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MiniJam159.Commands;

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

        #endregion

        public List<GameObject> commandButtons;

        // Singleton
        public static UIManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public void clearCommands()
        {
            CommandManager.instance.activeCommands.Clear();
            for (int i = 0; i < commandButtons.Count; i++)
            {
                Destroy(commandButtons[i]);
            }
        }

        public void populateCommandUI(List<CommandType> newCommands)
        {
            // Clear previous commands
            clearCommands();

            // Create new ui
            for (int i = 0; i < newCommands.Count; i++)
            {
                // Skip null commands
                if (newCommands[i] == CommandType.NULL)
                {
                    // Add null command to command manager
                    CommandManager.instance.activeCommands.Add(null);
                    continue;
                }

                // Create new button
                GameObject newButton = Instantiate(commandButtonPrefab, commandPanel.transform);

                // Set button position
                float xOffset = (i % 4) * 64.0f;
                float yOffset = (Mathf.Floor(i / 4.0f)) * -64.0f;
                newButton.transform.localPosition = new Vector2(-96.0f + xOffset, 64.0f + yOffset);

                Command newCommand = new Command();

                // Attach command script and texture to new button
                switch (newCommands[i])
                {
                    case CommandType.MOVE:
                        newCommand = newButton.AddComponent<MoveCommand>();
                        newButton.GetComponent<Image>().sprite = moveCommandSprite;
                        break;
                    case CommandType.ATTACK:
                        newCommand = newButton.AddComponent<AttackCommand>();
                        newButton.GetComponent<Image>().sprite = attackCommandSprite;
                        break;
                    case CommandType.HOLD:
                        newCommand = newButton.AddComponent<HoldCommand>();
                        newButton.GetComponent<Image>().sprite = holdCommandSprite;
                        break;
                    case CommandType.BUILD:
                        newCommand = newButton.AddComponent<BuildCommand>();
                        newButton.GetComponent<Image>().sprite = buildCommandSprite;
                        break;
                }
                commandButtons.Add(newButton);

                // Populate command manager active commands list
                CommandManager.instance.activeCommands.Add(newCommand);
            }
        }

    }
}
