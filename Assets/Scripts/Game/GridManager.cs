using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using MiniJam159.GameCore;
using UnityEditor.Rendering;
using MiniJam159.Common;

namespace MiniJam159.Game
{
    public class GridManager : GridManagerBase
    {
        public List<List<TileType>> gridMatrix;

        protected void Start()
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

        public override bool isTileOccupied(int x, int z)
        {
            if (x < 0 || x >= mapXLength || z < 0 || z >= mapZLength)
            {
                throw new System.Exception("Invalid tile position");
            }

            return gridMatrix[z][x] != TileType.EMPTY;
        }

        public override bool isTileOccupied(Vector2 tile)
        {
            if (tile.x < 0 || tile.x >= mapXLength || tile.y < 0 || tile.y >= mapZLength)
            {
                throw new System.Exception("Invalid tile position");
            }

            return gridMatrix[(int)tile.y][(int)tile.x] != TileType.EMPTY;
        }

        public override bool isAnyTileOccupied(List<Vector2> tiles)
        {
            foreach (Vector2 tile in tiles)
            {
                if (isTileOccupied(tile)) return true;
            }
            return false;
        }

        public override bool isTileWithinStructure(Vector2 tile, Vector2 structureStartTile, Vector2 structureSize)
        {
            if (tile.x >= structureStartTile.x && tile.x < structureStartTile.x + structureSize.x &&
                tile.y >= structureStartTile.y && tile.y < structureStartTile.y + structureSize.y)
            {
                return true;
            }
            return false;
        }

        public override bool isTileIgnored(Vector2 tile, List<TileIgnoreData> tileIgnoreData)
        {
            // Check if tile is within any structures in ignore data
            foreach (TileIgnoreData ignoreData in tileIgnoreData)
            {
                if (isTileWithinStructure(tile, ignoreData.startPosition, ignoreData.size)) return true;
            }
            return false;
        }

        public override void occupyTiles(Vector2 startPosition, Vector2 size, TileType occupationType = TileType.BUILDING)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    gridMatrix[(int)startPosition.y + j][(int)startPosition.x + i] = occupationType;
                }
            }
        }

        public override void occupyTiles(Vector3 startPosition, Vector3 size, TileType occupationType = TileType.BUILDING)
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
        public override Vector3 getClosestFreeTilePosition(Vector3 startPosition, Vector3 targetPosition)
        {
            Vector2 startTile = MathUtilities.toVector2Floored(startPosition);
            Vector2 targetTile = MathUtilities.toVector2Floored(targetPosition);
            Vector2 result = calculateClosestFreeTile(startTile, targetTile);

            // Return (-1, -1, -1) if no valid tiles found
            if (result == -Vector2.one) return -Vector3.one;

            return new Vector3(result.x, 0, result.y);
        }

        // Find closest tile to startTile, prioritizing direction of targetTile
        // startTile = Mouse position, targetTile = Entity position
        public Vector2 calculateClosestFreeTile(Vector2 startTile, Vector2 targetTile)
        {
            // Helper function for checking tile validity
            void addTileToQueue(Vector2 tile, MinPriorityQueue<Vector2> queue, List<List<float>> costMatrix, Vector2 predecessorTile, Vector2 targetTile)
            {
                // Convert from float to int
                int xPosition = (int)tile.x;
                int zPosition = (int)tile.y;

                // Check if out of bounds
                if (xPosition < 0 || xPosition >= mapXLength) return;
                if (zPosition < 0 || zPosition >= mapZLength) return;

                float predecessorCost = costMatrix[(int)predecessorTile.y][(int)predecessorTile.x];

                // Make sure we don't already have a better path to this tile
                if (costMatrix[zPosition][xPosition] == -1 || costMatrix[zPosition][xPosition] > predecessorCost + 1)
                {
                    // Calculate heuristic for this tile
                    float heuristic = Vector2.Distance(tile, targetTile);

                    // Moving through an occupied tile costs twice as much as moving through a regular tile
                    // Makes the search prefer free tiles over occupied ones
                    float cost = 1;
                    if (isTileOccupied(tile)) cost = 2;

                    // Add to queue and cost matrix with new cost + heuristic
                    costMatrix[zPosition][xPosition] = predecessorCost + cost;
                    queue.add(predecessorCost + cost + heuristic, tile);
                }
            }

            // Initialize matrices to hold calculation info
            List<List<float>> costMatrix = new List<List<float>>();
            for (int y = 0; y < gridMatrix.Count; y++)
            {
                List<float> costRow = new List<float>();
                for (int x = 0; x < gridMatrix[y].Count; x++) costRow.Add(-1);
                costMatrix.Add(costRow);
            }

            // Initialize priority queue and matrices with start tile
            MinPriorityQueue<Vector2> queue = new MinPriorityQueue<Vector2>();
            queue.add(0, startTile);
            costMatrix[(int)startTile.y][(int)startTile.x] = 0;

            // Loop until free tile found or all tiles exhausted
            while (queue.count() != 0)
            {
                Vector2 tile = queue.pop();

                // Check if current tile is free
                if (!isTileOccupied(tile)) return tile;

                // Add neighbours to queue (Only add if cost is less)
                addTileToQueue(new Vector2(tile.x, tile.y + 1), queue, costMatrix, tile, targetTile); // Above
                addTileToQueue(new Vector2(tile.x, tile.y - 1), queue, costMatrix, tile, targetTile); // Below
                addTileToQueue(new Vector2(tile.x - 1, tile.y), queue, costMatrix, tile, targetTile); // Left
                addTileToQueue(new Vector2(tile.x + 1, tile.y), queue, costMatrix, tile, targetTile); // Right
            }

            // Return (-1, -1) if no free tiles found
            return -Vector2.one;
        }

        // Wrapper function for calculateClosestUnoccupiedTile that takes a Vector3 and returns a Vector3
        public override Vector3 getClosestFreeTilePosition(Vector3 startPosition)
        {
            Vector2 startTile = MathUtilities.toVector2Floored(startPosition);
            Vector2 result = calculateClosestFreeTile(startTile);

            // Return (-1, -1, -1) if no valid tiles found
            if (result == -Vector2.one) return -Vector3.one;

            return new Vector3(result.x, 0, result.y);
        }

        public Vector2 calculateClosestFreeTile(Vector2 startTile)
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
                if (!isTileOccupied(tile)) return tile;

                // Enqueue all unvisited neighbours
                addTileToQueue(new Vector2(tile.x, tile.y + 1), queue, visitedMatrix); // Above
                addTileToQueue(new Vector2(tile.x, tile.y - 1), queue, visitedMatrix); // Below
                addTileToQueue(new Vector2(tile.x - 1, tile.y), queue, visitedMatrix); // Left
                addTileToQueue(new Vector2(tile.x + 1, tile.y), queue, visitedMatrix); // Right
            }

            // If no free tiles found, return (-1, -1)
            return -Vector2.one;
        }

        public override Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius, List<TileIgnoreData> tileIgnoreData)
        {
            // Find tile positions from given positions
            Vector2 startTile = MathUtilities.toVector2Floored(startPosition);
            Vector2 targetTile = MathUtilities.toVector2Floored(targetPosition);
            //if (isTileOccupied(targetTile)) targetTile = calculateClosestFreeTile(targetTile, startTile);

            // Calculate path
            List<Vector2> fullPath = calculatePath(startTile, targetTile, tileIgnoreData);
            List<Vector2> simplifiedPath = simplifyPath(fullPath, radius, tileIgnoreData);
            return pathToQueue(simplifiedPath);
        }

        public override List<Vector2> calculatePath(Vector2 startTile, Vector2 targetTile, List<TileIgnoreData> tileIgnoreData)
        {
            // Helper function for checking tile validity
            void addTileToQueue(Vector2 tile, MinPriorityQueue<Vector2> queue, List<List<float>> costMatrix, List<List<Vector2>> predecessorMatrix, Vector2 predecessorTile)
            {
                // Convert from float to int
                int xPosition = (int)tile.x;
                int zPosition = (int)tile.y;

                // Check if out of bounds
                if (xPosition < 0 || xPosition >= mapXLength) return;
                if (zPosition < 0 || zPosition >= mapZLength) return;

                // Check if tile is occupied, except tiles in tile ignore data
                if (isTileOccupied(tile) && !isTileIgnored(tile, tileIgnoreData)) return;

                float predecessorCost = costMatrix[(int)predecessorTile.y][(int)predecessorTile.x];

                // Make sure we don't already have a better path to this tile
                if (costMatrix[zPosition][xPosition] == -1 || costMatrix[zPosition][xPosition] > predecessorCost + 1)
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
                    predecessorRow.Add(-Vector2.one);
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

                // Add neighbours to queue (Only add if cost is less)
                addTileToQueue(new Vector2(tile.x, tile.y + 1), queue, costMatrix, predecessorMatrix, tile); // Above
                addTileToQueue(new Vector2(tile.x, tile.y - 1), queue, costMatrix, predecessorMatrix, tile); // Below
                addTileToQueue(new Vector2(tile.x - 1, tile.y), queue, costMatrix, predecessorMatrix, tile); // Left
                addTileToQueue(new Vector2(tile.x + 1, tile.y), queue, costMatrix, predecessorMatrix, tile); // Right
            }

            // Retrace path from target back to start
            List<Vector2> path = new List<Vector2>();
            Vector2 retraceTile = targetTile;
            while (retraceTile != -Vector2.one)
            {
                path.Add(retraceTile);
                retraceTile = predecessorMatrix[(int)retraceTile.y][(int)retraceTile.x];
            }
            path.Reverse();

            return path;
        }

        // To simplify the path, we do linecasts from and earlier point to a later point
        // If the cast hits nothing, we can remove all points in between
        public override List<Vector2> simplifyPath(List<Vector2> path, float radius, List<TileIgnoreData> tileIgnoreData)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                for (int j = path.Count - 1; j > i; j--)
                {
                    // Do linecast from i to j and check for occupied tiles
                    if (isLineBlocked(path[i] + new Vector2(0.5f, 0.5f), path[j] + new Vector2(0.5f, 0.5f), tileIgnoreData, radius)) continue;

                    // No occupied tiles, we can shorten path
                    for (int k = i + 1; k < j; k++) path.RemoveAt(i + 1);
                    break;
                }
            }

            // Remove first point from path since it's the starting tile
            if (path.Count > 0) path.RemoveAt(0);

            return path;
        }

        protected bool isLineBlocked(Vector2 startPosition, Vector2 endPosition, List<TileIgnoreData> tileIgnoreData, float radius)
        {
            // Only cast 1 line if radius is 0
            if (radius == 0) return isLineBlocked(startPosition, endPosition, tileIgnoreData);

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
            if (isLineBlocked(startPosition1, endPosition1, tileIgnoreData)) return true;
            return isLineBlocked(startPosition2, endPosition2, tileIgnoreData);
        }

        protected bool isLineBlocked(Vector2 startPosition, Vector2 endPosition, List<TileIgnoreData> tileIgnoreData)
        {
            // Calculate distance and direction
            float distance = Vector2.Distance(startPosition, endPosition);
            Vector2 direction = (endPosition - startPosition).normalized;

            // Initialize current tile and line start
            Vector2 linePosition = startPosition;
            Vector2 tile = MathUtilities.floorVector2(startPosition);

            // Loop until we reach the end tile
            float currentDistance = 0f;
            while (currentDistance < distance)
            {
                if (isTileOccupied(tile) && !isTileIgnored(tile, tileIgnoreData)) return true;

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

        public override Queue<Vector3> pathToQueue(List<Vector2> path)
        {
            Queue<Vector3> pathQueue = new Queue<Vector3>();
            foreach (Vector2 tile in path)
            {
                pathQueue.Enqueue(MathUtilities.toVector3(tile) + new Vector3(0.5f, 0, 0.5f));
            }
            return pathQueue;
        }

        /*
        protected List<Vector2> getTilesOnLine(Vector2 startPosition, Vector2 endPosition, float radius)
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

        protected List<Vector2> getTilesOnLine(Vector2 startPosition, Vector2 endPosition)
        {
            // Calculate distance and direction
            float distance = Vector2.Distance(startPosition, endPosition);
            Vector2 direction = (endPosition - startPosition).normalized;

            // Initialize current tile and line start
            Vector2 linePosition = startPosition;
            Vector2 tile = MathUtilities.toVector2Floored(startPosition);

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
        */

    }
}