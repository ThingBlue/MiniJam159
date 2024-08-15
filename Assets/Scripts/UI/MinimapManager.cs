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
                RectTransform minimapRectTransform = GetComponent<RectTransform>();
                Vector3 minimapBottomLeft = minimapRectTransform.position - (Vector3)(minimapRectTransform.sizeDelta / 2f);
                Vector3 minimapTopRight = minimapRectTransform.position + (Vector3)(minimapRectTransform.sizeDelta / 2f);

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
            if (InputManager.instance.getKey("Mouse0") && mouseDownOverMinimap)
            {
                // Move camera
                RectTransform minimapRectTransform = GetComponent<RectTransform>();
                Vector3 minimapBottomLeft = minimapRectTransform.position - (Vector3)(minimapRectTransform.sizeDelta / 2f);
                Vector3 minimapTopRight = minimapRectTransform.position + (Vector3)(minimapRectTransform.sizeDelta / 2f);

                // Find corresponding position on map
                Vector2 mousePositionPercent = new Vector2(
                    (Input.mousePosition.x - minimapBottomLeft.x) / (minimapTopRight.x - minimapBottomLeft.x),
                    (Input.mousePosition.y - minimapBottomLeft.y) /  (minimapTopRight.y - minimapBottomLeft.y)
                    );

                // Clamp result
                mousePositionPercent.x = Mathf.Clamp(mousePositionPercent.x, 0f, 1f);
                mousePositionPercent.y = Mathf.Clamp(mousePositionPercent.y, 0f, 1f);

                // Set camera position
                CameraController.instance.setMapPositionPercent(mousePositionPercent);
            }
        }

    }
}
