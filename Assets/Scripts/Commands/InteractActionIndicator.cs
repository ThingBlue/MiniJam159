using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Commands
{
    public class InteractActionIndicator : MonoBehaviour
    {
        public Vector3 targetPosition;
        public float radius;

        private void FixedUpdate()
        {
            int steps = 32 + (int)(radius * 2);

            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = steps + 1;

            for (int i = 0; i <= steps; i++)
            {
                float progress = (float)i / steps;
                float currentRadian = progress * 2f * Mathf.PI;

                Vector3 currentPosition = targetPosition + new Vector3(
                    Mathf.Cos(currentRadian) * radius,
                    0,
                    Mathf.Sin(currentRadian) * radius
                );

                lineRenderer.SetPosition(i, currentPosition);
            }
        }
    }
}
