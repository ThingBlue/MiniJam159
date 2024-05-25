using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Structures
{
    public class PlacementGuideTile : MonoBehaviour
    {
        private void FixedUpdate()
        {
            // Destroy self on update
            Destroy(gameObject);
        }
    }
}
