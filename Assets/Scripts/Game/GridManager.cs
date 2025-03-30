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

        public override bool isTileOccupied(Vector3 tile)
        {
            Vector3 flooredTile = MathUtilities.floorVector3(tile);
            if (flooredTile.x < 0 || flooredTile.x >= mapXLength || flooredTile.z < 0 || flooredTile.z >= mapZLength)
            {
                throw new System.Exception("Invalid tile position");
            }

            return gridMatrix[(int)flooredTile.z][(int)flooredTile.x] != TileType.EMPTY;
        }

        public override bool isAnyTileOccupied(List<Vector3> tiles)
        {
            foreach (Vector3 tile in tiles)
            {
                if (isTileOccupied(tile)) return true;
            }
            return false;
        }

        public override bool isTileWithinStructure(Vector3 tile, Vector3 structureStartTile, Vector3 structureSize)
        {
            if (tile.x >= structureStartTile.x && tile.x < structureStartTile.x + structureSize.x &&
                tile.z >= structureStartTile.z && tile.z < structureStartTile.z + structureSize.z)
            {
                return true;
            }
            return false;
        }

        public override bool isTileIgnored(Vector3 tile, List<TileIgnoreData> tileIgnoreData)
        {
            // Check if tile is within any structures in ignore data
            foreach (TileIgnoreData ignoreData in tileIgnoreData)
            {
                if (isTileWithinStructure(tile, ignoreData.startPosition, ignoreData.size)) return true;
            }
            return false;
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

        // Find closest tile to startTile, prioritizing direction of targetTile
        // startTile = Mouse position, targetTile = Entity position
        public override Vector3 calculateClosestFreeTile(Vector3 startPosition, Vector3 targetPosition)
        {
            // Helper function for checking tile validity
            void addTileToQueue(Vector3 tile, MinPriorityQueue<Vector3> queue, List<List<float>> costMatrix, Vector3 predecessorTile, Vector3 targetTile)
            {
                // Convert from float to int
                int xPosition = (int)tile.x;
                int zPosition = (int)tile.z;

                // Check if out of bounds
                if (xPosition < 0 || xPosition >= mapXLength) return;
                if (zPosition < 0 || zPosition >= mapZLength) return;

                float predecessorCost = costMatrix[(int)predecessorTile.z][(int)predecessorTile.x];

                // Make sure we don't already have a better path to this tile
                if (costMatrix[zPosition][xPosition] == -1 || costMatrix[zPosition][xPosition] > predecessorCost + 1)
                {
                    // Calculate heuristic for this tile
                    float heuristic = Vector3.Distance(tile, targetTile);

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
            for (int z = 0; z < gridMatrix.Count; z++)
            {
                List<float> costRow = new List<float>();
                for (int x = 0; x < gridMatrix[z].Count; x++) costRow.Add(-1);
                costMatrix.Add(costRow);
            }

            // Floor start and target positions to get tiles
            Vector3 startTile = MathUtilities.floorVector3(startPosition);
            Vector3 targetTile = MathUtilities.floorVector3(targetPosition);

            // Initialize priority queue and matrices with start tile
            MinPriorityQueue<Vector3> queue = new MinPriorityQueue<Vector3>();
            queue.add(0, startTile);
            costMatrix[(int)startTile.z][(int)startTile.x] = 0;

            // Loop until free tile found or all tiles exhausted
            while (queue.count() != 0)
            {
                Vector3 tile = queue.pop();

                // Check if current tile is free
                if (!isTileOccupied(tile)) return tile;

                // Add neighbours to queue (Only add if cost is less)
                addTileToQueue(new Vector3(tile.x, 0, tile.z + 1), queue, costMatrix, tile, targetTile); // Above
                addTileToQueue(new Vector3(tile.x, 0, tile.z - 1), queue, costMatrix, tile, targetTile); // Below
                addTileToQueue(new Vector3(tile.x - 1, 0, tile.z), queue, costMatrix, tile, targetTile); // Left
                addTileToQueue(new Vector3(tile.x + 1, 0, tile.z), queue, costMatrix, tile, targetTile); // Right
            }

            // Return (-1, -1, -1) if no free tiles found
            return -Vector3.one;
        }

        public override Vector3 calculateClosestFreeTile(Vector3 startPosition)
        {
            // Helper function for checking tile validity
            void addTileToQueue(Vector3 tile, Queue<Vector3> queue, List<List<bool>> visitedMatrix)
            {
                // Convert from float to int
                int xPosition = (int)tile.x;
                int zPosition = (int)tile.z;

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
            for (int z = 0; z < gridMatrix.Count; z++)
            {
                List<bool> visitedRow = new List<bool>();
                for (int x = 0; x < gridMatrix[z].Count; x++) visitedRow.Add(false);
                visitedMatrix.Add(visitedRow);
            }

            // Floor start and target positions to get tiles
            Vector3 startTile = MathUtilities.floorVector3(startPosition);

            // Initialize queue with start tile enqueued
            Queue<Vector3> queue = new Queue<Vector3>();
            queue.Enqueue(startTile);

            // At worst case, loop until all tiles have been checked
            while (queue.Count > 0)
            {
                Vector3 tile = queue.Dequeue();

                // Check if current tile is free
                if (!isTileOccupied(tile)) return tile;

                // Enqueue all unvisited neighbours
                addTileToQueue(new Vector3(tile.x, 0, tile.z + 1), queue, visitedMatrix); // Above
                addTileToQueue(new Vector3(tile.x, 0, tile.z - 1), queue, visitedMatrix); // Below
                addTileToQueue(new Vector3(tile.x - 1, 0, tile.z), queue, visitedMatrix); // Left
                addTileToQueue(new Vector3(tile.x + 1, 0, tile.z), queue, visitedMatrix); // Right
            }

            // If no free tiles found, return (-1, -1, -1)
            return -Vector3.one;
        }

        public override Queue<Vector3> getPathQueue(Vector3 startPosition, Vector3 targetPosition, float radius, List<TileIgnoreData> tileIgnoreData)
        {
            // Calculate path
            List<Vector3> fullPath = calculatePath(startPosition, targetPosition, tileIgnoreData);
            List<Vector3> simplifiedPath = simplifyPath(fullPath, radius, tileIgnoreData);
            return pathToQueue(simplifiedPath);
        }

        public override List<Vector3> calculatePath(Vector3 startPosition, Vector3 targetPosition, List<TileIgnoreData> tileIgnoreData)
        {
            // Standardize start and target positions
            Vector3 modifiedStartPosition = MathUtilities.addHalfToPositionFloored(startPosition);

            // Keep track of lowest heuristic tile
            Vector3 lowestHeuristicTile = modifiedStartPosition;
            float lowestHeuristic = Vector3.Distance(modifiedStartPosition, targetPosition);

            // Helper function for checking tile validity
            void addTileToQueue(Vector3 tile, MinPriorityQueue<Vector3> queue, List<List<float>> costMatrix, List<List<Vector3>> predecessorMatrix, Vector3 predecessorTile)
            {
                // Convert from float to int
                int xPosition = Mathf.FloorToInt(tile.x);
                int zPosition = Mathf.FloorToInt(tile.z);

                // Check if out of bounds
                if (xPosition < 0 || xPosition >= mapXLength) return;
                if (zPosition < 0 || zPosition >= mapZLength) return;

                // Check if tile is occupied, except tiles in tile ignore data
                if (isTileOccupied(tile) && !isTileIgnored(tile, tileIgnoreData)) return;

                float predecessorCost = costMatrix[(int)predecessorTile.z][(int)predecessorTile.x];

                // Make sure we don't already have a better path to this tile
                if (costMatrix[zPosition][xPosition] == -1 || costMatrix[zPosition][xPosition] > predecessorCost + 1)
                {
                    // Calculate heuristic for this tile
                    float heuristic = Vector3.Distance(tile, targetPosition);
                    if (heuristic < lowestHeuristic)
                    {
                        lowestHeuristic = heuristic;
                        lowestHeuristicTile = tile;
                    }

                    // Add to matrices
                    costMatrix[zPosition][xPosition] = predecessorCost + 1;
                    predecessorMatrix[zPosition][xPosition] = predecessorTile;

                    // Add to queue
                    queue.add(predecessorCost + 1 + heuristic, tile);
                }
            }

            // Initialize matrices to hold calculation info
            List<List<Vector3>> predecessorMatrix = new List<List<Vector3>>();
            List<List<float>> costMatrix = new List<List<float>>();
            for (int z = 0; z < gridMatrix.Count; z++)
            {
                List<Vector3> predecessorRow = new List<Vector3>();
                List<float> costRow = new List<float>();
                for (int x = 0; x < gridMatrix[z].Count; x++)
                {
                    predecessorRow.Add(-Vector3.one);
                    costRow.Add(-1);
                }
                predecessorMatrix.Add(predecessorRow);
                costMatrix.Add(costRow);
            }

            // Initialize priority queue and matrices with start tile
            MinPriorityQueue<Vector3> queue = new MinPriorityQueue<Vector3>();
            queue.add(0, modifiedStartPosition);
            costMatrix[(int)modifiedStartPosition.z][(int)modifiedStartPosition.x] = 0;

            // Loop until target found or all tiles exhausted
            Vector3 tile = modifiedStartPosition;
            while (queue.count() != 0)
            {
                tile = queue.pop();

                // Check if target reached
                if (MathUtilities.floorVector3(tile) == MathUtilities.floorVector3(targetPosition)) break;

                // Add neighbours to queue (Only add if cost is less)
                addTileToQueue(MathUtilities.addHalfToPositionFloored(new Vector3(tile.x, 0, tile.z + 1)), queue, costMatrix, predecessorMatrix, tile); // Above
                addTileToQueue(MathUtilities.addHalfToPositionFloored(new Vector3(tile.x, 0, tile.z - 1)), queue, costMatrix, predecessorMatrix, tile); // Below
                addTileToQueue(MathUtilities.addHalfToPositionFloored(new Vector3(tile.x - 1, 0, tile.z)), queue, costMatrix, predecessorMatrix, tile); // Left
                addTileToQueue(MathUtilities.addHalfToPositionFloored(new Vector3(tile.x + 1, 0, tile.z)), queue, costMatrix, predecessorMatrix, tile); // Right
            }

            Vector3 retraceTile = targetPosition;
            // No path to target tile, use tile with lowest heuristic as destination instead
            if (MathUtilities.floorVector3(tile) != MathUtilities.floorVector3(targetPosition)) retraceTile = lowestHeuristicTile;

            // Retrace path from target back to start
            List<Vector3> path = new List<Vector3>();
            while (retraceTile != -Vector3.one)
            {
                path.Add(retraceTile);
                retraceTile = predecessorMatrix[Mathf.FloorToInt(retraceTile.z)][Mathf.FloorToInt(retraceTile.x)];
            }
            path.Reverse();

            return path;
        }

        // To simplify the path, we do linecasts from and earlier point to a later point
        // If the cast hits nothing, we can remove all points in between
        public override List<Vector3> simplifyPath(List<Vector3> path, float radius, List<TileIgnoreData> tileIgnoreData)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                for (int j = path.Count - 1; j > i; j--)
                {
                    // Do linecast from i to j and check for occupied tiles
                    if (isLineBlocked(path[i], path[j], tileIgnoreData, radius)) continue;

                    // No occupied tiles, we can shorten path
                    for (int k = i + 1; k < j; k++) path.RemoveAt(i + 1);
                    break;
                }
            }

            // Remove first point from path since it's the starting tile
            if (path.Count > 0) path.RemoveAt(0);

            return path;
        }

        protected bool isLineBlocked(Vector3 startPosition, Vector3 endPosition, List<TileIgnoreData> tileIgnoreData, float radius)
        {
            // Only cast 1 line if radius is 0
            if (radius == 0) return isLineBlocked(startPosition, endPosition, tileIgnoreData);

            // Calculate direction
            Vector3 direction = (endPosition - startPosition).normalized;

            // Create one line on either side
            Vector3 normal = new Vector3(-direction.z, 0, direction.x);
            Vector3 startPosition1 = startPosition + (normal * radius);
            Vector3 startPosition2 = startPosition - (normal * radius);
            Vector3 endPosition1 = endPosition + (normal * radius);
            Vector3 endPosition2 = endPosition - (normal * radius);

            // Check that adding radius doesn't put us outside the map
            if (endPosition1.x < 0 || endPosition1.x >= mapXLength || endPosition1.z < 0 || endPosition1.z >= mapZLength) return true;
            if (endPosition2.x < 0 || endPosition2.x >= mapXLength || endPosition2.z < 0 || endPosition2.z >= mapZLength) return true;

            // Get tiles on line for both lines
            if (isLineBlocked(startPosition1, endPosition1, tileIgnoreData)) return true;
            return isLineBlocked(startPosition2, endPosition2, tileIgnoreData);
        }

        protected bool isLineBlocked(Vector3 startPosition, Vector3 endPosition, List<TileIgnoreData> tileIgnoreData)
        {
            // Calculate distance and direction
            float distance = Vector3.Distance(startPosition, endPosition);
            Vector3 direction = (endPosition - startPosition).normalized;

            // Initialize current tile and line start
            Vector3 linePosition = startPosition;
            Vector3 tile = MathUtilities.floorVector3(startPosition);

            // Loop until we reach the end tile
            float currentDistance = 0f;
            while (currentDistance < distance)
            {
                if (isTileOccupied(tile) && !isTileIgnored(tile, tileIgnoreData)) return true;

                // Calculate the next axes along the line
                float nextX = tile.x;
                float nextZ = tile.z;
                if (direction.x > 0) nextX = tile.x + 1;
                if (direction.z > 0) nextZ = tile.z + 1;

                // Calculate distance to next axes
                float distanceToNextX = Mathf.Infinity;
                float distanceToNextZ = Mathf.Infinity;

                if (direction.x != 0) distanceToNextX = Mathf.Abs(nextX - linePosition.x);
                if (direction.z != 0) distanceToNextZ = Mathf.Abs(nextZ - linePosition.z);

                // Calculate when the line crosses the next X and Y axes
                float timeToNextX = Mathf.Infinity;
                float timeToNextZ = Mathf.Infinity;

                if (distanceToNextX != Mathf.Infinity) timeToNextX = Mathf.Abs(distanceToNextX / direction.x);
                if (distanceToNextZ != Mathf.Infinity) timeToNextZ = Mathf.Abs(distanceToNextZ / direction.z);

                // Move horizontally
                if (timeToNextX < timeToNextZ)
                {
                    if (currentDistance + timeToNextX > distance) break;

                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    currentDistance += timeToNextX;
                }
                // Move vertically
                else if (timeToNextX > timeToNextZ)
                {
                    if (currentDistance + timeToNextZ > distance) break;

                    linePosition += direction * timeToNextZ;
                    tile.z += Mathf.Sign(direction.z);
                    currentDistance += timeToNextZ;
                }
                // Exact diagonal
                else
                {
                    if (currentDistance + timeToNextX > distance) break;

                    // Move along both axes
                    linePosition += direction * timeToNextX;
                    tile.x += Mathf.Sign(direction.x);
                    tile.z += Mathf.Sign(direction.z);
                    currentDistance += timeToNextX;
                }
            }

            // No tiles on line occupied
            return false;
        }

        public override Queue<Vector3> pathToQueue(List<Vector3> path)
        {
            Queue<Vector3> pathQueue = new Queue<Vector3>();
            foreach (Vector3 tile in path) pathQueue.Enqueue(tile);
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
