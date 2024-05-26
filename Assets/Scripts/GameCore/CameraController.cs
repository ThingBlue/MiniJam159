using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector members

        public Vector2 cameraBoundaryStart;
        public Vector2 cameraBoundaryEnd;

        public bool disablePan = false;
        public float panSpeed;

        #endregion

        // Singleton
        public static CameraController instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public void PanCamera(Vector2 direction)
        {
            if (disablePan) return;

            if (direction == Vector2.up) transform.position = new Vector3(transform.position.x - panSpeed, 10, transform.position.z - panSpeed);
            if (direction == Vector2.down) transform.position = new Vector3(transform.position.x + panSpeed, 10, transform.position.z + panSpeed);
            if (direction == Vector2.left) transform.position = new Vector3(transform.position.x + panSpeed, 10, transform.position.z - panSpeed);
            if (direction == Vector2.right) transform.position = new Vector3(transform.position.x - panSpeed, 10, transform.position.z + panSpeed);

            if (transform.position.x < cameraBoundaryStart.x) transform.position = new Vector3(cameraBoundaryStart.x, transform.position.y, transform.position.z);
            if (transform.position.z < cameraBoundaryStart.y) transform.position = new Vector3(transform.position.x, transform.position.y, cameraBoundaryStart.y);
            if (transform.position.x > cameraBoundaryEnd.x) transform.position = new Vector3(cameraBoundaryEnd.x, transform.position.y, transform.position.z);
            if (transform.position.z > cameraBoundaryEnd.y) transform.position = new Vector3(transform.position.x, transform.position.y, cameraBoundaryEnd.y);
        }
    }
}
