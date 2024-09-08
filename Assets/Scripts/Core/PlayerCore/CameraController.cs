using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MiniJam159.PlayerCore
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector members

        public Vector3 cameraBoundaryStart;
        public Vector3 cameraBoundaryEnd;

        public float minZoom;
        public float maxZoom;

        public bool disablePan = false;
        public float panSpeed;
        public float panSmoothTime;

        public bool disableZoom = false;
        public float zoomSpeed;

        #endregion

        public Vector3 targetPosition;
        private Vector3 panVelocity;

        private float defaultZoom;

        // Singleton
        public static CameraController instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            targetPosition = transform.position;
            defaultZoom = transform.position.y;
        }

        private void LateUpdate()
        {
            // Move towards target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref panVelocity, panSmoothTime);

            // Clamp to boundary
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, cameraBoundaryStart.x, cameraBoundaryEnd.x),
                Mathf.Clamp(transform.position.y, minZoom, maxZoom),
                Mathf.Clamp(transform.position.z, cameraBoundaryStart.z, cameraBoundaryEnd.z)
                );
        }

        public void panCamera(Vector3 direction)
        {
            if (disablePan) return;

            // Scale pan speed with current zoom
            float scaledPanSpeed = panSpeed * (transform.position.y / defaultZoom);

            // Pan towards direction
            if (direction == Vector3.forward) targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z + scaledPanSpeed);
            if (direction == Vector3.back) targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z - scaledPanSpeed);
            if (direction == Vector3.left) targetPosition = new Vector3(targetPosition.x - scaledPanSpeed, transform.position.y, targetPosition.z);
            if (direction == Vector3.right) targetPosition = new Vector3(targetPosition.x + scaledPanSpeed, transform.position.y, targetPosition.z);

            // Clamp to boundaries
            targetPosition = new Vector3(
                Mathf.Clamp(targetPosition.x, cameraBoundaryStart.x, cameraBoundaryEnd.x),
                Mathf.Clamp(targetPosition.y, minZoom, maxZoom),
                Mathf.Clamp(targetPosition.z, cameraBoundaryStart.z, cameraBoundaryEnd.z)
                );

            // Stuff for 45 degree camera
            /*
            if (direction == Vector2.up) transform.position = new Vector3(transform.position.x - panSpeed, transform.position.y, transform.position.z - panSpeed);
            if (direction == Vector2.down) transform.position = new Vector3(transform.position.x + panSpeed, transform.position.y, transform.position.z + panSpeed);
            if (direction == Vector2.left) transform.position = new Vector3(transform.position.x + panSpeed, transform.position.y, transform.position.z - panSpeed);
            if (direction == Vector2.right) transform.position = new Vector3(transform.position.x - panSpeed, transform.position.y, transform.position.z + panSpeed);
            */
        }

        public void zoomCamera(float mouseScroll)
        {
            // Calculate zoom movement direction based on camera angle
            float yMovement = -Mathf.Sin(transform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            float zMovement = Mathf.Cos(transform.rotation.eulerAngles.x * Mathf.Deg2Rad);
            Vector3 zoomMoveDirection = new Vector3(0, yMovement, zMovement).normalized;

            // Do zoom by changing target y and z position
            Vector3 zoomMovement = zoomMoveDirection * mouseScroll * zoomSpeed;

            // Don't allow z movement if y movement will pass boundary
            float allowedMovement = 1f;
            if (targetPosition.y + zoomMovement.y > maxZoom)
            {
                allowedMovement = Mathf.Abs(Mathf.Abs(maxZoom - targetPosition.y) / zoomMovement.y);
            }
            else if (targetPosition.y + zoomMovement.y < minZoom)
            {
                allowedMovement = Mathf.Abs(Mathf.Abs(minZoom - targetPosition.y) / zoomMovement.y);
            }

            // Do movement
            targetPosition += zoomMovement * allowedMovement;

            // Clamp to boundaries
            targetPosition = new Vector3(
                Mathf.Clamp(targetPosition.x, cameraBoundaryStart.x, cameraBoundaryEnd.x),
                Mathf.Clamp(targetPosition.y, minZoom, maxZoom),
                Mathf.Clamp(targetPosition.z, cameraBoundaryStart.z, cameraBoundaryEnd.z)
                );
        }

        public void setMapPositionPercent(Vector2 positionPercent)
        {
            Vector3 newTargetPosition = new Vector3(
                cameraBoundaryStart.x + positionPercent.x * (cameraBoundaryEnd.x - cameraBoundaryStart.x),
                transform.position.y,
                cameraBoundaryStart.z + positionPercent.y * (cameraBoundaryEnd.z - cameraBoundaryStart.z)
                );
            targetPosition = newTargetPosition;
        }
    }
}
