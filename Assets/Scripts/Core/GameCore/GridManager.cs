using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MiniJam159.GameCore
{
    public enum TileType
    {
        EMPTY = 0,
        BUILDING,
        RESOURCE,
        OBSTRUCTION
    }

    public class GridManager : MonoBehaviour
    {
        #region Inspector members

        public int mapXLength;
        public int mapZLength;

        #endregion

        // Singleton
        public static GridManager instance;

        public List<List<TileType>> gridMatrix;

        // DEBUG
        public List<Vector2> debugPath;
        public Vector2 debugLineStart = new Vector2(10, 10);
        public Vector2 debugLineEnd = new Vector2(20, 15);
        public List<Vector2> debugTiles;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // DEBUG
            debugPath = new List<Vector2>();
        }

        private void Start()
        {
            gridMatrix = new List<List<TileType>>();
            for (int i = 0; i < mapZLength; i++)
            {
                List<TileType> newList = new List<TileType>();
                for (int j = 0; j < mapXLength; j++)
                {
                    newList.Add(TileType.EMPTY);
                }
                gridMatrix.Add(newList);
            }
        }

        private void Update()
        {
            // DEBUG
            if (InputManager.instance.getKeyDown("Mouse1"))
            {
                debugPath = findPath(new Vector2(0, 0), new Vector2(10, 10));
                debugPath = simplifyPath(debugPath);
                Debug.Log("Path count: " + debugPath.Count);
                //foreach (Vector2 tile in debugPath) Debug.Log(tile);
            }
            if (InputManager.instance.getKeyDown("Deselect"))
            {
                debugTiles = getTilesOnLine(debugLineStart, debugLineEnd);
                // foreach (Vector2 tile in debugTiles) Debug.Log(tile);
            }

        }

        public bool isTileOccupied(int x, int z)
        {
            if (x < 0 || x >= gridMatrix[0].Count || z < 0 || z >= gridMatrix.Count)
            {
                throw new System.Exception("Invalid tile position");
            }

            return gridMatrix[z][x] != TileType.EMPTY;
        }

        public bool isAnyTileOccupied(List<Vector2> tiles)
        {
            foreach (Vector2 tile in tiles)
            {
                if (isTileOccupied((int)tile.x, (int)tile.y)) return true;
            }
            return false;
        }

        public void occupyTiles(Vector2 startPosition, Vector2 size, TileType occupationType = TileType.BUILDING)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    gridMatrix[(int)startPosition.y + j][(int)startPosition.x + i] = occupationType;
                }
            }
        }

        public void occupyTiles(Vector3 startPosition, Vector3 size, TileType occupationType = TileType.BUILDING)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.z; j++)
                {
                    gridMatrix[(int)startPosition.z + j][(int)startPosition.x + i] = occupationType;
                }
            }
        }

        public List<Vector2> findPath(Vector2 startTile, Vector2 targetTile)
        {
            // Initialize matrices to hold calculation info
            List<List<Vector2>> predecessorMatrix = new List<List<Vector2>>();
            List<List<float>> costMatrix = new List<List<float>>();
            for (int y = 0; y < gridMatrix.Count; y++)
            {
                List<Vector2> predecessorRow = new List<Vector2>();
                List<float> costRow = new List<float>();
                for (int x = 0; x < gridMatrix[y].Count; x++)
                {
                    predecessorRow.Add(new Vector2(-1, -1));
                    costRow.Add(-1);
                }
                predecessorMatrix.Add(predecessorRow);
                costMatrix.Add(costRow);
            }

            // Initialize priority queue and matrices with start tile
            PriorityQueue<Vector2> queue = new PriorityQueue<Vector2>();
            queue.add(0, startTile);
            costMatrix[(int)startTile.y][(int)startTile.x] = 0;

            // Loop until target found or all tiles exhausted
            while (queue.count() != 0)
            {
                Vector2 tile = queue.pop();

                // Check if target reached
                if (tile == targetTile) break;

                // Convert from float to int
                float cost = costMatrix[(int)tile.y][(int)tile.x];

                // Add neighbours to queue (Only add if cost is less)
                addTileToQueue(new Vector2(tile.x, tile.y + 1), queue, costMatrix, predecessorMatrix, tile, targetTile); // Above
                addTileToQueue(new Vector2(tile.x, tile.y - 1), queue, costMatrix, predecessorMatrix, tile, targetTile); // Below
                addTileToQueue(new Vector2(tile.x - 1, tile.y), queue, costMatrix, predecessorMatrix, tile, targetTile); // Left
                addTileToQueue(new Vector2(tile.x + 1, tile.y), queue, costMatrix, predecessorMatrix, tile, targetTile); // Right
            }

            // Retrace path from target back to start
            List<Vector2> path = new List<Vector2>();
            Vector2 retraceTile = targetTile;
            while (retraceTile != new Vector2(-1, -1))
            {
                path.Add(retraceTile);
                retraceTile = predecessorMatrix[(int)retraceTile.y][(int)retraceTile.x];
            }

            return path;
        }

        private void addTileToQueue(Vector2 tile, PriorityQueue<Vector2> queue, List<List<float>> costMatrix, List<List<Vector2>> predecessorMatrix, Vector2 predecessorTile, Vector2 targetTile)
        {
            // Convert from float to int
            int xPosition = (int)tile.x;
            int zPosition = (int)tile.y;

            // Check if out of bounds
            if (xPosition < 0 || xPosition > mapXLength) return;
            if (zPosition < 0 || zPosition > mapZLength) return;

            // Check if tile is occupied
            if (isTileOccupied((int)tile.x, (int)tile.y)) return;

            float predecessorCost = costMatrix[(int)predecessorTile.y][(int)predecessorTile.x];

            // Make sure we don't already have a better path to this tile
            if (costMatrix[zPosition][xPosition] == -1 ||
                costMatrix[zPosition][xPosition] > predecessorCost + 1)
            {
                // Calculate heuristic for this tile
                float heuristic = Vector2.Distance(tile, targetTile);

                // Add to matrices
                costMatrix[zPosition][xPosition] = predecessorCost + 1;
                predecessorMatrix[zPosition][xPosition] = predecessorTile;

                // Add to queue
                queue.add(predecessorCost + 1 + heuristic, tile);
            }
        }

        // To simplify the path, we do linecasts from and earlier point to a later point
        // If the cast hits nothing, we can remove all points in between
        public List<Vector2> simplifyPath(List<Vector2> path)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                for (int j = path.Count - 1; j > i; j--)
                {
                    // Do linecast from i to j and check for occupied tiles
                    List<Vector2> tilesOnLine = getTilesOnLine(path[i], path[j]);

                    // At least one occupied tile, we cannot shorten path with current tiles
                    if (isAnyTileOccupied(tilesOnLine)) continue;

                    // No occupied tiles, we can shorten path
                    for (int k = i + 1; k < j; k++) path.RemoveAt(i + 1);
                    break;
                }
            }

            return path;
        }

        private List<Vector2> getTilesOnLine(Vector2 startTile, Vector2 endTile)
        {
            // Calculate normalized direction vector of line
            Vector2 direction = (endTile - startTile).normalized;

            // Initialize current tile and line start
            Vector2 linePosition = startTile + new Vector2(0.5f, 0.5f);
            Vector2 tile = startTile;

            List<Vector2> tilesOnLine = new List<Vector2>();

            // Loop until we reach the end tile
            int i = 0;
            int maxIterations = Mathf.Max(mapXLength, mapZLength) * 2;
            while (tile != endTile && i < maxIterations)
            {
                tilesOnLine.Add(tile);

                // Calculate the next axes along the line
                float nextX = tile.x;
                float nextY = tile.y;
                if (direction.x > 0) nextX = tile.x + 1;
                if (direction.y > 0) nextY = tile.y + 1;

                // Calculate distance to next axes
                float distanceToNextX = Mathf.Infinity;
                float distanceToNextY = Mathf.Infinity;

                if (direction.x != 0) distanceToNextX = Mathf.Abs(nextX - linePosition.x);
                if (direction.y != 0) distanceToNextY = Mathf.Abs(nextY - linePosition.y);

                // Calculate when the line crosses the next X and Y axes
                float timeToNextX = Mathf.Infinity;
                float timeToNextY = Mathf.Infinity;

                if (distanceToNextX != Mathf.Infinity) timeToNextX = Mathf.Abs(distanceToNextX / direction.x);
                if (distanceToNextY != Mathf.Infinity) timeToNextY = Mathf.Abs(distanceToNextY / direction.y);

                // Move horizontally
                if (timeToNextX < timeToNextY)
                {
                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                }
                // Move vertically
                else if (timeToNextX > timeToNextY)
                {
                    linePosition += direction * timeToNextY;
                    tile.y += Mathf.Sign(direction.y);
                }
                // Exact diagonal
                else
                {
                    // Move along both axes
                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    tile.y += Mathf.Sign(direction.y);
                }

                // Failsafe to make sure we don't enter infinite loop on error
                i++;
            }

            if (tile != endTile)
            {
                Debug.LogError("Error: Reached max iterations on linecast without finding end tile");
            }

            return tilesOnLine;
        }

        protected virtual void OnDrawGizmos()
        {
            // DEBUG
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
            if (debugTiles.Count > 0)
            {
                Gizmos.color = Color.blue;
                for (int i = 0; i < debugTiles.Count; i++)
                {
                    Vector3 position = new Vector3(
                        debugTiles[i].x + 0.5f,
                        0.1f,
                        debugTiles[i].y + 0.5f
                    );

                    Gizmos.DrawWireCube(position, new Vector3(1, 1, 1));
                }
                Gizmos.color = Color.yellow;
                Vector3 debugLineStartPosition = new Vector3(
                    debugLineStart.x + 0.5f,
                    0.1f,
                    debugLineStart.y + 0.5f
                );
                Vector3 debugLineEndPosition = new Vector3(
                    debugLineEnd.x + 0.5f,
                    0.1f,
                    debugLineEnd.y + 0.5f
                );
                Gizmos.DrawLine(debugLineStartPosition, debugLineEndPosition);
            }
        }
    }
}
