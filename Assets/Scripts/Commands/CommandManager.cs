using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Commands
{
    public class CommandManager : MonoBehaviour
    {
        public List<Command> activeCommands;

        // Singleton
        public static CommandManager instance;

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
            if (activeCommands[index] == null) return;

            activeCommands[index].execute();
        }
    }
}
