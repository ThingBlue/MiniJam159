using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using MiniJam159.Common;
using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.UnitCore;
using MiniJam159.StructureCore;
using MiniJam159.CommandCore;
using MiniJam159.Resources;
using MiniJam159.UICore;

namespace MiniJam159.Player
{
    public class PlayerController : PlayerControllerBase
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
        private float mouseScroll;
        private bool cancelCommandKeyDown;
        private bool squad1KeyDown;
        private bool squad2KeyDown;
        private bool squad3KeyDown;
        private bool squad4KeyDown;
        private bool squad5KeyDown;
        private bool squad6KeyDown;
        private bool squad7KeyDown;
        private bool squad8KeyDown;

        private bool canSelect;
        private bool ignoreNextMouse0Up;

        private float massSelectStartTimer;

        private void Update()
        {
            // Mouse
            if (InputManager.instance.getKeyDown("Mouse0")) mouse0Down = true;
            if (InputManager.instance.getKeyDown("Mouse1")) mouse1Down = true;
            if (InputManager.instance.getKeyUp("Mouse0") && !ignoreNextMouse0Up) mouse0Up = true;
            if (InputManager.instance.getKeyUp("Mouse0") && ignoreNextMouse0Up) ignoreNextMouse0Up = false;
            if (InputManager.instance.getKeyUp("Mouse1")) mouse1Up = true;

            // Camera zooming
            mouseScroll += Input.GetAxis("Mouse ScrollWheel");

            // Keyboard
            if (playerMode == PlayerMode.NORMAL)
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

                if (InputManager.instance.getKeyDown("CycleFocus")) cycleFocus();

                if (InputManager.instance.getKeyDown("Squad1")) squad1KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad2")) squad2KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad3")) squad3KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad4")) squad4KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad5")) squad5KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad6")) squad6KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad7")) squad7KeyDown = true;
                if (InputManager.instance.getKeyDown("Squad8")) squad8KeyDown = true;

            }

            if (InputManager.instance.getKeyDown("CancelCommand")) cancelCommandKeyDown = true;
        }

        private void FixedUpdate()
        {
            // Camera panning
            if (Input.mousePosition.x <= 0) CameraController.instance.panCamera(Vector3.left);
            if (Input.mousePosition.x >= Screen.width) CameraController.instance.panCamera(Vector3.right);
            if (Input.mousePosition.y >= Screen.height) CameraController.instance.panCamera(Vector3.forward);
            if (Input.mousePosition.y <= 0) CameraController.instance.panCamera(Vector3.back);

            // Camera zooming
            CameraController.instance.zoomCamera(mouseScroll);

            // Handle input based on state
            switch (playerMode)
            {
                case PlayerMode.MASS_SELECT:
                    // Check for cancel
                    if (cancelCommandKeyDown || mouse1Down)
                    {
                        playerMode = PlayerMode.NORMAL;
                        canSelect = false;
                    }

                    SelectionControllerBase.instance.updateMassSelectBox();

                    if (mouse0Up)
                    {
                        // Clear commands
                        CommandManagerBase.instance.clearCommands();

                        // Execute mass select
                        SelectionControllerBase.instance.executeMassSelect();
                    }
                    break;

                case PlayerMode.ATTACK_TARGET:
                    // Check for cancel
                    if (cancelCommandKeyDown) playerMode = PlayerMode.NORMAL;

                    // Wait for player input
                    if (!mouse0Down) break;

                    GameObject attackTarget = InputManager.instance.mouseRaycastObject(unitLayer | structureLayer);
                    if (attackTarget == null || attackTarget.tag != enemyTag)
                    {
                        // No target, execute attack move instead
                        executeAttackMoveCommand(InputManager.instance.getMousePositionInWorld());
                        break;
                    }
                    if (!EventSystem.current.IsPointerOverGameObject()) executeAttackCommand(attackTarget);
                    break;

                case PlayerMode.STRUCTURE_PLACEMENT:
                    if (mouse0Down)
                    {
                        // Get data for new structure
                        GameObject newStructureObject = StructureManagerBase.instance.confirmPlacement();

                        // Send selected workers to build structure
                        if (newStructureObject) executeBuildCommand(newStructureObject);

                        ignoreNextMouse0Up = true;
                    }
                    if (cancelCommandKeyDown || mouse1Down) StructureManagerBase.instance.cancelPlacement();
                    break;

                case PlayerMode.NORMAL:
                    // Update mouse raycast for hovered object
                    SelectionControllerBase.instance.updateMouseHover();

                    // Check for squad input
                    if (squad1KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[0]);
                    if (squad2KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[1]);
                    if (squad3KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[2]);
                    if (squad4KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[3]);
                    if (squad5KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[4]);
                    if (squad6KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[5]);
                    if (squad7KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[6]);
                    if (squad8KeyDown) SelectionControllerBase.instance.retrieveSquad(SelectionManager.instance.boundSquads[7]);

                    // Set start position for mass select
                    if (mouse0Down && !EventSystem.current.IsPointerOverGameObject())
                    {
                        SelectionControllerBase.instance.massSelectStartPosition = Input.mousePosition;
                        canSelect = true;
                    }
                    if (mouse0Down && EventSystem.current.IsPointerOverGameObject())
                    {
                        canSelect = false;
                    }

                    // Detect start of mass select
                    if (InputManager.instance.getKey("Mouse0") && canSelect)
                    {
                        massSelectStartTimer += Time.deltaTime;
                        if (massSelectStartTimer >= SelectionControllerBase.instance.massSelectDelay ||
                            Vector3.Distance(SelectionControllerBase.instance.massSelectStartPosition, Input.mousePosition) > SelectionController.instance.massSelectMouseMoveDistance)
                        {
                            // Start mass select
                            playerMode = PlayerMode.MASS_SELECT;
                        }
                    }
                    else
                    {
                        // Reset mass select timer
                        massSelectStartTimer = 0.0f;
                        SelectionControllerBase.instance.massSelectStartPosition = Input.mousePosition;
                    }
                    SelectionControllerBase.instance.updateMassSelectBox();

                    // Execute single select
                    if (mouse0Up && canSelect && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Clear commands
                        CommandManagerBase.instance.clearCommands();

                        SelectionControllerBase.instance.executeSingleSelect();
                    }

                    // Right click commands
                    if (mouse1Down && !EventSystem.current.IsPointerOverGameObject())
                    {
                        // Raycast at mouse position to check what the player is hovering over
                        GameObject targetObject = InputManager.instance.mouseRaycastObject(unitLayer | structureLayer | resourceLayer);

                        if (targetObject != null) interactWithObject(targetObject);
                        // Default to move if no object hit
                        else executeMoveCommand(InputManager.instance.getMousePositionInWorld());
                    }
                    break;
            }

            // Don't allow start of mass select when occupied
            if (playerMode != PlayerMode.NORMAL && playerMode != PlayerMode.MASS_SELECT)
            {
                SelectionControllerBase.instance.massSelectStartPosition = Input.mousePosition;
                canSelect = false;
            }

            // Clear key states
            mouse0Down = false;
            mouse1Down = false;
            mouse0Up = false;
            mouse1Up = false;
            mouseScroll = 0;
            cancelCommandKeyDown = false;
            squad1KeyDown = false;
            squad2KeyDown = false;
            squad3KeyDown = false;
            squad4KeyDown = false;
            squad5KeyDown = false;
            squad6KeyDown = false;
            squad7KeyDown = false;
            squad8KeyDown = false;
        }

        public void cycleFocus()
        {
            // Find entity with old focused sorting priority
            // -1 if none found
            int oldFocusIndex = SelectionManager.instance.getFocusIndex();

            // If oldFocusIndex == -1, then focused sorting priority is no longer in selected objects, and we default to 0
            int newFocusIndex = 0;

            if (oldFocusIndex != -1)
            {
                // Find entity with new focus
                for (int i = oldFocusIndex; i < SelectionManager.instance.selectedObjects.Count; i++)
                {
                    if (SelectionManager.instance.selectedObjects[i].GetComponent<Entity>().sortPriority != SelectionManager.instance.focusSortPriority)
                    {
                        newFocusIndex = i;
                        break;
                    }
                }
                // If no new focus is found, defaults to 0
                // Also works in the case the current focus is the final selected type

                // Set new focus sorting priority
                SelectionManager.instance.focusSortPriority = SelectionManager.instance.selectedObjects[newFocusIndex].GetComponent<Entity>().sortPriority;
            }

            // Update commands
            SelectionControllerBase.instance.populateCommands();

            // Update UI
            SelectionDisplayManagerBase.instance.updateSelectionDisplayBoxes(false);
        }

        public void interactWithObject(GameObject targetObject)
        {
            if (targetObject == null) return;

            // Check what the object is
            Entity targetEntity = targetObject.GetComponent<Entity>();
            UnitBase targetUnit = targetObject.GetComponent<UnitBase>();
            Structure targetStructure = targetObject.GetComponent<Structure>();
            Resource targetResource = targetObject.GetComponent<Resource>();

            // Attack if hovering over enemy unit or structure
            if (targetEntity != null && targetObject.tag == enemyTag) executeAttackCommand(targetObject);
            // Harvest is hovering over resource
            else if (targetResource != null) executeHarvestCommand(targetObject);
            // Harvest is hovering over unfinished building
            else if (targetStructure != null && targetStructure.buildProgress < targetStructure.maxBuildProgress) executeBuildCommand(targetObject);
            // Move if none of the above
            else executeMoveCommand(InputManager.instance.getMousePositionInWorld());
        }

        #region Command executors

        public override void executeMoveCommand(Vector3 targetPosition)
        {
            // Make sure target is not out of bounds
            if (targetPosition.x < 0 || targetPosition.x > GridManagerBase.instance.mapXLength ||
                targetPosition.z < 0 || targetPosition.z > GridManagerBase.instance.mapZLength)
            {
                playerMode = PlayerMode.NORMAL;
                return;
            }

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a unit component
                UnitBase unit = selectedObject.GetComponent<UnitBase>();
                if (unit == null) continue;

                bool addToQueue = InputManager.instance.getKey("QueueCommand");
                unit.moveCommand(addToQueue, targetPosition);
            }

            // Finish command
            playerMode = PlayerMode.NORMAL;
        }

        public override void executeAttackMoveCommand(Vector3 targetPosition)
        {
            // Make sure target is not out of bounds
            if (targetPosition.x < 0 || targetPosition.x > GridManagerBase.instance.mapXLength ||
                targetPosition.z < 0 || targetPosition.z > GridManagerBase.instance.mapZLength)
            {
                playerMode = PlayerMode.NORMAL;
                return;
            }

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a unit component
                UnitBase unit = selectedObject.GetComponent<UnitBase>();
                if (unit == null) continue;

                bool addToQueue = InputManager.instance.getKey("QueueCommand");
                unit.attackMoveCommand(addToQueue, targetPosition);
            }

            // Finish command
            playerMode = PlayerMode.NORMAL;
        }

        public override void executeAttackCommand(GameObject targetObject)
        {
            if (targetObject == null) return;

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a unit component
                UnitBase unit = selectedObject.GetComponent<UnitBase>();
                if (unit == null) continue;

                bool addToQueue = InputManager.instance.getKey("QueueCommand");
                unit.attackCommand(addToQueue, targetObject);
            }

            // Finish attack command
            playerMode = PlayerMode.NORMAL;
        }

        public override void executeHarvestCommand(GameObject targetObject)
        {
            if (targetObject == null) return;

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a unit component
                UnitBase unit = selectedObject.GetComponent<UnitBase>();
                if (unit == null) continue;

                bool addToQueue = InputManager.instance.getKey("QueueCommand");
                unit.harvestCommand(addToQueue, targetObject);
            }

            // Finish attack command
            playerMode = PlayerMode.NORMAL;
        }

        public override void executeBuildCommand(GameObject targetObject)
        {
            if (targetObject == null) return;

            // Invoke command on all selected units
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                // Check that object has a unit component
                UnitBase unit = selectedObject.GetComponent<UnitBase>();
                if (unit == null) continue;

                bool addToQueue = InputManager.instance.getKey("QueueCommand");
                unit.buildCommand(addToQueue, targetObject);
            }

            // Finish attack command
            playerMode = PlayerMode.NORMAL;
        }

        #endregion
    }
}
