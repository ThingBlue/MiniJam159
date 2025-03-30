using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public enum TileType
    {
        EMPTY = 0,
        BUILDING,
        RESOURCE,
        OBSTRUCTION
    }

    public class TileIgnoreData
    {
        public Vector3 startPosition;
        public Vector3 size;

        public TileIgnoreData() { }
        public TileIgnoreData(Vector3 startPosition, Vector3 size)
        {
            this.startPosition = startPosition;
            this.size = size;
        }
    }

    public class GridManagerBase : MonoBehaviour
    {
        #region Inspector members

        public int mapXLength;
        public int mapZLength;

        #endregion

        // Singleton
        public static GridManagerBase instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See GridManager for implementations
        public virtual bool isTileOccupied(int x, int z) { return false; }
        public virtual bool isTileOccupied(Vector3 tile) { return false; }
        public virtual bool isAnyTileOccupied(List<Vector3> tiles) { return false; }
        public virtual bool isTileWithinStructure(Vector3 tile, Vector3 structureStartTile, Vector3 structureSize) { return false; }
        public virtual bool isTileIgnored(Vector3 tile, List<TileIgnoreData> tileIgnoreData) { return false; }

        public virtual void occupyTiles(Vector3 startPosition, Vector3 size, TileType occupationType = TileType.BUILDING) { }

        public virtual Vector3 getClosestFreeTilePosition(Vector3 startPosition, Vector3 targetPosition) { return Vector3.zero; }
        public virtual Vector3 getClosestFreeTilePosition(Vector3 startPosition) { return Vector3.zero; }

        public virtual Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius, List<TileIgnoreData> tileIgnoreData) { return new Queue<Vector3>(); }
        public virtual List<Vector3> calculatePath(Vector3 startPosition, Vector3 targetPosition, List<TileIgnoreData> tileIgnoreData) { return new List<Vector3>(); }
        public virtual List<Vector3> simplifyPath(List<Vector3> path, float radius, List<TileIgnoreData> tileIgnoreData) { return new List<Vector3>(); }

        public virtual Queue<Vector3> pathToQueue(List<Vector3> path) { return new Queue<Vector3>(); }
    }
}
