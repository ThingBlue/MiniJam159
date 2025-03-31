using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using MiniJam159.GameCore;
using MiniJam159.MapCore;
using MiniJam159.Common;

namespace MiniJam159.Map
{
    public class GridManager : GridManagerBase
    {
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
            if (size.x >= 1 || size.z >= 1) mapChangedEvent.Invoke();
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


    }
}
