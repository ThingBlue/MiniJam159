using MiniJam159.GameCore;
using MiniJam159.Structures;
using MiniJam159.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniJam159
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector members

        public RectTransform massSelectBoxTransform;
        public float massSelectDelay;

        public LayerMask unitLayer;
        public LayerMask structureLayer;

        public float massSelectionRaycastDistance;

        #endregion

        private bool mouse0Down;
        private bool mouse1Down;
        private bool mouse0Up;
        private bool mouse1Up;

        private bool massSelecting;
        private float massSelectStartTimer;
        private Vector2 massSelectStartPosition;

        private List<GameObject> selectedObjects;


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
            // Check key states
            if (InputManager.instance.getKeyDown("Mouse0")) mouse0Down = true;
            if (InputManager.instance.getKeyDown("Mouse1")) mouse1Down = true;
            if (InputManager.instance.getKeyUp("Mouse0")) mouse0Up = true;
            if (InputManager.instance.getKeyUp("Mouse1")) mouse1Up = true;

            // DEBUG DEBUG DEBUG TEST TEST TEST
            if (InputManager.instance.getKeyDown("PlacementTest"))
            {
                Structure newStructure = new Structure();
                newStructure.size = new Vector2(2, 3);
                StructureManager.instance.beginPlacement(newStructure);
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
                UIManager.instance.updateCommandUI(commands);
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
                if (mouse0Down) StructureManager.instance.finishPlacement();
                if (mouse1Down) StructureManager.instance.cancelPlacement();
            }
            else
            {
                // Mass select
                if (InputManager.instance.getKey("Mouse0"))
                {
                    massSelectStartTimer += Time.deltaTime;
                    if (massSelectStartTimer >= massSelectDelay)
                    {
                        // Start mass select
                        massSelecting = true;
                    }
                }
                else
                {
                    // Reset mass select timer
                    massSelectStartTimer = 0.0f;
                }
                updateMassSelectBox();

                // Execute mass select
                if (massSelecting && mouse0Up)
                {
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
                        Structure structure = structureObject.GetComponent<Structure>();
                        List<Vector2> structurePoints = new List<Vector2>();
                        structurePoints.Add(structure.position + (structure.size / 2f));
                        structurePoints.Add(structure.position + new Vector2(structure.size.x / 2f, 0) - new Vector2(0, structure.size.y / 2f));
                        structurePoints.Add(structure.position - (structure.size / 2f));
                        structurePoints.Add(structure.position - new Vector2(structure.size.x / 2f, 0) + new Vector2(0, structure.size.y / 2f));

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
                        }
                    }

                    // Reset mass select
                    massSelecting = false;
                }

                // Single select
                if (mouse0Down && !massSelecting)
                {


                    // Set start position for mass select
                    massSelectStartPosition = Input.mousePosition;
                }

                // Movement commands
                if (mouse1Down)
                {
                    // Attack if hovering over enemy
                    // Interact if hovering over interactable
                    // Move if none of the above
                }
            }

            // Keyboard
            if (InputManager.instance.getKeyDown("QCommand")) UIManager.instance.executeCommand(0);
            if (InputManager.instance.getKeyDown("WCommand")) UIManager.instance.executeCommand(1);
            if (InputManager.instance.getKeyDown("ECommand")) UIManager.instance.executeCommand(2);
            if (InputManager.instance.getKeyDown("RCommand")) UIManager.instance.executeCommand(3);
            if (InputManager.instance.getKeyDown("ACommand")) UIManager.instance.executeCommand(4);
            if (InputManager.instance.getKeyDown("SCommand")) UIManager.instance.executeCommand(5);
            if (InputManager.instance.getKeyDown("DCommand")) UIManager.instance.executeCommand(6);
            if (InputManager.instance.getKeyDown("FCommand")) UIManager.instance.executeCommand(7);
            if (InputManager.instance.getKeyDown("ZCommand")) UIManager.instance.executeCommand(8);
            if (InputManager.instance.getKeyDown("XCommand")) UIManager.instance.executeCommand(9);
            if (InputManager.instance.getKeyDown("CCommand")) UIManager.instance.executeCommand(10);
            if (InputManager.instance.getKeyDown("VCommand")) UIManager.instance.executeCommand(11);

            // Clear key states
            mouse0Down = false;
            mouse1Down = false;
            mouse0Up = false;
            mouse1Up = false;
        }

        Vector2 closestPointOnNormal(Vector2 normal, Vector2 point)
        {
            Vector2 normalized = normal.normalized;
            float d = Vector2.Dot(point, normalized);
            return normalized * d;
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
