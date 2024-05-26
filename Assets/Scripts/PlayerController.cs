using MiniJam159.GameCore;
using MiniJam159.Structures;
using MiniJam159.UI;
using MiniJam159.Commands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MiniJam159.Selection;

namespace MiniJam159
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector members

        #endregion

        private bool mouse0Down;
        private bool mouse1Down;
        private bool mouse0Up;
        private bool mouse1Up;

        private bool canStartMassSelect;
        private bool ignoreNextMouse0Up;

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
            // Mouse
            if (InputManager.instance.getKeyDown("Mouse0")) mouse0Down = true;
            if (InputManager.instance.getKeyDown("Mouse1")) mouse1Down = true;
            if (InputManager.instance.getKeyUp("Mouse0") && !ignoreNextMouse0Up) mouse0Up = true;
            if (InputManager.instance.getKeyUp("Mouse0") && ignoreNextMouse0Up) ignoreNextMouse0Up = false;
            if (InputManager.instance.getKeyUp("Mouse1")) mouse1Up = true;

            // Keyboard
            if (InputManager.instance.getKeyDown("QCommand")) CommandManager.instance.executeCommand(0);
            if (InputManager.instance.getKeyDown("WCommand")) CommandManager.instance.executeCommand(1);
            if (InputManager.instance.getKeyDown("ECommand")) CommandManager.instance.executeCommand(2);
            if (InputManager.instance.getKeyDown("RCommand")) CommandManager.instance.executeCommand(3);
            if (InputManager.instance.getKeyDown("ACommand")) CommandManager.instance.executeCommand(4);
            if (InputManager.instance.getKeyDown("SCommand")) CommandManager.instance.executeCommand(5);
            if (InputManager.instance.getKeyDown("DCommand")) CommandManager.instance.executeCommand(6);
            if (InputManager.instance.getKeyDown("FCommand")) CommandManager.instance.executeCommand(7);
            if (InputManager.instance.getKeyDown("ZCommand")) CommandManager.instance.executeCommand(8);
            if (InputManager.instance.getKeyDown("XCommand")) CommandManager.instance.executeCommand(9);
            if (InputManager.instance.getKeyDown("CCommand")) CommandManager.instance.executeCommand(10);
            if (InputManager.instance.getKeyDown("VCommand")) CommandManager.instance.executeCommand(11);

            // DEBUG DEBUG DEBUG TEST TEST TEST
            if (InputManager.instance.getKeyDown("PlacementTest"))
            {
                StructureData newStructureData = new StructureData();
                newStructureData.size = new Vector2(2, 3);
                newStructureData.commands = new List<CommandType>();

                newStructureData.commands.Add(CommandType.MOVE);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.HOLD);
                newStructureData.commands.Add(CommandType.NULL);

                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.ATTACK);
                newStructureData.commands.Add(CommandType.NULL);

                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.BUILD);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);

                StructureManager.instance.beginPlacement(newStructureData);
            }
            if (InputManager.instance.getKeyDown("PlacementTest2"))
            {
                StructureData newStructureData = new StructureData();
                newStructureData.size = new Vector2(6, 4);
                newStructureData.commands = new List<CommandType>();

                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.MOVE);

                newStructureData.commands.Add(CommandType.BUILD);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.ATTACK);

                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.HOLD);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);

                StructureManager.instance.beginPlacement(newStructureData);
            }
            if (InputManager.instance.getKeyDown("CommandUpdateTest"))
            {
                List<CommandType> commands = new List<CommandType>();
                commands.Add(CommandType.MOVE);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.HOLD);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.ATTACK);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.BUILD);
                commands.Add(CommandType.NULL);
                commands.Add(CommandType.NULL);
                CommandManager.instance.populateCommands(commands);
                UIManager.instance.populateCommandButtons();
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
                if (mouse0Down)
                {
                    StructureManager.instance.finishPlacement();
                    ignoreNextMouse0Up = true;
                }
                if (mouse1Down) StructureManager.instance.cancelPlacement();

                SelectionManager.instance.massSelecting = false;
                SelectionManager.instance.massSelectStartPosition = Input.mousePosition;
                canStartMassSelect = false;
            }
            else if (SelectionManager.instance.massSelecting)
            {
                SelectionManager.instance.updateMassSelectBox();

                // Execute mass select
                if (mouse0Up) SelectionManager.instance.executeMassSelect();
            }
            else
            {
                // Set start position for mass select
                if (mouse0Down && !EventSystem.current.IsPointerOverGameObject())
                {
                    SelectionManager.instance.massSelectStartPosition = Input.mousePosition;
                    canStartMassSelect = true;
                }
                if (mouse0Down && EventSystem.current.IsPointerOverGameObject())
                {
                    canStartMassSelect = false;
                }

                if (InputManager.instance.getKey("Mouse0") && canStartMassSelect)
                {
                    massSelectStartTimer += Time.deltaTime;
                    if (massSelectStartTimer >= SelectionManager.instance.massSelectDelay ||
                        Vector2.Distance(SelectionManager.instance.massSelectStartPosition, Input.mousePosition) > SelectionManager.instance.massSelectMouseMoveDistance)
                    {
                        // Start mass select
                        SelectionManager.instance.massSelecting = true;

                        Debug.Log("Timer: " + massSelectStartTimer + ", Distance: " + Vector2.Distance(SelectionManager.instance.massSelectStartPosition, Input.mousePosition));
                    }
                }
                else
                {
                    // Reset mass select timer
                    massSelectStartTimer = 0.0f;
                    SelectionManager.instance.massSelectStartPosition = Input.mousePosition;
                }
                SelectionManager.instance.updateMassSelectBox();

                // Execute single select
                if (mouse0Up && !EventSystem.current.IsPointerOverGameObject()) SelectionManager.instance.executeSingleSelect();

                // Movement commands
                if (mouse1Down)
                {
                    // Attack if hovering over enemy
                    // Interact if hovering over interactable
                    // Move if none of the above
                }
            }

            // Clear key states
            mouse0Down = false;
            mouse1Down = false;
            mouse0Up = false;
            mouse1Up = false;
        }
    }
}
