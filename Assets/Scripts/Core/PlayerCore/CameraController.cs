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

        public bool disablePan = false;
        public float panSpeed;
        public float smoothTime;

        #endregion

        public Vector3 targetPosition;
        public Vector3 velocity;

        // Singleton
        public static CameraController instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            targetPosition = transform.position;
        }

        private void LateUpdate()
        {
            // Move towards target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Clamp to boundary
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, cameraBoundaryStart.x, cameraBoundaryEnd.x),
                transform.position.y,
                Mathf.Clamp(transform.position.z, cameraBoundaryStart.z, cameraBoundaryEnd.z)
                );
        }

        public void panCamera(Vector3 direction)
        {
            if (disablePan) return;

            // Pan towards direction
            if (direction == Vector3.forward) targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z + panSpeed);
            if (direction == Vector3.back) targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z - panSpeed);
            if (direction == Vector3.left) targetPosition = new Vector3(targetPosition.x - panSpeed, transform.position.y, targetPosition.z);
            if (direction == Vector3.right) targetPosition = new Vector3(targetPosition.x + panSpeed, transform.position.y, targetPosition.z);

            // Clamp to boundary
            targetPosition = new Vector3(
                Mathf.Clamp(targetPosition.x, cameraBoundaryStart.x, cameraBoundaryEnd.x),
                targetPosition.y,
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
