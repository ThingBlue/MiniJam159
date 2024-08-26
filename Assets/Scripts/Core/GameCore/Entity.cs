using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public class Entity : MonoBehaviour
    {
        #region Inspector members

        public int sortPriority = 0;
        public Sprite displayIcon;
        public float maxHealth;

        #endregion
    }

    // Custom comparer class for entities
    public class EntityComparer : IComparer<Entity>
    {
        public int Compare(Entity entity1, Entity entity2)
        {
            if (entity1 == null || entity2 == null) return 0;

            // Sort in descending order
            return entity2.sortPriority.CompareTo(entity1.sortPriority);
        }
    }

    public class EntityGameObjectComparer : IComparer<GameObject>
    {
        public int Compare(GameObject gameObject1, GameObject gameObject2)
        {
            Entity entity1 = gameObject1.GetComponent<Entity>();
            Entity entity2 = gameObject2.GetComponent<Entity>();

            // Make sure we have GameAIs attached
            if (entity1 == null && entity2 == null) return 0;
            if (entity1 == null) return -1;
            if (entity2 == null) return 1;

            // Sort in descending order
            return entity2.sortPriority.CompareTo(entity1.sortPriority);
        }
    }
}
