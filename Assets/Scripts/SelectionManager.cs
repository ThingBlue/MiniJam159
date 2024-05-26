using MiniJam159;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159
{
    public class SelectionManager : MonoBehaviour
    {
        #region Inspector members

        public RectTransform massSelectBoxTransform;
        public float massSelectDelay;

        public LayerMask unitLayer;
        public LayerMask structureLayer;

        public float massSelectionRaycastDistance;

        public Material structureOutlineMaterial;

        #endregion

        public bool massSelecting;
        public Vector2 massSelectStartPosition;

        public List<GameObject> selectedObjects;

        // Singleton
        public static SelectionManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            selectedObjects = new List<GameObject>();
        }

        private void FixedUpdate()
        {
            
        }
    }
}
