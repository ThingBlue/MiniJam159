using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.Structures;
using MiniJam159.AICore;
using MiniJam159.CommandCore;
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

        public LayerMask unitLayer;
        public LayerMask structureLayer;
        public LayerMask resourceLayer;

        public string enemyTag;

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
                if (InputManager.instance.getKeyDown("QCommand")) CommandManagerBase.instance.executeCommand(0);
                if (InputManager.instance.getKeyDown("WCommand")) CommandManagerBase.instance.executeCommand(1);
                if (InputManager.instance.getKeyDown("ECommand")) CommandManagerBase.instance.executeCommand(2);
                if (InputManager.instance.getKeyDown("RCommand")) CommandManagerBase.instance.executeCommand(3);
                if (InputManager.instance.getKeyDown("ACommand")) CommandManagerBase.instance.executeCommand(4);
                if (InputManager.instance.getKeyDown("SCommand")) CommandManagerBase.instance.executeCommand(5);
                if (InputManager.instance.getKeyDown("DCommand")) CommandManagerBase.instance.executeCommand(6);
                if (InputManager.instance.getKeyDown("FCommand")) CommandManagerBase.instance.executeCommand(7);
                if (InputManager.instance.getKeyDown("ZCommand")) CommandManagerBase.instance.executeCommand(8);
                if (InputManager.instance.getKeyDown("XCommand")) CommandManagerBase.instance.executeCommand(9);
                if (InputManager.instance.getKeyDown("CCommand")) CommandManagerBase.instance.executeCommand(10);
                if (InputManager.instance.getKeyDown("VCommand")) CommandManagerBase.instance.executeCommand(11);
            }

            // DEBUG DEBUG DEBUG TEST TEST TEST
            if (InputManager.instance.getKeyDown("PlacementTest"))
            {
                EventManager.instance.buildWombCommandEvent.Invoke();
            }
            if (InputManager.instance.getKeyDown("PlacementTest2"))
            {
                EventManager.instance.buildNestCommandEvent.Invoke();
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
                        // Create structure
                        GameObject newStructureObject = StructureManager.instance.finishPlacement();

                        // Send selected workers to build structure
                        if (newStructureObject) executeBuildTarget(newStructureObject);

                        ignoreNextMouse0Up = true;
                    }
                    if (mouse1Down) StructureManager.instance.cancelPlacement();
                    break;

                case PlayerMode.ATTACK_TARGET:
                    // Wait for player input
                    if (!mouse0Down) break;

                    GameObject attackTarget = InputManager.instance.mouseRaycastObject(unitLayer | structureLayer);
                    if (attackTarget == null || attackTarget.tag != enemyTag)
                    {
                        // No target, execute attack move instead
                        executeAttackMove();
                        return;
                    }
                    if (!EventSystem.current.IsPointerOverGameObject()) executeAttackTarget(attackTarget);
                    break;

                case PlayerMode.HARVEST_TARGET:
                    // Wait for player input
                    if (!mouse0Down) break;

                    GameObject resourceTarget = InputManager.instance.mouseRaycastObject(resourceLayer);
                    if (resourceTarget == null)
                    {
                        // No target, cancel attack command
                        PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
                        return;
                    }
                    if (!EventSystem.current.IsPointerOverGameObject()) executeHarvestTarget(resourceTarget);
                    break;

                case PlayerMode.MOVE_TARGET:
                    if (mouse0Down && !EventSystem.current.IsPointerOverGameObject()) executeMove();
                    break;

                case PlayerMode.MASS_SELECT:
                    SelectionController.instance.updateMassSelectBox();

                    if (mouse0Up)
                    {
                        // Clear commands
                        CommandManagerBase.instance.clearCommands();

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
                        CommandManagerBase.instance.clearCommands();

                        SelectionController.instance.executeSingleSelect();
                    }

                    // Movement commands
                    if (mouse1Down && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Raycast at mouse position to check what the player is hovering over
                        GameObject target = InputManager.instance.mouseRaycastObject(unitLayer | structureLayer | resourceLayer);
                        Entity targetEntity = null;
                        GameAI targetUnit = null;
                        Structure targetStructure = null;
                        Resource targetResource = null;
                        if (target) targetEntity = target.GetComponent<Entity>();
                        if (target) targetUnit = target.GetComponent<GameAI>();
                        if (target) targetStructure = target.GetComponent<Structure>();
                        if (target) targetResource = target.GetComponent<Resource>();

                        // Attack if hovering over enemy unit or structure
                        if (targetEntity != null && target.tag == enemyTag) executeAttackTarget(target);
                        // Harvest is hovering over resource
                        else if (targetResource != null) executeHarvestTarget(target);
                        // Harvest is hovering over unfinished building
                        else if (targetStructure != null && targetStructure.buildProgress < targetStructure.maxBuildProgress) executeBuildTarget(target);
                        // Move if none of the above
                        else executeMove();
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

        public void executeMove()
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
                    // Invoke command method in ai using mouse position in world
                    Vector3 mousePositionInWorld = InputManager.instance.getMousePositionInWorld();
                    method.Invoke(ai, new object[] { mousePositionInWorld });
                }
            }

            // Finish command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public void executeAttackMove()
        {
            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a GameAI
                GameAI ai = selectedObject.GetComponent<GameAI>();
                if (ai == null) continue;

                MethodInfo method = ai.GetType().GetMethod("attackMoveAICommand");
                if (method != null)
                {
                    // Invoke command method in ai using mouse position in world
                    Vector3 mousePositionInWorld = InputManager.instance.getMousePositionInWorld();
                    method.Invoke(ai, new object[] { mousePositionInWorld });

                    Debug.Log("Attack moving");
                }
            }

            // Finish command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public void executeAttackTarget(GameObject target)
        {
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

                    Debug.Log("Attacking " + target);
                }
            }

            // Finish attack command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public void executeHarvestTarget(GameObject target)
        {
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
                    method.Invoke(ai, new object[] { target });

                    Debug.Log("Harvesting " + target);
                }
            }

            // Finish attack command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public void executeBuildTarget(GameObject target)
        {
            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a GameAI
                GameAI ai = selectedObject.GetComponent<GameAI>();
                if (ai == null) continue;

                MethodInfo method = ai.GetType().GetMethod("buildStructureCommand");
                if (method != null)
                {
                    // Invoke command method in ai using transform of target
                    method.Invoke(ai, new object[] { target });

                    Debug.Log("Building " + target);
                }
            }

            // Finish attack command
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }
    }
}
