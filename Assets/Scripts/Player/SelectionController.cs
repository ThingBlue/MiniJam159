using MiniJam159.AICore;
using MiniJam159.GameCore;
using MiniJam159.Selection;
using MiniJam159.Structures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        public void executeSingleSelect()
        {
            // Clear current selection regardless of if we hit amything
            SelectionManager.instance.clearSelectedObjects();

            EventManager.instance.selectionStartEvent.Invoke();

            // Raycast from mouse and grab first hit
            LayerMask raycastMask = unitLayer | structureLayer;
            GameObject hitObject = InputManager.instance.mouseRaycastObject(raycastMask);

            if (hitObject == null) return;

            // We have a hit
            if (hitObject.GetComponent<GameAI>())
            {
                // Hit the paernt ai
                SelectionManager.instance.selectedObjects.Add(hitObject);
            }
            else if (hitObject.GetComponent<Structure>())
            {
                // Hit the parent structure
                SelectionManager.instance.selectedObjects.Add(hitObject);
            }
            else
            {
                // Hit the child
                SelectionManager.instance.selectedObjects.Add(hitObject.transform.parent.gameObject);
            }
            SelectionManager.instance.addOutlinesToSelectedObjects();

            // Populate command menu using the first object in list
            GameObject focusObject = SelectionManager.instance.selectedObjects[0];
            if (focusObject == null) return;

            if (focusObject.layer == LayerMask.NameToLayer("Unit"))
            {
                GameAI newUnit = focusObject.GetComponent<GameAI>();
                newUnit.populateCommands();
            }
            else if (focusObject.layer == LayerMask.NameToLayer("Structure"))
            {
                Structure newStructure = focusObject.GetComponent<Structure>();
                newStructure.populateCommands();
            }

            EventManager.instance.selectionCompleteEvent.Invoke();
        }

        public void executeMassSelect()
        {
            // Clear current selection
            SelectionManager.instance.clearSelectedObjects();

            EventManager.instance.selectionStartEvent.Invoke();

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

            // We now have a polygon on the world plane
            // We need to select every entity inside the polygon

            // Find all normals in casted quadrilateral
            List<Vector2> normals = new List<Vector2>();
            normals.Add(Vector2.Perpendicular(castPoints[0] - castPoints[castPoints.Count - 1]).normalized);
            for (int i = 0; i < castPoints.Count - 1; i++)
            {
                normals.Add(Vector2.Perpendicular(castPoints[i + 1] - castPoints[i]).normalized);
            }

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
                    selectedObjects.Add(structureObject);
                }
            }
            */

            // Loop through all units
            foreach (GameAI unitAI in GameAI.allAIs)
            {
                GameObject unitObject = unitAI.gameObject;
                if (unitObject == null || unitObject.tag != "Unit") continue;

                // Find corners of unit
                List<Vector2> unitPoints = new List<Vector2>();
                Vector2 unitPosition = new Vector2(unitObject.transform.position.x, unitObject.transform.position.z);
                Vector2 unitSize = new Vector2(unitObject.transform.localScale.x, unitObject.transform.localScale.z);
                unitPoints.Add(unitPosition + (unitSize / 2f));
                unitPoints.Add(unitPosition + new Vector2(unitSize.x / 2f, 0) - new Vector2(0, unitSize.y / 2f));
                unitPoints.Add(unitPosition - (unitSize / 2f));
                unitPoints.Add(unitPosition - new Vector2(unitSize.x / 2f, 0) + new Vector2(0, unitSize.y / 2f));

                // Find all normals
                List<Vector2> allNormals = new List<Vector2>(normals);
                allNormals.Add(Vector2.Perpendicular(unitPoints[0] - unitPoints[unitPoints.Count - 1]).normalized);
                for (int i = 0; i < unitPoints.Count - 1; i++)
                {
                    allNormals.Add(Vector2.Perpendicular(unitPoints[i + 1] - unitPoints[i]).normalized);
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
                    float unitMin = float.MaxValue;
                    float unitMax = float.MinValue;
                    foreach (Vector2 point in unitPoints)
                    {
                        Vector2 projectedPoint = closestPointOnNormal(normal, point);
                        float distance = Vector2.Distance(Vector2.zero, projectedPoint);
                        if (Vector2.Dot(projectedPoint, normal) < 0) distance = -distance;
                        if (distance < unitMin) unitMin = distance;
                        if (distance > unitMax) unitMax = distance;
                    }

                    // Check if projected shapes overlap
                    if (castMax < unitMin || unitMax < castMin)
                    {
                        separated = true;
                        break;
                    }
                }

                if (!separated)
                {
                    // Inside selection if not separated
                    SelectionManager.instance.selectedObjects.Add(unitObject);
                }
            }
            SelectionManager.instance.addOutlinesToSelectedObjects();

            // Populate command menu using the first unit in list
            GameObject focusObject = SelectionManager.instance.selectedObjects[0];
            if (focusObject == null) return;

            if (focusObject.layer == LayerMask.NameToLayer("Unit"))
            {
                GameAI newUnit = focusObject.GetComponent<GameAI>();
                newUnit.populateCommands();
            }
            // NOT ALLOWING MASS SELECT ON STRUCTURES FOR NOW
            /*
            else if (focusObject.layer == LayerMask.NameToLayer("Structure"))
            {
                StructureData structureData = focusObject.GetComponent<Structure>().structureData;

                CommandManager.instance.populateCommands(structureData.commands);
                UIManager.instance.populateCommandButtons();
            }
            */

            EventManager.instance.selectionCompleteEvent.Invoke();

            // Reset mass select
            PlayerModeManager.instance.playerMode = PlayerMode.NORMAL;
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
        }

        Vector2 closestPointOnNormal(Vector2 normal, Vector2 point)
        {
            Vector2 normalized = normal.normalized;
            float d = Vector2.Dot(point, normalized);
            return normalized * d;
        }

        public void singleSelectObjectInList(int index)
        {
            // Store the one object we want to keep
            GameObject selectedObject = SelectionManager.instance.selectedObjects[index];

            EventManager.instance.selectionStartEvent.Invoke();

            // Clear list
            SelectionManager.instance.clearSelectedObjects();

            // Add object back in
            SelectionManager.instance.selectedObjects.Add(selectedObject);
            SelectionManager.instance.addOutlinesToSelectedObjects();

            // Populate command menu using the first object in list
            GameObject focusObject = SelectionManager.instance.selectedObjects[0];
            if (focusObject == null) return;

            if (focusObject.layer == LayerMask.NameToLayer("Unit"))
            {
                GameAI newUnit = focusObject.GetComponent<GameAI>();
                newUnit.populateCommands();
            }
            else if (focusObject.layer == LayerMask.NameToLayer("Structure"))
            {
                Structure newStructure = focusObject.GetComponent<Structure>();
                newStructure.populateCommands();
            }

            EventManager.instance.selectionCompleteEvent.Invoke();
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
