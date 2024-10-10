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
        public Vector2 startPosition;
        public Vector2 size;

        public TileIgnoreData() { }
        public TileIgnoreData(Vector2 startPosition, Vector2 size)
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
        public virtual bool isTileOccupied(Vector2 tile) { return false; }
        public virtual bool isAnyTileOccupied(List<Vector2> tiles) { return false; }
        public virtual bool isTileWithinStructure(Vector2 tile, Vector2 structureStartTile, Vector2 structureSize) { return false; }
        public virtual bool isTileIgnored(Vector2 tile, List<TileIgnoreData> tileIgnoreData) { return false; }

        public virtual void occupyTiles(Vector2 startPosition, Vector2 size, TileType occupationType = TileType.BUILDING) { }
        public virtual void occupyTiles(Vector3 startPosition, Vector3 size, TileType occupationType = TileType.BUILDING) { }

        public virtual Vector3 getClosestFreeTilePosition(Vector3 startPosition, Vector3 targetPosition) { return Vector3.zero; }
        public virtual Vector3 getClosestFreeTilePosition(Vector3 startPosition) { return Vector3.zero; }

        public virtual Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius, List<TileIgnoreData> tileIgnoreData) { return new Queue<Vector3>(); }
        public virtual List<Vector2> calculatePath(Vector2 startTile, Vector2 targetTile, List<TileIgnoreData> tileIgnoreData) { return new List<Vector2>(); }
        public virtual List<Vector2> simplifyPath(List<Vector2> path, float radius, List<TileIgnoreData> tileIgnoreData) { return new List<Vector2>(); }

        public virtual Queue<Vector3> pathToQueue(List<Vector2> path) { return new Queue<Vector3>(); }
    }
}
