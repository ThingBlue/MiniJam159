using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.CommandCore
{
    public class MoveActionIndicator : MonoBehaviour
    {
        #region Inspector members

        public Transform upperTransform;
        public Transform lowerTransform;

        public float upperSpeed;
        public float lowerSpeed;
        public float upperMultiplier;
        public float lowerMultiplier;

        #endregion

        private void FixedUpdate()
        {
            // Default value + amplitude + scaled sin
            upperTransform.localScale = Vector3.one + (Vector3.one * upperMultiplier) + (Vector3.one * Mathf.Sin(upperSpeed * Time.fixedTime) * upperMultiplier);
            lowerTransform.localPosition = new Vector3(0, 0.1f + lowerMultiplier + (Mathf.Sin(lowerSpeed * Time.fixedTime) * lowerMultiplier), 0);
        }
    }
}
