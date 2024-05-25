using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MiniJam159.UI
{
    public class UIManager : MonoBehaviour
    {
        #region Inspector members

        public GameObject commandPanel;

        #endregion

        public List<Command> activeCommands;

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
            activeCommands = new List<Command>();
        }

        public void executeCommand(int index)
        {
            if (activeCommands[index])
            {
                activeCommands[index].execute();
            }
        }
    }
}
