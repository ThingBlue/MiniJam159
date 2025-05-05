using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using MiniJam159.Common;

namespace MiniJam159.MapCore
{
    public class Pathfinder : PathfinderBase
    {
        protected void Update()
        {
            // Handle queue in regular update
            if (pathRequestQueue.Count > 0) handlePathRequests();
        }

        protected virtual void handlePathRequests()
        {
            if (pathRequestQueue.Count == 0 || threadCount >= maxThreadCount) return;

            // Handle top request
            PathRequest currentPathRequest = pathRequestQueue.Dequeue();
            Thread thread = new Thread(() => handlePathRequest(currentPathRequest));
            thread.Start();

            Interlocked.Increment(ref threadCount); // Thread safe increment
        }

        protected void handlePathRequest(PathRequest pathRequest)
        {
            Queue<Vector3> result = getPathQueue(pathRequest.startPosition, pathRequest.targetPosition, pathRequest.radius, pathRequest.tileIgnoreData);

            Interlocked.Decrement(ref threadCount); // Thread safe decrement

            // Callback
            pathRequest.callback.Invoke(result);
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
                if (xPosition < 0 || xPosition >= GridManagerBase.instance.mapXLength) return;
                if (zPosition < 0 || zPosition >= GridManagerBase.instance.mapZLength) return;

                // Check if tile is occupied, except tiles in tile ignore data
                if (GridManagerBase.instance.isTileOccupied(tile) && !GridManagerBase.instance.isTileIgnored(tile, tileIgnoreData)) return;

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
            for (int z = 0; z < GridManagerBase.instance.gridMatrix.Count; z++)
            {
                List<Vector3> predecessorRow = new List<Vector3>();
                List<float> costRow = new List<float>();
                for (int x = 0; x < GridManagerBase.instance.gridMatrix[z].Count; x++)
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
            if (endPosition1.x < 0 || endPosition1.x >= GridManagerBase.instance.mapXLength || endPosition1.z < 0 || endPosition1.z >= GridManagerBase.instance.mapZLength) return true;
            if (endPosition2.x < 0 || endPosition2.x >= GridManagerBase.instance.mapXLength || endPosition2.z < 0 || endPosition2.z >= GridManagerBase.instance.mapZLength) return true;

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
                if (GridManagerBase.instance.isTileOccupied(tile) && !GridManagerBase.instance.isTileIgnored(tile, tileIgnoreData)) return true;

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

    }
}
