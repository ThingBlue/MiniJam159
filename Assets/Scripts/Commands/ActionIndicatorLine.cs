using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Commands
{
    public class ActionIndicatorLine : MonoBehaviour
    {
        public Transform startTransform;
        public Transform endTransform;

        private void Update()
        {
            if (startTransform == null || endTransform == null) return;

            // Update line start and end positions
            GetComponent<LineRenderer>().SetPosition(0, startTransform.position);
            GetComponent<LineRenderer>().SetPosition(1, endTransform.position);
        }
    }
}
