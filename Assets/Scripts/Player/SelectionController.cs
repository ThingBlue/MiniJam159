using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

using MiniJam159.UnitCore;
using MiniJam159.StructureCore;
using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.UICore;
using MiniJam159.CommandCore;

namespace MiniJam159.Player
{
    public class SelectionController : SelectionControllerBase
    {
        #region Inspector members

        public LayerMask unitLayer;
        public LayerMask structureLayer;

        public RectTransform massSelectBoxTransform;
        public float selectionRaycastDistance;

        public bool drawMassSelectBoxCastGizmo;

        #endregion

        public GameObject hoveredObject;

        protected virtual void Update()
        {
            // DEBUG
            if (InputManager.instance.getKeyDown("CreateSquad")) createSquadFromCurrentSelection();

        }

        public override void updateMouseHover()
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

        public override void updateMassSelectBox()
        {
            bool massSelecting = (PlayerControllerBase.instance.playerMode == PlayerMode.MASS_SELECT);
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

            List<Vector2> castPoints = getMassSelectionBoxPoints();
            List<Vector2> castNormals = getMassSelectionBoxNormals(castPoints);

            // Get key states
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool addToSelectionKey = InputManager.instance.getKey("AddToSelection");

            // Separate entities into 2 lists, one for entities in the box and one for entities outside
            List<GameObject> entitiesInsideBox = new List<GameObject>();
            List<GameObject> entitiesOutsideBox = new List<GameObject>();
            bool unitInBox = false;
            foreach (GameObject entityObject in EntityManager.instance.playerEntityObjects)
            {
                Entity entity = entityObject.GetComponent<Entity>();
                if (entity.insideCast(castPoints, castNormals))
                {
                    entitiesInsideBox.Add(entityObject);
                    if (entityObject.GetComponent<UnitBase>() != null) unitInBox = true;
                }
                else
                {
                    entitiesOutsideBox.Add(entityObject);
                }
            }

            // If there is a unit in the box that is not in our current selection, we will ignore structures in the box
            bool includeStructuresInAddition = true;
            if (addToSelectionKey)
            {
                foreach (GameObject entityObject in entitiesInsideBox)
                {
                    UnitBase unit = entityObject.GetComponent<UnitBase>();
                    if (unit != null && !SelectionManager.instance.selectedObjects.Contains(entityObject))
                    {
                        // There is a unit in the box that is not in our current selection
                        includeStructuresInAddition = false;
                        break;
                    }
                }
            }

            // Conditions:
            // When in standard mode, we remove structures if there are any units in the box
            // When in addition mode, we remove structures if there are any units in the box that are not already in our current selection
            // When deselecting, we always include structures
            if (((unitInBox && !addToSelectionKey) || (addToSelectionKey && !includeStructuresInAddition)) && !deselectKey)
            {
                // Remove all structures if conditions met
                for (int i = entitiesInsideBox.Count - 1; i >= 0; i--)
                {
                    GameObject entityObject = entitiesInsideBox[i];

                    if (entityObject.GetComponent<Structure>() != null)
                    {
                        // Move entity to outside list
                        entitiesOutsideBox.Add(entitiesInsideBox[i]);
                        entitiesInsideBox.RemoveAt(i);
                    }
                }
            }

            // Add/clears outlines to objects inside box
            foreach (GameObject entityObject in entitiesInsideBox)
            {
                Entity entity = entityObject.GetComponent<Entity>();
                if (deselectKey)
                {
                    // Only apply deselect outline if object is selected
                    if (SelectionManager.instance.selectedObjects.Contains(entityObject))
                    {
                        // Deselecting
                        entity.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.deselectOutlineColor);
                    }
                    // Deselecting but current object is not selected
                    else
                    {
                        // Clear outline
                        entity.clearOutline(SelectionManager.instance.selectedOutlineMaterial);
                    }
                }
                else
                {
                    // Regular hover
                    entity.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.hoveredOutlineColor);
                }
            }

            // Add/clear outlines to objects outside box
            foreach (GameObject entityObject in entitiesOutsideBox)
            {
                Entity entity = entityObject.GetComponent<Entity>();
                if (SelectionManager.instance.selectedObjects.Contains(entityObject))
                {
                    // Selected already
                    entity.setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.selectedOutlineColor);
                }
                else
                {
                    // None, clear outline
                    entity.clearOutline(SelectionManager.instance.selectedOutlineMaterial);
                }
            }
        }

        public override void executeSingleSelect()
        {
            // Raycast from mouse and grab first hit
            LayerMask raycastMask = unitLayer | structureLayer;
            List<GameObject> hitObjects = InputManager.instance.mouseRaycastAll(raycastMask);
            List<GameObject> newSelection = new List<GameObject>();

            // Get first hit structure and first hit unit
            GameObject firstHitStructure = null;
            GameObject firstHitUnit = null;
            foreach (GameObject hitObject in hitObjects)
            {
                if (hitObject.GetComponent<Structure>() != null && firstHitStructure == null) firstHitStructure = hitObject;
                if (hitObject.GetComponent<UnitBase>() != null && firstHitUnit == null) firstHitUnit = hitObject;
            }

            // Get key states
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool addToSelectionKey = InputManager.instance.getKey("AddToSelection");

            // Determine what our selected object should be between structure and unit
            // No unit hit
            if (firstHitUnit == null) newSelection.Add(firstHitStructure);
            // No structure hit
            else if (firstHitStructure == null) newSelection.Add(firstHitUnit);
            // Hit both unit and structure
            else if (deselectKey)
            {
                // Structure selected, deselect structure
                if (SelectionManager.instance.selectedObjects.Contains(firstHitStructure)) newSelection.Add(firstHitStructure);
                else newSelection.Add(firstHitUnit);
            }
            else if (!deselectKey)
            {
                // Structure already selected, switch to unit
                if (SelectionManager.instance.selectedObjects.Contains(firstHitStructure)) newSelection.Add(firstHitUnit);
                // Prioritize sturcture
                else newSelection.Add(firstHitStructure);
            }

            // Handle selection
            executeSelect(newSelection);
        }

        public override void executeMassSelect()
        {
            // Clear UI
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            List<Vector2> castPoints = getMassSelectionBoxPoints();
            List<Vector2> castNormals = getMassSelectionBoxNormals(castPoints);

            // Create new selection
            List<GameObject> newSelection = new List<GameObject>();

            // Loop through all player entities
            bool unitInBox = false;
            foreach (GameObject entityObject in EntityManager.instance.playerEntityObjects)
            {
                Entity entity = entityObject.GetComponent<Entity>();
                if (entity.insideCast(castPoints, castNormals))
                {
                    newSelection.Add(entityObject);
                    if (entityObject.GetComponent<UnitBase>() != null) unitInBox = true;
                }
            }

            // Get key states
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool addToSelectionKey = InputManager.instance.getKey("AddToSelection");

            // If there is a unit in the box that is not in our current selection, we will ignore structures in the box
            bool includeStructuresInAddition = true;
            if (addToSelectionKey)
            {
                foreach (GameObject entityObject in newSelection)
                {
                    UnitBase unit = entityObject.GetComponent<UnitBase>();
                    if (unit != null && !SelectionManager.instance.selectedObjects.Contains(entityObject))
                    {
                        // There is a unit in the box that is not in our current selection
                        includeStructuresInAddition = false;
                        break;
                    }
                }
            }

            // Conditions:
            // When in standard mode, we remove structures if there are any units in the box
            // When in addition mode, we remove structures if there are any units in the box that are not already in our current selection
            // When deselecting, we always include structures
            if (((unitInBox && !addToSelectionKey) || (addToSelectionKey && !includeStructuresInAddition)) && !deselectKey)

            // Clear structures if any units are in the selection box
            //if (!deselectKey && (!addToSelectionKey && unitInBox))
            {
                newSelection.RemoveAll(gameObject => gameObject.GetComponent<Structure>() != null);
            }

            // Handle selection
            executeSelect(newSelection);

            // Reset mass select
            PlayerControllerBase.instance.playerMode = PlayerMode.NORMAL;
        }

        // Takes a list of new selected objects and adds them to selection manager
        protected virtual void executeSelect(List<GameObject> newSelection)
        {
            // Clear UI
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Get key states
            bool deselectKey = InputManager.instance.getKey("Deselect");
            bool addToSelectionKey = InputManager.instance.getKey("AddToSelection");

            // Replace previous selection if no extra key is true
            if (!deselectKey && !addToSelectionKey) SelectionManager.instance.clearSelectedObjects();

            // Loop through all new hit objects
            foreach (GameObject hitObject in newSelection)
            {
                // Behaviour based on whether the object we clicked is already selected
                if (deselectKey && addToSelectionKey)
                {
                    // Remove object from selection
                    if (SelectionManager.instance.selectedObjects.Contains(hitObject)) SelectionManager.instance.removeSelectedObject(hitObject);
                    // Add object to selection
                    else SelectionManager.instance.addSelectedObject(hitObject);
                }
                // Deselect from previous selection
                else if (deselectKey)
                {
                    SelectionManager.instance.removeSelectedObject(hitObject);
                }
                // Add to previous selection
                else if (addToSelectionKey)
                {
                    // Object not in selection yet, add it and give it selected outline
                    if (!SelectionManager.instance.selectedObjects.Contains(hitObject)) SelectionManager.instance.addSelectedObject(hitObject);
                    // Object is already in selection, set to selected outline
                    else hitObject.GetComponent<Entity>().setOutline(SelectionManager.instance.selectedOutlineMaterial, SelectionManager.instance.selectedOutlineColor);
                }
                // Default: Replace selection
                else
                {
                    SelectionManager.instance.addSelectedObject(hitObject);
                }
            }

            // Sort and do anything that needs to be done after selection finishes
            postSelection();
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

        public override void reselectSingle(int index)
        {
            // Store the one object we want to keep
            GameObject targetObject = SelectionManager.instance.selectedObjects[index];

            // Clear UI
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add object back in
            SelectionManager.instance.addSelectedObject(targetObject);

            // Sort and do anything that needs to be done after selection finishes
            postSelection();
        }

        public override void reselectType(int index)
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
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add objects back in
            SelectionManager.instance.setSelectedObjects(reselectedObjects);

            // Sort and do anything that needs to be done after selection finishes
            postSelection();
        }

        public override void deselectSingle(int index)
        {
            // Remove target object
            SelectionManager.instance.removeSelectedObject(SelectionManager.instance.selectedObjects[index]);

            // Clear UI
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Sort and do anything that needs to be done after selection finishes
            postSelection();
        }

        public override void deselectType(int index)
        {
            Entity targetEntity = SelectionManager.instance.selectedObjects[index].GetComponent<Entity>();

            // Get all objects with the same sorting priority as target
            List<GameObject> objectsToDeselect = new List<GameObject>();
            foreach (GameObject selectedObject in SelectionManager.instance.selectedObjects)
            {
                if (selectedObject.GetComponent<Entity>().sortPriority == targetEntity.sortPriority) objectsToDeselect.Add(selectedObject);
            }

            // Remove all objects with same sorting priority as target
            foreach (GameObject objectToDeselect in objectsToDeselect) SelectionManager.instance.removeSelectedObject(objectToDeselect);

            // Clear UI
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Sort and do anything that needs to be done after selection finishes
            postSelection();
        }

        public override void createSquadFromCurrentSelection()
        {
            // Create new squad
            Squad newSquad = new Squad(SelectionManager.instance.assignSquadId(), SelectionManager.instance.selectedObjects);
            SelectionManager.instance.squads.Add(newSquad);

            // Create new squad icon in squads UI
            GameObject newSquadDisplayBox = SquadPanelManagerBase.instance.createSquadDisplayBox(newSquad);
        }

        public virtual void addToSquad()
        {

        }

        public override void retrieveSquad(Squad squad)
        {
            // Clear UI
            SelectionDisplayManagerBase.instance.clearSelectionDisplayBoxes();
            CommandPanelManagerBase.instance.clearCommandButtons();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add objects back in from squad
            if (squad != null) SelectionManager.instance.setSelectedObjects(squad.entities);

            // Sort and do anything that needs to be done after selection finishes
            postSelection();
        }

        private void postSelection()
        {
            // Sort
            sortSelection();

            // Populate commands after sorting
            populateCommands();

            // Action indicators may be out of date, refresh
            ActionIndicatorManagerBase.instance.refreshActionIndicators();
        }

        public override void sortSelection()
        {
            SelectionManager.instance.selectedObjects.Sort(new EntityGameObjectComparer());

            // Clear focus if nothing is selected
            if (SelectionManager.instance.selectedObjects.Count == 0) SelectionManager.instance.focusSortPriority = 0;
            // Set focus to first entity is there is none
            if (SelectionManager.instance.selectedObjects.Count > 0 && SelectionManager.instance.getFocusIndex() == -1)
            {
                SelectionManager.instance.focusSortPriority = SelectionManager.instance.selectedObjects[0].GetComponent<Entity>().sortPriority;
            }

            // Create display boxes
            SelectionDisplayManagerBase.instance.showSelectionDisplayBoxes();
        }

        public override void populateCommands()
        {
            int focusIndex = SelectionManager.instance.getFocusIndex();

            if (focusIndex == -1) focusIndex = 0;
            if (SelectionManager.instance.selectedObjects.Count < focusIndex + 1) return;

            // Populate command menu using the first object in list
            GameObject selectedObject = SelectionManager.instance.selectedObjects[focusIndex];
            if (selectedObject == null) return;

            if (selectedObject.layer == LayerMask.NameToLayer("Unit"))
            {
                UnitBase newUnit = selectedObject.GetComponent<UnitBase>();
                newUnit.populateCommands();
            }
            else if (selectedObject.layer == LayerMask.NameToLayer("Structure"))
            {
                Structure newStructure = selectedObject.GetComponent<Structure>();
                newStructure.populateCommands();
            }
        }

        protected void OnDrawGizmos()
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
