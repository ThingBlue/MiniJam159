using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using MiniJam159.UnitCore;
using MiniJam159.GameCore;
using UnityEngine.UI;

namespace MiniJam159.PlayerCore
{
    public class SelectionControllerBase : MonoBehaviour
    {
        #region Inspector members

        public LayerMask unitLayer;
        public LayerMask structureLayer;

        public RectTransform massSelectBoxTransform;
        public float massSelectDelay;
        public float massSelectMouseMoveDistance;
        public float selectionRaycastDistance;

        public bool drawMassSelectBoxCastGizmo;

        #endregion

        public GameObject hoveredObject;
        public Vector3 massSelectStartPosition;

        // Singleton
        public static SelectionControllerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            massSelectStartPosition = Vector3.zero;
        }

        protected virtual void Start()
        {
            // Subscribe to events
            EventManager.instance.openBuildMenuCommandEvent.AddListener(onOpenBuildMenuCommandCallback);
            EventManager.instance.cancelBuildMenuCommandEvent.AddListener(onCancelBuildMenuCommandCallback);
        }

        protected virtual void Update()
        {
            // DEBUG
            if (InputManager.instance.getKeyDown("CreateSquad")) createSquadFromCurrentSelection();

        }

        public virtual void updateMouseHover()
        {
            // Raycast from mouse and grab first hit
            LayerMask raycastMask = unitLayer | structureLayer;
            GameObject hitObject = InputManager.instance.mouseRaycastObject(raycastMask);

            // Handle outline of previous hovered object
            if (hoveredObject != null && hitObject != hoveredObject)
            {
                // Reset outline of previous object
                if (SelectionManager.instance.selectedObjects.Contains(hoveredObject))
                {
                    // Set outline back to selected
                    hoveredObject.GetComponent<Entity>().setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.selectedOutlineColor);
                }
                else
                {
                    // Clear outline from previous hovered object
                    hoveredObject.GetComponent<Entity>().clearOutline(SelectionManager.instance.selectedOutlineMaterial);
                }
            }

            // Handle outline of new hovered object
            if (hitObject != null)// && hitObject != hoveredObject)
            {
                Entity hitEntity = hitObject.GetComponent<Entity>();
                if (hitEntity == null) Debug.Log("hitEntity is null for object " + hitObject);

                // Add outline to new hovered object
                if (InputManager.instance.getKey("Deselect"))
                {
                    // Only apply deselect outline if object is selected
                    if (SelectionManager.instance.selectedObjects.Contains(hitObject))
                    {
                        // Deselect
                        hitEntity.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.deselectOutlineColor);
                    }
                    // Deselecting but current object is not selected
                    else
                    {
                        // Clear outline
                        hitEntity.clearOutline(SelectionManager.instance.selectedOutlineMaterial);
                    }
                }
                else
                {
                    // Regular hover
                    hitEntity.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.hoveredOutlineColor);
                }
            }

            // Set hovered object
            hoveredObject = hitObject;
        }

        public virtual void updateMassSelectBox()
        {
            // See SelectionController::updateMassSelectBox()
        }

        public virtual void executeSingleSelect()
        {
            // See SelectionController::executeSingleSelect()
        }

        public virtual void executeMassSelect()
        {
            // See SelectionController::executeMassSelect()
        }

        // Takes a list of new selected objects and adds them to selection manager
        public virtual void executeSelect(List<GameObject> newSelection)
        {
            // See SelectionController::executeSelect(List<GameObject> newSelection)
        }

        public virtual void reselectSingle(int index)
        {
            // See SelectionController::reselectSingle(int index)
        }

        public virtual void reselectType(int index)
        {
            // See SelectionController::reselectType(int index)
        }

        public virtual void deselectSingle(int index)
        {
            // See SelectionController::deselectSingle(int index)
        }

        public virtual void deselectType(int index)
        {
            // See SelectionController::deselectType(int index)
        }

        public virtual void sortSelection()
        {
            // See SelectionController::sortSelection()
        }

        public virtual void populateCommands()
        {
            // See SelectionController::populateCommands()
        }

        public virtual void createSquadFromCurrentSelection()
        {
            // See SelectionController::createSquadFromCurrentSelection()
        }

        public virtual void addToSquad()
        {
            // See SelectionController::addToSquad()
        }

        public virtual void retrieveSquad(Squad squad)
        {
            // See SelectionController::replaceSelectionWithSquad(Squad squad)
        }

        protected virtual void onOpenBuildMenuCommandCallback()
        {
            // First selected unit must be a worker
            if (SelectionManager.instance.selectedObjects.Count == 0) return;

            GameObject selectedObject = SelectionManager.instance.selectedObjects[SelectionManager.instance.getFocusIndex()];
            if (selectedObject == null) return;

            UnitBase selectedUnit = selectedObject.GetComponent<UnitBase>();
            if (selectedUnit == null) return;

            // Populate commands using worker's structure data list
            MethodInfo method = selectedUnit.GetType().GetMethod("openBuildMenuCommand");
            if (method != null)
            {
                // Invoke attack command method in ai using transform of target
                method.Invoke(selectedUnit, new object[] { });
            }
        }

        protected virtual void onCancelBuildMenuCommandCallback()
        {
            populateCommands();
        }

    }
}
