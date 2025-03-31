using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.MapCore
{
    public class PathRequest
    {
        public PathRequest(Vector3 startPosition, Vector3 targetPosition, float radius, List<TileIgnoreData> tileIgnoreData, Action<Queue<Vector3>> callback)
        {
            this.startPosition = startPosition;
            this.targetPosition = targetPosition;
            this.radius = radius;
            this.tileIgnoreData = tileIgnoreData;
            this.callback = callback;
        }

        public Vector3 startPosition;
        public Vector3 targetPosition;
        public float radius;
        public List<TileIgnoreData> tileIgnoreData;
        public Action<Queue<Vector3>> callback;
    }

    public class PathfinderBase : MonoBehaviour
    {
        #region Inspector members

        public int maxThreadCount;

        #endregion

        public Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
        public int threadCount = 0;

        // Singleton
        public static PathfinderBase instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        public virtual void addPathRequest(PathRequest pathRequest) { pathRequestQueue.Enqueue(pathRequest); }

        public virtual Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius, List<TileIgnoreData> tileIgnoreData) { return new Queue<Vector3>(); }
        public virtual List<Vector3> calculatePath(Vector3 startPosition, Vector3 targetPosition, List<TileIgnoreData> tileIgnoreData) { return new List<Vector3>(); }
        public virtual List<Vector3> simplifyPath(List<Vector3> path, float radius, List<TileIgnoreData> tileIgnoreData) { return new List<Vector3>(); }

        public virtual Queue<Vector3> pathToQueue(List<Vector3> path) { return new Queue<Vector3>(); }
    }
}
