using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Common
{
    public class MathUtilities
    {
        public static Vector2 toVector2(Vector3 value)
        {
            return new Vector2(value.x, value.z);
        }
        public static Vector3 toVector3(Vector2 value)
        {
            return new Vector3(value.x, 0, value.y);
        }

        public static Vector2 toVector2Floored(Vector3 value)
        {
            return new Vector2(Mathf.Floor(value.x), Mathf.Floor(value.z));
        }
        public static Vector2 floorVector2(Vector2 value)
        {
            return new Vector2(Mathf.Floor(value.x), Mathf.Floor(value.y));
        }

        public static Vector3 toVector3Floored(Vector2 value)
        {
            return new Vector3(Mathf.Floor(value.x), 0, Mathf.Floor(value.y));
        }
        public static Vector3 floorVector3(Vector3 value)
        {
            return new Vector3(Mathf.Floor(value.x), Mathf.Floor(value.y), Mathf.Floor(value.z));
        }
    }
}
