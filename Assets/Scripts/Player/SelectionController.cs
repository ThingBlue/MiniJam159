using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using MiniJam159.AICore;
using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.Structures;
using System;

namespace MiniJam159.Player
{
    public class SelectionController : MonoBehaviour
    {
        #region Inspector members

        public RectTransform massSelectBoxTransform;
        public float massSelectDelay;
        public float massSelectMouseMoveDistance;

        public LayerMask unitLayer;
        public LayerMask structureLayer;

        public float selectionRaycastDistance;

        #endregion

        public Vector3 massSelectStartPosition;

        // Singleton
        public static SelectionController instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            massSelectStartPosition = Vector3.zero;
        }

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.openBuildMenuCommandEvent.AddListener(onOpenBuildMenuCommandCallback);
            EventManager.instance.cancelBuildMenuCommandEvent.AddListener(onCancelBuildMenuCommandCallback);
        }

        public void updateMassSelectBox()
        {
            bool massSelecting = (PlayerModeManager.instance.playerMode == PlayerMode.MASS_SELECT);
            massSelectBoxTransform.gameObject.GetComponent<Image>().enabled = massSelecting;
            if (!massSelecting) return;

            Vector2 bottomLeft = Vector2.zero;
            bottomLeft.x = Mathf.Min(massSelectStartPosition.x, Input.mousePosition.x);
            bottomLeft.y = Mathf.Min(massSelectStartPosition.y, Input.mousePosition.y);
            Vector2 topRight = Vector2.zero;
            topRight.x = Mathf.Max(massSelectStartPosition.x, Input.mousePosition.x);
            topRight.y = Mathf.Max(massSelectStartPosition.y, Input.mousePosition.y);

            massSelectBoxTransform.position = bottomLeft;
            massSelectBoxTransform.sizeDelta = topRight - bottomLeft;

            // Add outlines to entities inside the box and remove outlines for entities outside the box
            List<Vector2> castPoints = getMassSelectionBoxPoints();
            List<Vector2> castNormals = getMassSelectionBoxNormals(castPoints);
            foreach (GameAI unit in GameAI.allAIs)
            {
                if (unit.insideCast(castPoints, castNormals))
                {
                    // Hovered
                    unit.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.hoveredOutlineColor);
                }
                else if (SelectionManager.instance.selectedObjects.Contains(unit.gameObject))
                {
                    // Selected already
                    unit.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.selectedOutlineColor);
                }
                else
                {
                    // None, clear outline
                    unit.clearOutline(SelectionManager.instance.selectedOutlineMaterial);
                }
            }
        }

        public void updateMouseHover()
        {

        }

        public void executeSingleSelect()
        {
            // Keep track of previous selection
            List<GameObject> previousSelection = new List<GameObject>(SelectionManager.instance.selectedObjects);

            // Clear current selection regardless of if we hit amything
            SelectionManager.instance.clearSelectedObjects();

            // Clear UI
            EventManager.instance.selectionStartEvent.Invoke();

            // Raycast from mouse and grab first hit
            LayerMask raycastMask = unitLayer | structureLayer;
            GameObject hitObject = InputManager.instance.mouseRaycastObject(raycastMask);

            // No hits
            if (hitObject == null)
            {
                // Clear selection and return
                SelectionManager.instance.clearSelectedObjects();
                EventManager.instance.selectionCompleteEvent.Invoke();
                return;
            }

            // If we hit a child, we want to grab the parent
            //if (hitObject.GetComponent<Entity>() == null) hitObject = hitObject.transform.parent.gameObject;

            // Get key states
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool addToSelectionKey = InputManager.instance.getKey("AddToSelection");

            // We will be working with previous selection if either key is true
            if (deselectKey || addToSelectionKey) SelectionManager.instance.selectedObjects = previousSelection;

            // Behaviour based on whether the object we clicked is already selected
            if (deselectKey && addToSelectionKey)
            {
                // Remove object from selection
                if (SelectionManager.instance.selectedObjects.Contains(hitObject)) SelectionManager.instance.selectedObjects.Remove(hitObject);
                // Add object to selection
                else SelectionManager.instance.selectedObjects.Add(hitObject);
            }
            // Deselect from previous selection
            else if (deselectKey) SelectionManager.instance.selectedObjects.Remove(hitObject);
            // Add to previous selection
            else if (addToSelectionKey)
            {
                // Prevent adding a duplicate
                if (!SelectionManager.instance.selectedObjects.Contains(hitObject)) SelectionManager.instance.selectedObjects.Add(hitObject);
            }
            // Default: Replace selection
            else SelectionManager.instance.selectedObjects.Add(hitObject);

            SelectionManager.instance.addOutlinesToSelectedObjects(SelectionManager.instance.selectedOutlineColor);

            EventManager.instance.selectionCompleteEvent.Invoke();

            // Populate commands after sorting
            populateCommands();
        }

        public void executeMassSelect()
        {
            // Keep track of previous selection
            List<GameObject> previousSelection = new List<GameObject>(SelectionManager.instance.selectedObjects);

            // Clear current selection
            //SelectionManager.instance.clearSelectedObjects();

            // Clear UI
            EventManager.instance.selectionStartEvent.Invoke();

            List<Vector2> castPoints = getMassSelectionBoxPoints();
            List<Vector2> castNormals = getMassSelectionBoxNormals(castPoints);

            // Create new selection
            List<GameObject> newSelection = new List<GameObject>();

            // NOT ALLOWING MASS SELECT ON STRUCTURES FOR NOW
            /*
            foreach (GameObject structureObject in StructureManager.instance.structures)
            {
                // Find corners of structure
                StructureData structureData = structureObject.GetComponent<Structure>().structureData;
                List<Vector2> structurePoints = new List<Vector2>();
                structurePoints.Add(structureData.position + (structureData.size / 2f));
                structurePoints.Add(structureData.position + new Vector2(structureData.size.x / 2f, 0) - new Vector2(0, structureData.size.y / 2f));
                structurePoints.Add(structureData.position - (structureData.size / 2f));
                structurePoints.Add(structureData.position - new Vector2(structureData.size.x / 2f, 0) + new Vector2(0, structureData.size.y / 2f));

                // Find all normals
                List<Vector2> allNormals = new List<Vector2>(normals);
                allNormals.Add(Vector2.Perpendicular(structurePoints[0] - structurePoints[structurePoints.Count - 1]).normalized);
                for (int i = 0; i < structurePoints.Count - 1; i++)
                {
                    allNormals.Add(Vector2.Perpendicular(structurePoints[i + 1] - structurePoints[i]).normalized);
                }

                // Loop over all normals
                bool separated = false;
                foreach (Vector2 normal in allNormals)
                {
                    // Project shapes onto normal
                    float castMin = float.MaxValue;
                    float castMax = float.MinValue;
                    foreach (Vector2 point in castPoints)
                    {
                        Vector2 projectedPoint = closestPointOnNormal(normal, point);
                        float distance = Vector2.Distance(Vector2.zero, projectedPoint);
                        if (Vector2.Dot(projectedPoint, normal) < 0) distance = -distance;
                        if (distance < castMin) castMin = distance;
                        if (distance > castMax) castMax = distance;
                    }
                    float structureMin = float.MaxValue;
                    float structureMax = float.MinValue;
                    foreach (Vector2 point in structurePoints)
                    {
                        Vector2 projectedPoint = closestPointOnNormal(normal, point);
                        float distance = Vector2.Distance(Vector2.zero, projectedPoint);
                        if (Vector2.Dot(projectedPoint, normal) < 0) distance = -distance;
                        if (distance < structureMin) structureMin = distance;
                        if (distance > structureMax) structureMax = distance;
                    }

                    // Check if projected shapes overlap
                    if (castMax < structureMin || structureMax < castMin)
                    {
                        separated = true;
                        break;
                    }
                }

                if (!separated)
                {
                    // Inside selection if not separated
                    newSelection.Add(structureObject);
                }
            }
            */

            // Loop through all units
            foreach (GameAI unitAI in GameAI.allAIs)
            {
                GameObject unitObject = unitAI.gameObject;
                if (unitAI.insideCast(castPoints, castNormals)) newSelection.Add(unitObject);
            }

            // Get key states
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool addToSelectionKey = InputManager.instance.getKey("AddToSelection");

            // We will be working with previous selection if either key is true
            if (deselectKey || addToSelectionKey)
            {
                SelectionManager.instance.selectedObjects = previousSelection;
            }
            // Clear previous selection if neither key is true
            else
            {
                //SelectionManager.instance.focusSortPriority = -1;
                SelectionManager.instance.clearSelectedObjects();
            }

            // Loop through all new hit objects
            foreach (GameObject hitObject in newSelection)
            {
                // Behaviour based on whether the object we clicked is already selected
                if (deselectKey && addToSelectionKey)
                {
                    // We're going to be working with the previous selection in either case
                    SelectionManager.instance.selectedObjects = previousSelection;

                    // Remove object from selection
                    if (SelectionManager.instance.selectedObjects.Contains(hitObject))
                    {
                        SelectionManager.instance.selectedObjects.Remove(hitObject);

                        // Clear outline
                    }
                    // Add object to selection
                    else SelectionManager.instance.selectedObjects.Add(hitObject);
                }
                // Deselect from previous selection
                else if (deselectKey) SelectionManager.instance.selectedObjects.Remove(hitObject);
                // Add to previous selection
                else if (addToSelectionKey)
                {
                    if (!SelectionManager.instance.selectedObjects.Contains(hitObject)) SelectionManager.instance.selectedObjects.Add(hitObject);
                }
                // Default: Replace selection
                else SelectionManager.instance.selectedObjects.Add(hitObject);
            }

            SelectionManager.instance.addOutlinesToSelectedObjects(SelectionManager.instance.selectedOutlineColor);

            EventManager.instance.selectionCompleteEvent.Invoke();

            // Populate commands after sorting
            populateCommands();

            // Reset mass select
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
        }

        public List<Vector2> getMassSelectionBoxPoints()
        {
            // Find boundaries of selection box in screen space
            Vector3 bottomLeft = massSelectBoxTransform.position;
            Vector3 bottomRight = massSelectBoxTransform.position + new Vector3(massSelectBoxTransform.sizeDelta.x, 0, 0);
            Vector3 topRight = massSelectBoxTransform.position + (Vector3)massSelectBoxTransform.sizeDelta;
            Vector3 topLeft = massSelectBoxTransform.position + new Vector3(0, massSelectBoxTransform.sizeDelta.y, 0);

            Plane worldPlane = new Plane(Vector3.up, Vector3.zero);

            // Transform position into world space
            Ray bottomLeftRay = Camera.main.ScreenPointToRay(bottomLeft);
            Ray bottomRightRay = Camera.main.ScreenPointToRay(bottomRight);
            Ray topRightRay = Camera.main.ScreenPointToRay(topRight);
            Ray topLeftRay = Camera.main.ScreenPointToRay(topLeft);
            Vector3 bottomLeftWorldSpace = Vector3.zero;
            Vector3 bottomRightWorldSpace = Vector3.zero;
            Vector3 topRightWorldSpace = Vector3.zero;
            Vector3 topLeftWorldSpace = Vector3.zero;
            if (worldPlane.Raycast(bottomLeftRay, out float bottomLeftEnter)) bottomLeftWorldSpace = bottomLeftRay.GetPoint(bottomLeftEnter);
            if (worldPlane.Raycast(bottomRightRay, out float bottomRightEnter)) bottomRightWorldSpace = bottomRightRay.GetPoint(bottomRightEnter);
            if (worldPlane.Raycast(topRightRay, out float topRightEnter)) topRightWorldSpace = topRightRay.GetPoint(topRightEnter);
            if (worldPlane.Raycast(topLeftRay, out float topLeftEnter)) topLeftWorldSpace = topLeftRay.GetPoint(topLeftEnter);

            List<Vector2> castPoints = new List<Vector2>();
            castPoints.Add(new Vector2(bottomLeftWorldSpace.x, bottomLeftWorldSpace.z));
            castPoints.Add(new Vector2(bottomRightWorldSpace.x, bottomRightWorldSpace.z));
            castPoints.Add(new Vector2(topRightWorldSpace.x, topRightWorldSpace.z));
            castPoints.Add(new Vector2(topLeftWorldSpace.x, topLeftWorldSpace.z));

            return castPoints;
        }

        public List<Vector2> getMassSelectionBoxNormals(List<Vector2> castPoints)
        {
            // Find all normals in casted quadrilateral
            List<Vector2> castNormals = new List<Vector2>();
            castNormals.Add(Vector2.Perpendicular(castPoints[0] - castPoints[castPoints.Count - 1]).normalized);
            for (int i = 0; i < castPoints.Count - 1; i++)
            {
                castNormals.Add(Vector2.Perpendicular(castPoints[i + 1] - castPoints[i]).normalized);
            }
            return castNormals;
        }

        private Vector2 closestPointOnNormal(Vector2 normal, Vector2 point)
        {
            Vector2 normalized = normal.normalized;
            float d = Vector2.Dot(point, normalized);
            return normalized * d;
        }

        public void reselectSingle(int index)
        {
            // Store the one object we want to keep
            GameObject targetObject = SelectionManager.instance.selectedObjects[index];

            // Clear UI
            EventManager.instance.selectionStartEvent.Invoke();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add object back in
            SelectionManager.instance.selectedObjects.Add(targetObject);
            SelectionManager.instance.addOutlinesToSelectedObjects(SelectionManager.instance.selectedOutlineColor);

            // Sort
            EventManager.instance.selectionCompleteEvent.Invoke();

            // Populate commands after sorting
            populateCommands(SelectionManager.instance.getFocusIndex());
        }

        public void reselectType(int index)
        {
            // Store the objects we want to keep
            List<GameObject> reselectedObjects = new List<GameObject>();

            // Get sorting priority of target
            GameObject targetObject = SelectionManager.instance.selectedObjects[index];
            Entity targetEntity = targetObject.GetComponent<Entity>();

            // Get all objects with the same sorting priority
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                Entity selectedEntity = selectedObject.GetComponent<Entity>();
                if (selectedEntity.sortPriority == targetEntity.sortPriority)
                {
                    reselectedObjects.Add(selectedObject);
                }
            }

            // Clear UI
            EventManager.instance.selectionStartEvent.Invoke();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add objects back in
            SelectionManager.instance.selectedObjects = new List<GameObject>(reselectedObjects);
            SelectionManager.instance.addOutlinesToSelectedObjects(SelectionManager.instance.selectedOutlineColor);

            // Sort
            EventManager.instance.selectionCompleteEvent.Invoke();

            // Populate commands after sorting
            populateCommands(SelectionManager.instance.getFocusIndex());
        }

        public void deselectSingle(int index)
        {
            // Store the objects we want to keep
            List<GameObject> reselectedObjects = new List<GameObject>(SelectionManager.instance.selectedObjects);

            // Remove target object
            GameObject targetObject = SelectionManager.instance.selectedObjects[index];
            reselectedObjects.Remove(targetObject);

            // Clear UI
            EventManager.instance.selectionStartEvent.Invoke();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add object back in
            SelectionManager.instance.selectedObjects = new List<GameObject>(reselectedObjects);
            SelectionManager.instance.addOutlinesToSelectedObjects(SelectionManager.instance.selectedOutlineColor);

            // Sort
            EventManager.instance.selectionCompleteEvent.Invoke();

            // Populate commands after sorting
            populateCommands(SelectionManager.instance.getFocusIndex());
        }

        public void deselectType(int index)
        {
            // Store the objects we want to keep
            List<GameObject> reselectedObjects = new List<GameObject>(SelectionManager.instance.selectedObjects);

            // Get sorting priority of target
            GameObject targetObject = SelectionManager.instance.selectedObjects[index];
            Entity targetEntity = targetObject.GetComponent<Entity>();

            // Remove all objects with the same sorting priority
            reselectedObjects.RemoveAll(selectedObject =>
                selectedObject.GetComponent<Entity>().sortPriority == targetEntity.sortPriority
            );

            // Clear UI
            EventManager.instance.selectionStartEvent.Invoke();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add objects back in
            SelectionManager.instance.selectedObjects = new List<GameObject>(reselectedObjects);
            SelectionManager.instance.addOutlinesToSelectedObjects(SelectionManager.instance.selectedOutlineColor);

            // Sort
            EventManager.instance.selectionCompleteEvent.Invoke();

            // Populate commands after sorting
            populateCommands(SelectionManager.instance.getFocusIndex());
        }

        public void populateCommands(int focusIndex = 0)
        {
            if (SelectionManager.instance.selectedObjects.Count < focusIndex + 1) return;
            if (focusIndex == -1) return;

            // Populate command menu using the first object in list
            GameObject selectedObject = SelectionManager.instance.selectedObjects[focusIndex];
            if (selectedObject == null) return;

            if (selectedObject.layer == LayerMask.NameToLayer("Unit"))
            {
                GameAI newUnit = selectedObject.GetComponent<GameAI>();
                newUnit.populateCommands();
            }
            else if (selectedObject.layer == LayerMask.NameToLayer("Structure"))
            {
                Structure newStructure = selectedObject.GetComponent<Structure>();
                newStructure.populateCommands();
            }
        }

        private void onOpenBuildMenuCommandCallback()
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

        private void onCancelBuildMenuCommandCallback()
        {
            populateCommands(SelectionManager.instance.getFocusIndex());
        }

        private void OnDrawGizmos()
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
