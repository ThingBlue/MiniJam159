using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniJam159.GameCore;

namespace MiniJam159.Structures
{
    public class GridProximityRenderer : MonoBehaviour
    {
        public Material material;
        public Vector3 focusPosition;

        public void Update()
        {
            material.SetVector("_FocusPosition", focusPosition);
        }
    }
}
