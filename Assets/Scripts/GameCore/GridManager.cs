using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public enum CellType
    {
        Empty = 0,
        Building = 1,
        Resource = 2
    }

    public class GridManager : MonoBehaviour
    {
        // Singleton
        public static GridManager instance;

        public List<List<CellType>> gridMatrix;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            gridMatrix = new List<List<CellType>>();
            for (int i = 0; i < 20; i++)
            {
                List<CellType> newList = new List<CellType>();
                for (int j = 0; j < 20; j++)
                {
                    newList.Add(CellType.Empty);
                }
                gridMatrix.Add(newList);
            }
        }

        public bool isCellOccupied(int x, int y)
        {
            if (x < 0 || x >= gridMatrix[0].Count || y < 0 || y >= gridMatrix.Count)
            {
                throw new System.Exception("Invalid cell position");
            }

            return gridMatrix[y][x] != CellType.Empty;
        }

        public void occupyCells(Vector2 startPosition, Vector2 size, CellType occupationType = CellType.Building)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    gridMatrix[(int)startPosition.y + j][(int)startPosition.x + i] = occupationType;
                }
            }
        }
    }
}
