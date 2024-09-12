using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.AICore;
using MiniJam159.GameCore;
using UnityEngine.UI;
using System.Reflection;

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

        public virtual void populateCommands(int focusIndex = 0)
        {
            // See SelectionController::populateCommands(int focusIndex = 0)
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

            GameAI selectedUnit = selectedObject.GetComponent<GameAI>();
            if (selectedUnit == null) return;

            // Populate commands using worker's structure data list
            MethodInfo method = selectedUnit.GetType().GetMethod("openBuildMenuAICommand");
            if (method != null)
            {
                // Invoke attack command method in ai using transform of target
                method.Invoke(selectedUnit, new object[] { });
            }
        }

        protected virtual void onCancelBuildMenuCommandCallback()
        {
            populateCommands(SelectionManager.instance.getFocusIndex());
        }

        protected virtual void OnDrawGizmos()
        {
            if (drawMassSelectBoxCastGizmo)
            {
                // Transform position into world space
                // Find boundaries of selection box in screen space
                //Vector3 center = massSelectBoxTransform.position + ((massSelectBoxTransform.position + (Vector3)massSelectBoxTransform.sizeDelta) - massSelectBoxTransform.position) / 2f;
                Vector3 bottomLeft = massSelectBoxTransform.position;
                Vector3 bottomRight = massSelectBoxTransform.position + new Vector3(massSelectBoxTransform.sizeDelta.x, 0, 0);
                Vector3 topRight = massSelectBoxTransform.position + (Vector3)massSelectBoxTransform.sizeDelta;
                Vector3 topLeft = massSelectBoxTransform.position + new Vector3(0, massSelectBoxTransform.sizeDelta.y, 0);

                Plane worldPlane = new Plane(Vector3.up, Vector3.zero);

                // Transform position into world space
                //Ray centerRay = Camera.main.ScreenPointToRay(center);
                Ray bottomLeftRay = Camera.main.ScreenPointToRay(bottomLeft);
                Ray bottomRightRay = Camera.main.ScreenPointToRay(bottomRight);
                Ray topRightRay = Camera.main.ScreenPointToRay(topRight);
                Ray topLeftRay = Camera.main.ScreenPointToRay(topLeft);
                //Vector3 centerWorldSpace = Vector3.zero;
                Vector3 bottomLeftWorldSpace = Vector3.zero;
                Vector3 bottomRightWorldSpace = Vector3.zero;
                Vector3 topRightWorldSpace = Vector3.zero;
                Vector3 topLeftWorldSpace = Vector3.zero;
                //if (worldPlane.Raycast(centerRay, out float centerEnter)) centerWorldSpace = centerRay.GetPoint(centerEnter);
                if (worldPlane.Raycast(bottomLeftRay, out float bottomLeftEnter)) bottomLeftWorldSpace = bottomLeftRay.GetPoint(bottomLeftEnter);
                if (worldPlane.Raycast(bottomRightRay, out float bottomRightEnter)) bottomRightWorldSpace = bottomRightRay.GetPoint(bottomRightEnter);
                if (worldPlane.Raycast(topRightRay, out float topRightEnter)) topRightWorldSpace = topRightRay.GetPoint(topRightEnter);
                if (worldPlane.Raycast(topLeftRay, out float topLeftEnter)) topLeftWorldSpace = topLeftRay.GetPoint(topLeftEnter);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(bottomLeftWorldSpace, 1.0f);
                Gizmos.DrawWireSphere(bottomRightWorldSpace, 1.0f);
                Gizmos.DrawWireSphere(topLeftWorldSpace, 1.0f);
                Gizmos.DrawWireSphere(topRightWorldSpace, 1.0f);
            }

        }
    }
}
