using MiniJam159.GameCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Debugging
{
    public class PathfindingDebugger : MonoBehaviour
    {
        #region Inspector members

        public Vector2 pathfindingStartPosition = Vector2.zero;
        public Vector2 pathfindingEndPosition = Vector2.zero;

        #endregion

        private List<Vector2> debugPath = new List<Vector2>();

        // Singleton
        public static PathfindingDebugger instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Update()
        {
            if (InputManager.instance.getKeyDown("DebugPathfinding"))
            {
                debugPath = GridManager.instance.findPath(new Vector2(0, 0), new Vector2(10, 10));
                debugPath = GridManager.instance.simplifyPath(debugPath);
                debugPath.Insert(0, pathfindingStartPosition);
                Debug.Log("Path count: " + debugPath.Count);
                foreach (Vector2 tile in debugPath) Debug.Log(tile);
            }

        }

        private void OnDrawGizmos()
        {
            if (debugPath.Count > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < debugPath.Count - 1; i++)
                {
                    Vector3 position1 = new Vector3(
                        debugPath[i].x + 0.5f,
                        0.1f,
                        debugPath[i].y + 0.5f
                    );
                    Vector3 position2 = new Vector3(
                        debugPath[i + 1].x + 0.5f,
                        0.1f,
                        debugPath[i + 1].y + 0.5f
                    );
                    Gizmos.DrawLine(position1, position2);
                }
            }
        }

    }
}
