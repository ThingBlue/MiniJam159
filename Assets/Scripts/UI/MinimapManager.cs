using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using MiniJam159.GameCore;
using MiniJam159.PlayerCore;

namespace MiniJam.UI
{
    public class MinimapManager : MonoBehaviour
    {
        #region Inspector members

        public LayerMask uiLayer;

        public RectTransform minimapRawImageRectTransform;

        public GameObject viewBorderLeft;
        public GameObject viewBorderRight;
        public GameObject viewBorderTop;
        public GameObject viewBorderBottom;

        #endregion

        private bool mouseDownOverMinimap;

        // Singleton
        public static MinimapManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Update()
        {
            if (InputManager.instance.getKeyDown("Mouse0"))
            {
                Vector3 minimapBottomLeft = minimapRawImageRectTransform.position - (Vector3)(minimapRawImageRectTransform.sizeDelta / 2f);
                Vector3 minimapTopRight = minimapRawImageRectTransform.position + (Vector3)(minimapRawImageRectTransform.sizeDelta / 2f);

                if (Input.mousePosition.x >= minimapBottomLeft.x && Input.mousePosition.y >= minimapBottomLeft.y &&
                    Input.mousePosition.x <= minimapTopRight.x && Input.mousePosition.y <= minimapTopRight.y)
                {
                    mouseDownOverMinimap = true;
                }
            }
            // Reset mouse down state on mouse up
            if (InputManager.instance.getKeyUp("Mouse0")) mouseDownOverMinimap = false;
        }

        private void FixedUpdate()
        {
            // Calculate minimap dimensions
            Vector3 minimapBottomLeft = minimapRawImageRectTransform.position - (Vector3)(minimapRawImageRectTransform.sizeDelta / 2f);
            Vector3 minimapTopRight = minimapRawImageRectTransform.position + (Vector3)(minimapRawImageRectTransform.sizeDelta / 2f);

            // Move camera
            if (InputManager.instance.getKey("Mouse0") && mouseDownOverMinimap)
            {
                // Find corresponding position on map
                Vector2 scaledMousePosition = new Vector2(
                    (Input.mousePosition.x - minimapBottomLeft.x) / (minimapTopRight.x - minimapBottomLeft.x),
                    (Input.mousePosition.y - minimapBottomLeft.y) /  (minimapTopRight.y - minimapBottomLeft.y)
                );

                // Clamp result
                scaledMousePosition.x = Mathf.Clamp(scaledMousePosition.x, 0f, 1f);
                scaledMousePosition.y = Mathf.Clamp(scaledMousePosition.y, 0f, 1f);

                // Set camera position
                CameraController.instance.setScaledMapPosition(scaledMousePosition);
            }

            // Handle view rectangle
            // Get scaled camera corner points
            List<Vector2> scaledViewCornerPoints = CameraController.instance.getScaledCameraViewCornerPoints();
            
            // Scale up by minimap size
            for (int i = 0; i < scaledViewCornerPoints.Count; i++)
            {
                scaledViewCornerPoints[i] = new Vector2(
                    (scaledViewCornerPoints[i].x - 0.5f) * minimapRawImageRectTransform.sizeDelta.x,
                    (scaledViewCornerPoints[i].y - 0.5f) * minimapRawImageRectTransform.sizeDelta.y
                );
            }

            // Create border
            setBorderPoints(viewBorderLeft, scaledViewCornerPoints[0], scaledViewCornerPoints[1]);
            setBorderPoints(viewBorderTop, scaledViewCornerPoints[1], scaledViewCornerPoints[2]);
            setBorderPoints(viewBorderRight, scaledViewCornerPoints[2], scaledViewCornerPoints[3]);
            setBorderPoints(viewBorderBottom, scaledViewCornerPoints[3], scaledViewCornerPoints[0]);
        }

        private void setBorderPoints(GameObject border, Vector2 startPoint, Vector2 endPoint)
        {
            RectTransform borderRectTransform = border.GetComponent<RectTransform>();

            // Calculate the difference between start and end points
            Vector2 direction = endPoint - startPoint;
            float distance = direction.magnitude;

            // Set the size of the line
            borderRectTransform.sizeDelta = new Vector2(distance, borderRectTransform.sizeDelta.y);

            // Rotate the line to align with the direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            borderRectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // Set the position of the line
            borderRectTransform.localPosition = startPoint + (direction / 2);
        }

    }
}
