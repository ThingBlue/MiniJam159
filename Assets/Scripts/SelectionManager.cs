using MiniJam159;
using MiniJam159.Commands;
using MiniJam159.Structures;
using MiniJam159.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159
{
    public class SelectionManager : MonoBehaviour
    {
        #region Inspector members

        public RectTransform massSelectBoxTransform;
        public float massSelectDelay;
        public float massSelectMouseMoveDistance;

        public LayerMask unitLayer;
        public LayerMask structureLayer;

        public float selectionRaycastDistance;

        public Material defaultMaterial;
        public Material structureOutlineMaterial;

        #endregion

        public bool massSelecting;
        public Vector2 massSelectStartPosition;

        public List<GameObject> selectedObjects;

        // Singleton
        public static SelectionManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            selectedObjects = new List<GameObject>();
        }

        public void clearSelectedObjects()
        {
            // Clear outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                if (selectedObject.layer == LayerMask.NameToLayer("Structure"))
                {
                    MeshRenderer renderer = selectedObject.transform.Find("Mesh").GetComponent<MeshRenderer>();
                    Material[] newMaterials = new Material[2];
                    renderer.materials.CopyTo(newMaterials, 0);
                    for (int i = 0; i < renderer.materials.Length; i++)
                    {
                        if (renderer.materials[i].name == structureOutlineMaterial.name + " (Instance)")
                        {
                            //renderer.materials[i] = defaultMaterial;
                            newMaterials[i] = null;
                        }
                    }
                    renderer.materials = newMaterials;
                }
            }

            selectedObjects.Clear();

            // Clear commands
            UIManager.instance.clearCommandButtons();
        }

        public void setSingleSelectObject()
        {
            if (selectedObjects.Count == 0 || selectedObjects[0] == null) return;
            GameObject selectedObject = selectedObjects[0];

            // Add outline
            if (selectedObject.layer == LayerMask.NameToLayer("Structure"))
            {
                MeshRenderer renderer = selectedObject.transform.Find("Mesh").GetComponent<MeshRenderer>();
                Material[] newMaterials = renderer.materials;
                bool hasOutlineMaterial = false;
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    if (newMaterials[i].name == structureOutlineMaterial.name + " (Instance)")
                    {
                        hasOutlineMaterial = true;
                        break;
                    }
                }
                if (!hasOutlineMaterial)
                {
                    newMaterials[0] = new Material(structureOutlineMaterial);
                    renderer.materials = newMaterials;
                }
            }

            // Populate command menu using the first unit in list
            GameObject focusObject = selectedObjects[0];
            if (focusObject == null) return;

            if (focusObject.layer == LayerMask.NameToLayer("Structure"))
            {
                StructureData structureData = focusObject.GetComponent<Structure>().structureData;

                CommandManager.instance.populateCommands(structureData.commands);
                UIManager.instance.populateCommandButtons();
            }
            else if (focusObject.layer == LayerMask.NameToLayer("Unit"))
            {

            }
        }

        public void setMassSelectObjects()
        {
            if (selectedObjects.Count == 0) return;

            // Add outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                if (selectedObject.layer == LayerMask.NameToLayer("Structure"))
                {
                    MeshRenderer renderer = selectedObject.transform.Find("Mesh").GetComponent<MeshRenderer>();
                    Material[] newMaterials = renderer.materials;
                    bool hasOutlineMaterial = false;
                    for (int i = 0; i < newMaterials.Length; i++)
                    {
                        if (newMaterials[i].name == structureOutlineMaterial.name + " (Instance)")
                        {
                            hasOutlineMaterial = true;
                            break;
                        }
                    }
                    if (!hasOutlineMaterial)
                    {
                        newMaterials[0] = new Material(structureOutlineMaterial);
                        renderer.materials = newMaterials;
                    }
                }
            }

            // Populate command menu using the first unit in list
            GameObject focusObject = selectedObjects[0];
            if (focusObject == null) return;

            if (focusObject.layer == LayerMask.NameToLayer("Structure"))
            {
                StructureData structureData = focusObject.GetComponent<Structure>().structureData;

                CommandManager.instance.populateCommands(structureData.commands);
                UIManager.instance.populateCommandButtons();
            }
            else if (focusObject.layer == LayerMask.NameToLayer("Unit"))
            {

            }
        }

        public void executeSingleSelect()
        {
            // Clear current selection regardless of if we hit amything
            clearSelectedObjects();

            // Raycast from mouse and grab first hit
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            LayerMask raycastMask = unitLayer | structureLayer;
            if (Physics.Raycast(mouseRay, out hit, selectionRaycastDistance, raycastMask))
            {
                // We have a hit
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Structure"))
                {
                    selectedObjects.Add(hit.collider.transform.parent.gameObject);
                    setSingleSelectObject();
                }
            }
        }

        public void executeMassSelect()
        {
            // Clear current selection
            clearSelectedObjects();

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
            setMassSelectObjects();

            // Reset mass select
            massSelecting = false;
        }

        public void updateMassSelectBox()
        {
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
