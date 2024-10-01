using System;
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

        public List<List<TileType>> gridMatrix;

        // Singleton
        public static GridManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
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

        public Vector2 getTileFromPosition(Vector2 position)
        {
            return new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.y));
        }

        public Vector2 getTileFromPosition(Vector3 position)
        {
            return new Vector2(Mathf.Floor(position.x), Mathf.Floor(position.z));
        }

        public Vector3 getPositionFromTile(Vector2 tile)
        {
            return new Vector3(
                tile.x + 0.5f,
                0f,
                tile.y + 0.5f
            );
        }

        public bool isTileOccupied(int x, int z)
        {
            if (x < 0 || x >= mapXLength || z < 0 || z >= mapZLength)
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

        // Wrapper function for calculateClosestUnoccupiedTile that takes a Vector3 and returns a Vector3
        public Vector3 getClosestUnoccupiedTilePosition(Vector3 startPosition)
        {
            Vector2 startTile = getTileFromPosition(startPosition);
            Vector2 result = calculateClosestUnoccupiedTile(startTile);

            // Return (-1, -1, -1) if no valid tiles found
            if (result == -Vector2.one) return -Vector3.one;

            return new Vector3(result.x, 0, result.y);
        }

        public Vector2 calculateClosestUnoccupiedTile(Vector2 startTile)
        {
            // Helper function for checking tile validity
            void addTileToQueue(Vector2 tile, Queue<Vector2> queue, List<List<bool>> visitedMatrix)
            {
                // Convert from float to int
                int xPosition = (int)tile.x;
                int zPosition = (int)tile.y;

                // Check if out of bounds
                if (xPosition < 0 || xPosition >= mapXLength) return;
                if (zPosition < 0 || zPosition >= mapZLength) return;

                // Check if tile has already been visited
                if (visitedMatrix[zPosition][xPosition] == true) return;

                // Mark as visited
                visitedMatrix[zPosition][xPosition] = true;

                // Add to queue
                queue.Enqueue(tile);
            }

            // Matrix to keep track of visited tiles
            List<List<bool>> visitedMatrix = new List<List<bool>>();
            for (int y = 0; y < gridMatrix.Count; y++)
            {
                List<bool> visitedRow = new List<bool>();
                for (int x = 0; x < gridMatrix[y].Count; x++) visitedRow.Add(false);
                visitedMatrix.Add(visitedRow);
            }

            // Initialize queue with start tile enqueued
            Queue<Vector2> queue = new Queue<Vector2>();
            queue.Enqueue(startTile);

            // At worst case, loop until all tiles have been checked
            while (queue.Count > 0)
            {
                Vector2 tile = queue.Dequeue();

                // Check if current tile is free
                if (!isTileOccupied((int)tile.x, (int)tile.y)) return tile;

                // Enqueue all unvisited neighbours
                addTileToQueue(new Vector2(tile.x, tile.y + 1), queue, visitedMatrix); // Above
                addTileToQueue(new Vector2(tile.x, tile.y - 1), queue, visitedMatrix); // Below
                addTileToQueue(new Vector2(tile.x - 1, tile.y), queue, visitedMatrix); // Left
                addTileToQueue(new Vector2(tile.x + 1, tile.y), queue, visitedMatrix); // Right
            }

            // If no free tiles found, return (-1, -1)
            return -Vector2.one;
        }

        public Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius)
        {
            Vector2 startTile = getTileFromPosition(startPosition);
            Vector2 targetTile = getTileFromPosition(targetPosition);
            List<Vector2> fullPath = calculatePath(startTile, targetTile);
            List<Vector2> simplifiedPath = simplifyPath(fullPath, radius);
            return pathToQueue(simplifiedPath);
        }

        public List<Vector2> calculatePath(Vector2 startTile, Vector2 targetTile)
        {
            // Helper function for checking tile validity
            void addTileToQueue(Vector2 tile, MinPriorityQueue<Vector2> queue, List<List<float>> costMatrix, List<List<Vector2>> predecessorMatrix, Vector2 predecessorTile, Vector2 targetTile)
            {
                // Convert from float to int
                int xPosition = (int)tile.x;
                int zPosition = (int)tile.y;

                // Check if out of bounds
                if (xPosition < 0 || xPosition >= mapXLength) return;
                if (zPosition < 0 || zPosition >= mapZLength) return;

                // Check if tile is occupied
                if (isTileOccupied(xPosition, zPosition)) return;

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
            MinPriorityQueue<Vector2> queue = new MinPriorityQueue<Vector2>();
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
            path.Reverse();

            return path;
        }

        // To simplify the path, we do linecasts from and earlier point to a later point
        // If the cast hits nothing, we can remove all points in between
        public List<Vector2> simplifyPath(List<Vector2> path, float radius)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                for (int j = path.Count - 1; j > i; j--)
                {
                    /*
                    // Do linecast from i to j and check for occupied tiles
                    List<Vector2> tilesOnLine = getTilesOnLine(path[i], path[j], radius);

                    // At least one occupied tile, we cannot shorten path with current tiles
                    if (isAnyTileOccupied(tilesOnLine)) continue;
                    */
                    if (isLineBlocked(path[i] + new Vector2(0.5f, 0.5f), path[j] + new Vector2(0.5f, 0.5f), radius)) continue;

                    // No occupied tiles, we can shorten path
                    for (int k = i + 1; k < j; k++) path.RemoveAt(i + 1);
                    break;
                }
            }

            // Remove first point from path since it's the starting tile
            if (path.Count > 0) path.RemoveAt(0);

            return path;
        }

        private bool isLineBlocked(Vector2 startPosition, Vector2 endPosition, float radius)
        {
            // Only cast 1 line if radius is 0
            if (radius == 0) return isLineBlocked(startPosition, endPosition);

            // Calculate direction
            Vector2 direction = (endPosition - startPosition).normalized;

            // Create one line on either side
            Vector2 normal = new Vector2(-direction.y, direction.x);
            Vector2 startPosition1 = startPosition + (normal * radius);
            Vector2 startPosition2 = startPosition - (normal * radius);
            Vector2 endPosition1 = endPosition + (normal * radius);
            Vector2 endPosition2 = endPosition - (normal * radius);

            // Check that adding radius doesn't put us outside the map
            if (endPosition1.x < 0 || endPosition1.x >= mapXLength || endPosition1.y < 0 || endPosition1.y >= mapZLength) return true;
            if (endPosition2.x < 0 || endPosition2.x >= mapXLength || endPosition2.y < 0 || endPosition2.y >= mapZLength) return true;

            // Get tiles on line for both lines
            if (isLineBlocked(startPosition1, endPosition1)) return true;
            return isLineBlocked(startPosition2, endPosition2);
        }

        private bool isLineBlocked(Vector2 startPosition, Vector2 endPosition)
        {
            // Calculate distance and direction
            float distance = Vector2.Distance(startPosition, endPosition);
            Vector2 direction = (endPosition - startPosition).normalized;

            // Initialize current tile and line start
            Vector2 linePosition = startPosition;
            Vector2 tile = getTileFromPosition(startPosition);

            List<Vector2> tilesOnLine = new List<Vector2>();

            // Loop until we reach the end tile
            float currentDistance = 0f;
            while (currentDistance < distance)
            {
                if (isTileOccupied((int)tile.x, (int)tile.y)) return true;

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
                    if (currentDistance + timeToNextX > distance) break;

                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    currentDistance += timeToNextX;
                }
                // Move vertically
                else if (timeToNextX > timeToNextY)
                {
                    if (currentDistance + timeToNextY > distance) break;

                    linePosition += direction * timeToNextY;
                    tile.y += Mathf.Sign(direction.y);
                    currentDistance += timeToNextY;
                }
                // Exact diagonal
                else
                {
                    if (currentDistance + timeToNextX > distance) break;

                    // Move along both axes
                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    tile.y += Mathf.Sign(direction.y);
                    currentDistance += timeToNextX;
                }
            }

            // No tiles on line occupied
            return false;
        }

        private List<Vector2> getTilesOnLine(Vector2 startPosition, Vector2 endPosition, float radius)
        {
            // Only cast 1 line if radius is 0
            if (radius == 0) return getTilesOnLine(startPosition, endPosition);

            // Calculate direction
            Vector2 direction = (endPosition - startPosition).normalized;

            // Create one line on either side
            Vector2 normal = new Vector2(-direction.y, direction.x);
            Vector2 startPosition1 = startPosition + normal;
            Vector2 startPosition2 = startPosition - normal;
            Vector2 endPosition1 = endPosition + normal;
            Vector2 endPosition2 = endPosition - normal;

            // Get tiles on line for both lines
            List<Vector2> tilesOnLine1 = getTilesOnLine(startPosition1, endPosition1);
            List<Vector2> tilesOnLine2 = getTilesOnLine(startPosition2, endPosition2);

            // Merge and remove duplicates
            List<Vector2> tilesOnLine = tilesOnLine1.Union(tilesOnLine2).ToList();

            return tilesOnLine;
        }

        private List<Vector2> getTilesOnLine(Vector2 startPosition, Vector2 endPosition)
        {
            // Calculate distance and direction
            float distance = Vector2.Distance(startPosition, endPosition);
            Vector2 direction = (endPosition - startPosition).normalized;

            // Initialize current tile and line start
            Vector2 linePosition = startPosition;
            Vector2 tile = getTileFromPosition(startPosition);

            List<Vector2> tilesOnLine = new List<Vector2>();

            // Loop until we reach the end tile
            float currentDistance = 0f;
            while (currentDistance < distance)
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
                    if (currentDistance + timeToNextX > distance) break;

                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    currentDistance += timeToNextX;
                }
                // Move vertically
                else if (timeToNextX > timeToNextY)
                {
                    if (currentDistance + timeToNextY > distance) break;

                    linePosition += direction * timeToNextY;
                    tile.y += Mathf.Sign(direction.y);
                    currentDistance += timeToNextY;
                }
                // Exact diagonal
                else
                {
                    if (currentDistance + timeToNextX > distance) break;

                    // Move along both axes
                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    tile.y += Mathf.Sign(direction.y);
                    currentDistance += timeToNextX;
                }
            }

            return tilesOnLine;
        }

        public Queue<Vector3> pathToQueue(List<Vector2> path)
        {
            Queue<Vector3> pathQueue = new Queue<Vector3>();
            foreach (Vector2 tile in path)
            {
                pathQueue.Enqueue(getPositionFromTile(tile));
            }
            return pathQueue;
        }

    }
}
