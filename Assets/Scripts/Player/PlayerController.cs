using MiniJam159.GameCore;
using MiniJam159.Structures;
using MiniJam159.AICore;
using MiniJam159.Commands;
using MiniJam159.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniJam159.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector members

        public LayerMask enemyLayer;
        public LayerMask resourceLayer;

        public Sprite testStructureSprite;

        #endregion

        private bool mouse0Down;
        private bool mouse1Down;
        private bool mouse0Up;
        private bool mouse1Up;

        private bool canSelect;
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
            if (PlayerModeManager.instance.playerMode == PlayerMode.NORMAL)
            {
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
            }

            // DEBUG DEBUG DEBUG TEST TEST TEST
            if (InputManager.instance.getKeyDown("PlacementTest"))
            {
                StructureData newStructureData = new StructureData();
                newStructureData.structureType = StructureType.NULL;
                newStructureData.size = new Vector3(2, 1, 3);
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
                newStructureData.commands.Add(CommandType.OPEN_BUILD_MENU);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);

                newStructureData.displayIcon = testStructureSprite;

                StructureManager.instance.beginPlacement(newStructureData);
            }
            if (InputManager.instance.getKeyDown("PlacementTest2"))
            {
                StructureData newStructureData = new StructureData();
                newStructureData.structureType = StructureType.NEST;
                newStructureData.size = new Vector3(5, 1, 5);
                newStructureData.commands = new List<CommandType>();

                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.MOVE);

                newStructureData.commands.Add(CommandType.OPEN_BUILD_MENU);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.ATTACK);

                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.HOLD);
                newStructureData.commands.Add(CommandType.NULL);
                newStructureData.commands.Add(CommandType.NULL);

                newStructureData.displayIcon = testStructureSprite;

                StructureManager.instance.beginPlacement(newStructureData);
            }
        }

        private void FixedUpdate()
        {
            // Camera panning
            if (Input.mousePosition.x <= 0) CameraController.instance.panCamera(Vector3.left);
            if (Input.mousePosition.x >= Screen.width) CameraController.instance.panCamera(Vector3.right);
            if (Input.mousePosition.y >= Screen.height) CameraController.instance.panCamera(Vector3.forward);
            if (Input.mousePosition.y <= 0) CameraController.instance.panCamera(Vector3.back);

            switch (PlayerModeManager.instance.playerMode)
            {
                case PlayerMode.STRUCTURE_PLACEMENT:
                    if (mouse0Down)
                    {
                        StructureManager.instance.finishPlacement();
                        ignoreNextMouse0Up = true;
                    }
                    if (mouse1Down) StructureManager.instance.cancelPlacement();
                    break;

                case PlayerMode.ATTACK_TARGET:
                    if (mouse0Down && !EventSystem.current.IsPointerOverGameObject()) executeAttackTarget();
                    break;

                case PlayerMode.HARVEST_TARGET:
                    if (mouse0Down && !EventSystem.current.IsPointerOverGameObject()) executeHarvestTarget();
                    break;

                case PlayerMode.MOVE_TARGET:
                    if (mouse0Down && !EventSystem.current.IsPointerOverGameObject()) executeMoveTarget();
                    break;

                case PlayerMode.MASS_SELECT:
                    SelectionController.instance.updateMassSelectBox();

                    if (mouse0Up)
                    {
                        // Clear commands
                        CommandManager.instance.clearCommands();

                        // Execute mass select
                        SelectionController.instance.executeMassSelect();
                    }
                    break;

                case PlayerMode.NORMAL:
                    // Set start position for mass select
                    if (mouse0Down && !EventSystem.current.IsPointerOverGameObject())
                    {
                        SelectionController.instance.massSelectStartPosition = Input.mousePosition;
                        canSelect = true;
                    }
                    if (mouse0Down && EventSystem.current.IsPointerOverGameObject())
                    {
                        canSelect = false;
                    }

                    if (InputManager.instance.getKey("Mouse0") && canSelect)
                    {
                        massSelectStartTimer += Time.deltaTime;
                        if (massSelectStartTimer >= SelectionController.instance.massSelectDelay ||
                            Vector3.Distance(SelectionController.instance.massSelectStartPosition, Input.mousePosition) > SelectionController.instance.massSelectMouseMoveDistance)
                        {
                            // Start mass select
                            PlayerModeManager.instance.playerMode = PlayerMode.MASS_SELECT;

                            Debug.Log("Timer: " + massSelectStartTimer + ", Distance: " + Vector3.Distance(SelectionController.instance.massSelectStartPosition, Input.mousePosition));
                        }
                    }
                    else
                    {
                        // Reset mass select timer
                        massSelectStartTimer = 0.0f;
                        SelectionController.instance.massSelectStartPosition = Input.mousePosition;
                    }
                    SelectionController.instance.updateMassSelectBox();

                    // Execute single select
                    if (mouse0Up && canSelect && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Clear commands
                        CommandManager.instance.clearCommands();

                        SelectionController.instance.executeSingleSelect();
                    }

                    // Movement commands
                    if (mouse1Down && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Attack if hovering over enemy
                        if (InputManager.instance.mouseRaycastObject(enemyLayer)) executeAttackTarget();
                        // Interact if hovering over interactable
                        else if (InputManager.instance.mouseRaycastObject(resourceLayer)) executeHarvestTarget();
                        // Move if none of the above
                        else executeMoveTarget();
                    }
                    break;
            }

            // Don't allow start of mass select when occupied
            if (PlayerModeManager.instance.playerMode != PlayerMode.NORMAL && PlayerModeManager.instance.playerMode != PlayerMode.MASS_SELECT)
            {
                SelectionController.instance.massSelectStartPosition = Input.mousePosition;
                canSelect = false;
            }

            // Clear key states
            mouse0Down = false;
            mouse1Down = false;
            mouse0Up = false;
            mouse1Up = false;
        }

        public void executeMoveTarget()
        {
            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a GameAI
                GameAI ai = selectedObject.GetComponent<GameAI>();
                if (ai == null) continue;

                MethodInfo method = ai.GetType().GetMethod("moveAICommand");
                if (method != null)
                {
                    // Invoke move command method in ai using mouse position in world
                    Vector3 mousePositionInWorld = InputManager.instance.getMousePositionInWorld();
                    method.Invoke(ai, new object[] { mousePositionInWorld });
                }
            }

            // Finish move command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public void executeAttackTarget()
        {
            GameObject target = InputManager.instance.mouseRaycastObject(enemyLayer);
            if (target == null)
            {
                // No target, cancel attack command
                PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
                return;
            }

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a GameAI
                GameAI ai = selectedObject.GetComponent<GameAI>();
                if (ai == null) continue;

                MethodInfo method = ai.GetType().GetMethod("attackAICommand");
                if (method != null)
                {
                    // Invoke attack command method in ai using transform of target
                    method.Invoke(ai, new object[] { target.transform });
                }
            }

            // Finish attack command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public void executeHarvestTarget()
        {
            GameObject target = InputManager.instance.mouseRaycastObject(resourceLayer);
            if (target == null)
            {
                // No target, cancel attack command
                PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
                return;
            }
            IResource resource = target.GetComponent<IResource>();

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a GameAI
                GameAI ai = selectedObject.GetComponent<GameAI>();
                if (ai == null) continue;

                MethodInfo method = ai.GetType().GetMethod("harvestAICommand");
                if (method != null)
                {
                    // Invoke command method in ai using transform of target
                    method.Invoke(ai, new object[] { resource });

                    Debug.Log("Harvesting " + resource);
                }
            }

            // Finish attack command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }
    }
}
