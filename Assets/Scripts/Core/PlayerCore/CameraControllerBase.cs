using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.PlayerCore
{
    public class CameraControllerBase : MonoBehaviour
    {
        // Singleton
        public static CameraControllerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See CameraController for implementations
        public virtual void panCamera(Vector3 direction) { }
        public virtual void zoomCamera(float mouseScroll) { }
        public virtual void setScaledMapPosition(Vector2 scaledPosition) { }
        public virtual Vector3 getCameraCenterPositionInWorld() { return Vector3.zero; }
        public virtual List<Vector3> getCameraViewCornerPoints() { return new List<Vector3>(); }
        public virtual List<Vector2> getScaledCameraViewCornerPoints() { return new List<Vector2>(); }
    }
}
