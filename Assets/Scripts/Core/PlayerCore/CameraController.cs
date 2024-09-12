using MiniJam159.GameCore;
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

        public Vector3 mapSize;

        #endregion

        private Vector3 targetPosition;
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
            // Find direction of movement baesd on mouse position in world
            Vector3 zoomDirection = (InputManager.instance.getMousePositionInWorld() - transform.position).normalized;

            // Scale y component to -1
            Vector3 scaledZoomDirection = zoomDirection / -zoomDirection.y;

            // Scale movement based off scroll value and zoom speed
            Vector3 zoomMovement = scaledZoomDirection * mouseScroll * zoomSpeed;

            // Don't allow x and z movement if y movement will pass boundary
            float allowedMovement = 1f;
            if (targetPosition.y + zoomMovement.y > maxZoom) allowedMovement = Mathf.Abs(Mathf.Abs(maxZoom - targetPosition.y) / zoomMovement.y);
            else if (targetPosition.y + zoomMovement.y < minZoom) allowedMovement = Mathf.Abs(Mathf.Abs(minZoom - targetPosition.y) / zoomMovement.y);

            // Commit movement to target position
            targetPosition += zoomMovement * allowedMovement;

            // Clamp to boundaries
            targetPosition = new Vector3(
                Mathf.Clamp(targetPosition.x, cameraBoundaryStart.x, cameraBoundaryEnd.x),
                Mathf.Clamp(targetPosition.y, minZoom, maxZoom),
                Mathf.Clamp(targetPosition.z, cameraBoundaryStart.z, cameraBoundaryEnd.z)
            );
        }

        public void setScaledMapPosition(Vector2 scaledPosition)
        {
            // Calculate Z offset due to camera angle
            float zOffset = getCameraCenterPositionInWorld().z - transform.position.z;

            // Set new target position
            targetPosition = new Vector3(
                scaledPosition.x * mapSize.x,
                transform.position.y,
                scaledPosition.y * mapSize.z - zOffset
            );
        }

        public Vector3 getCameraCenterPositionInWorld()
        {
            // Create plane at zero
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            // Cast ray onto plane
            Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

            // Return hit location
            if (plane.Raycast(ray, out float enter)) return ray.GetPoint(enter);
            return Vector3.zero; // Should never execute
        }

        public List<Vector3> getCameraViewCornerPoints()
        {
            // Create plane at zero
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            // Do raycasts at the top left and bottom right corners
            Ray bottomLeftRay = Camera.main.ScreenPointToRay(new Vector2(0, 0));
            Ray topLeftRay = Camera.main.ScreenPointToRay(new Vector2(0, Screen.height));
            Ray topRightRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height));
            Ray bottomRightRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width, 0));

            // Get hit locations
            List<Vector3> cameraViewCornerPoints = new List<Vector3>();
            if (plane.Raycast(bottomLeftRay, out float bottomLeftEnter)) cameraViewCornerPoints.Add(bottomLeftRay.GetPoint(bottomLeftEnter));
            if (plane.Raycast(topLeftRay, out float topLeftEnter)) cameraViewCornerPoints.Add(topLeftRay.GetPoint(topLeftEnter));
            if (plane.Raycast(topRightRay, out float topRightEnter)) cameraViewCornerPoints.Add(topRightRay.GetPoint(topRightEnter));
            if (plane.Raycast(bottomRightRay, out float bottomRightEnter)) cameraViewCornerPoints.Add(bottomRightRay.GetPoint(bottomRightEnter));

            return cameraViewCornerPoints;
        }

        public List<Vector2> getScaledCameraViewCornerPoints()
        {
            List<Vector3> cameraViewCornerPoints = getCameraViewCornerPoints();

            // Scale all points to a value between 0 and 1
            List<Vector2> scaledViewCornerPoints = new List<Vector2>();
            foreach (Vector3 cornerPoint in cameraViewCornerPoints)
            {
                scaledViewCornerPoints.Add(new Vector2(
                    cornerPoint.x / mapSize.x,
                    cornerPoint.z / mapSize.z
                ));
            }
            return scaledViewCornerPoints;
        }
    }
}
