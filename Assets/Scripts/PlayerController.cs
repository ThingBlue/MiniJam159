using MiniJam159.GameCore;
using MiniJam159.Structures;
using MiniJam159.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector members

        public float massSelectDelay;

        #endregion

        private bool mouse0Down;
        private bool mouse1Down;

        private bool massSelecting;
        private float massSelectStartTimer;

        // Singleton
        public static PlayerController instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Update()
        {
            // Check key states
            if (InputManager.instance.getKeyDown("Mouse0")) mouse0Down = true;
            if (InputManager.instance.getKeyDown("Mouse1")) mouse1Down = true;

            // DEBUG DEBUG DEBUG TEST TEST TEST
            if (InputManager.instance.getKeyDown("PlacementTest"))
            {
                Structure newStructure = new Structure();
                newStructure.size = new Vector2(2, 3);
                StructureManager.instance.beginPlacement(newStructure);
            }
            if (InputManager.instance.getKeyDown("CommandUpdateTest"))
            {
                List<CommandType> commands = new List<CommandType>();
                commands.Add(CommandType.MOVE);
                commands.Add(CommandType.ATTACK);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.HOLD);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.BUILD);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                UIManager.instance.updateCommandUI(commands);
            }
        }

        private void FixedUpdate()
        {
            // Camera panning
            if (Input.mousePosition.x <= 0) CameraController.instance.PanCamera(Vector2.left);
            if (Input.mousePosition.x >= Screen.width) CameraController.instance.PanCamera(Vector2.right);
            if (Input.mousePosition.y >= Screen.height) CameraController.instance.PanCamera(Vector2.up);
            if (Input.mousePosition.y <= 0) CameraController.instance.PanCamera(Vector2.down);

            // Mouse
            if (StructureManager.instance.inPlacementMode)
            {
                if (mouse0Down) StructureManager.instance.finishPlacement();
                if (mouse1Down) StructureManager.instance.cancelPlacement();
            }
            else
            {
                // Mass select
                if (InputManager.instance.getKey("Mouse0"))
                {
                    massSelectStartTimer += Time.deltaTime;
                }
                else
                {
                    if (massSelectStartTimer >= massSelectDelay)
                    {
                        // Execute mass select
                        massSelecting = true;
                    }

                    // Reset mass select
                    massSelectStartTimer = 0.0f;
                    massSelecting = false;
                }

                // Single select
                if (mouse0Down && !massSelecting)
                {

                }

                // Movement commands
                if (mouse1Down)
                {
                    // Attack if hovering over enemy
                    // Interact if hovering over interactable
                    // Move if none of the above
                }
            }

            // Keyboard
            if (InputManager.instance.getKeyDown("QCommand")) UIManager.instance.executeCommand(0);
            if (InputManager.instance.getKeyDown("WCommand")) UIManager.instance.executeCommand(1);
            if (InputManager.instance.getKeyDown("ECommand")) UIManager.instance.executeCommand(2);
            if (InputManager.instance.getKeyDown("RCommand")) UIManager.instance.executeCommand(3);
            if (InputManager.instance.getKeyDown("ACommand")) UIManager.instance.executeCommand(4);
            if (InputManager.instance.getKeyDown("SCommand")) UIManager.instance.executeCommand(5);
            if (InputManager.instance.getKeyDown("DCommand")) UIManager.instance.executeCommand(6);
            if (InputManager.instance.getKeyDown("FCommand")) UIManager.instance.executeCommand(7);
            if (InputManager.instance.getKeyDown("ZCommand")) UIManager.instance.executeCommand(8);
            if (InputManager.instance.getKeyDown("XCommand")) UIManager.instance.executeCommand(9);
            if (InputManager.instance.getKeyDown("CCommand")) UIManager.instance.executeCommand(10);
            if (InputManager.instance.getKeyDown("VCommand")) UIManager.instance.executeCommand(11);

            // Clear key states
            mouse0Down = false;
            mouse1Down = false;
        }
    }
}
