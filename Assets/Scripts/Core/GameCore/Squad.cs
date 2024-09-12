using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class Squad
    {
        public int id;
        public List<GameObject> entities;

        public Squad()
        {
            // Default constructor should only be called for debugging
            this.entities = new List<GameObject>();
        }
        public Squad(int id)
        {
            this.id = id;
            this.entities = new List<GameObject>();
        }
        public Squad(int id, List<GameObject> entities)
        {
            this.id = id;
            this.entities = new List<GameObject>(entities);
        }
    }
}
