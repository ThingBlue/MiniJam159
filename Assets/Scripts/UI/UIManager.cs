using Codice.Client.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public List<CommandType> activeCommands;
        public List<GameObject> commandButtons;

        // Singleton
        public static UIManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            activeCommands = new List<CommandType>();
        }

        public void executeCommand(int index)
        {
            if (commandButtons[index] == null) return;

            Command commandComponent = commandButtons[index].GetComponent<Command>();
            if (commandComponent == null) return;

            commandComponent.execute();
        }

        public void updateCommandUI(List<CommandType> newCommands)
        {
            // Clear previous command ui
            for (int i = 0; i < commandButtons.Count; i++)
            {
                Destroy(commandButtons[i]);
            }

            // Set new commands
            activeCommands = newCommands;

            // Create new ui
            for (int i = 0; i < newCommands.Count; i++)
            {
                // Skip null commands
                if (newCommands[i] == CommandType.NULL) continue;

                // Create new button
                GameObject newButton = Instantiate(commandButtonPrefab, commandPanel.transform);

                // Set button position
                float xOffset = (i % 4) * 64.0f;
                float yOffset = (Mathf.Floor(i / 4.0f)) * -64.0f;
                newButton.transform.localPosition = new Vector2(-96.0f + xOffset, 64.0f + yOffset);

                // Attach command script and texture to new button
                switch (newCommands[i])
                {
                    case CommandType.MOVE:
                        newButton.AddComponent<MoveCommand>();
                        newButton.GetComponent<Image>().sprite = moveCommandSprite;
                        break;
                    case CommandType.ATTACK:
                        newButton.AddComponent<AttackCommand>();
                        newButton.GetComponent<Image>().sprite = attackCommandSprite;
                        break;
                    case CommandType.HOLD:
                        newButton.AddComponent<HoldCommand>();
                        newButton.GetComponent<Image>().sprite = holdCommandSprite;
                        break;
                    case CommandType.BUILD:
                        newButton.AddComponent<BuildCommand>();
                        newButton.GetComponent<Image>().sprite = buildCommandSprite;
                        break;
                }

                commandButtons.Add(newButton);
            }
        }

    }
}
