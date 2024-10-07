using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public virtual Vector2 getTileFromPosition(Vector2 position) { return Vector2.zero; }
        public virtual Vector2 getTileFromPosition(Vector3 position) { return Vector3.zero; }
        public virtual Vector3 getPositionFromTile(Vector2 tile) { return Vector3.zero; }

        public virtual bool isTileOccupied(int x, int z) { return false; }
        public virtual bool isTileOccupied(Vector2 tile) { return false; }
        public virtual bool isAnyTileOccupied(List<Vector2> tiles) { return false; }

        public virtual void occupyTiles(Vector2 startPosition, Vector2 size, TileType occupationType = TileType.BUILDING) { }
        public virtual void occupyTiles(Vector3 startPosition, Vector3 size, TileType occupationType = TileType.BUILDING) { }

        public virtual Vector3 getClosestFreeTilePosition(Vector3 startPosition, Vector3 targetPosition) { return Vector3.zero; }
        public virtual Vector3 getClosestFreeTilePosition(Vector3 startPosition) { return Vector3.zero; }

        public virtual Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius) { return new Queue<Vector3>(); }
        public virtual Queue<Vector3> getPathQueueToStructure(Vector3 startPosition, Vector3 targetPosition, float radius, Vector3 structureStartPosition, Vector3 structureSize) { return new Queue<Vector3>(); }
        public virtual List<Vector2> calculatePath(Vector2 startTile, Vector2 targetTile) { return new List<Vector2>(); }
        public virtual List<Vector2> calculatePathToStructure(Vector2 startTile, Vector2 targetTile, Vector2 structureStartTile, Vector2 structureSize) { return new List<Vector2>(); }
        public virtual List<Vector2> simplifyPath(List<Vector2> path, float radius) { return new List<Vector2>(); }

        public virtual Queue<Vector3> pathToQueue(List<Vector2> path) { return new Queue<Vector3>(); }
    }
}
