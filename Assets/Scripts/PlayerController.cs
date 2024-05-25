using MiniJam159.GameCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159
{
    public class PlayerController : MonoBehaviour
    {
        #region Inspector members

        public float massSelectDelay;

        #endregion

        private bool mouse1Down;
        private bool mouse2Down;

        private bool massSelecting;
        private float massSelectStartTimer;

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
            if (InputManager.instance.getKeyDown("Mouse1")) mouse1Down = true;
            if (InputManager.instance.getKeyDown("Mouse2")) mouse2Down = true;
        }

        private void FixedUpdate()
        {
            // Camera panning
            if (Input.mousePosition.x <= 0) CameraController.instance.PanCamera(Vector2.left);
            if (Input.mousePosition.x >= Screen.width) CameraController.instance.PanCamera(Vector2.right);
            if (Input.mousePosition.y >= Screen.height) CameraController.instance.PanCamera(Vector2.up);
            if (Input.mousePosition.y <= 0) CameraController.instance.PanCamera(Vector2.down);

            // Mass select
            if (InputManager.instance.getKey("Mouse1"))
            {
                massSelectStartTimer += Time.deltaTime;
            }
            else
            {
                if (massSelectStartTimer >= massSelectDelay)
                {
                    // Execute mass select
                    massSelecting = true;
                }

                // Reset mass select
                massSelectStartTimer = 0.0f;
                massSelecting = false;
            }

            // Single select
            if (mouse1Down && !massSelecting)
            {

            }

            // Movement commands
            if (mouse2Down)
            {
                // Attack if hovering over enemy
                // Interact if hovering over interactable
                // Move if none of the above
            }

            // Clear key states
            mouse1Down = false;
            mouse2Down = false;
        }
    }
}
