using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector members

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
            if (direction == Vector2.up) transform.position = new Vector3(transform.position.x - panSpeed, 10, transform.position.z - panSpeed);
            if (direction == Vector2.down) transform.position = new Vector3(transform.position.x + panSpeed, 10, transform.position.z + panSpeed);
            if (direction == Vector2.left) transform.position = new Vector3(transform.position.x + panSpeed, 10, transform.position.z - panSpeed);
            if (direction == Vector2.right) transform.position = new Vector3(transform.position.x - panSpeed, 10, transform.position.z + panSpeed);
        }
    }
}
