using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class GridManager : MonoBehaviour
    {
        // Singleton
        public static GridManager instance;

        public List<List<bool>> gridMatrix;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            gridMatrix = new List<List<bool>>();
            for (int i = 0; i < 20; i++)
            {
                List<bool> newList = new List<bool>();
                for (int j = 0; j < 20; j++)
                {
                    newList.Add(false);
                }
                gridMatrix.Add(newList);
            }
        }

        public bool isCellOccupied(int x, int y)
        {
            return gridMatrix[y][x];
        }

        public void occupyCells(Vector2 startPosition, Vector2 size)
        {
            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    gridMatrix[(int)startPosition.y + j][(int)startPosition.x + i] = true;
                }
            }
        }
    }
}
