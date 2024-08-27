using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace MiniJam159.GameCore
{
    public class Entity : MonoBehaviour
    {
        #region Inspector members

        public int sortPriority = 0;
        public Sprite displayIcon;
        public float maxHealth;

        #endregion

        public void setOutline(Material outlineMaterial, Color color)
        {
            MeshRenderer renderer = transform.Find("Mesh").GetComponent<MeshRenderer>();
            Material[] newMaterials = renderer.materials;
            int outlineMaterialIndex = -1;
            for (int i = 0; i < newMaterials.Length; i++)
            {
                if (newMaterials[i].name == outlineMaterial.name + " (Instance)")
                {
                    // Renderer already has outline material
                    outlineMaterialIndex = i;
                    break;
                }
            }

            // Renderer does not have outline material, add it
            if (outlineMaterialIndex == -1)
            {
                newMaterials[0] = new Material(outlineMaterial);
                renderer.materials = newMaterials;
                outlineMaterialIndex = 0;
            }

            // Set colour
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock, outlineMaterialIndex);
            propertyBlock.SetColor("_OutlineColor", color);
            renderer.SetPropertyBlock(propertyBlock, outlineMaterialIndex);
        }

        public void clearOutline(Material outlineMaterial)
        {
            MeshRenderer renderer = transform.Find("Mesh").GetComponent<MeshRenderer>();
            Material[] newMaterials = new Material[2];
            renderer.materials.CopyTo(newMaterials, 0);
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                if (renderer.materials[i].name == outlineMaterial.name + " (Instance)")
                {
                    //renderer.materials[i] = defaultMaterial;
                    newMaterials[i] = null;
                }
            }
            renderer.materials = newMaterials;
        }

        public bool insideCast(List<Vector2> castPoints, List<Vector2> castNormals)
        {
            if (gameObject.tag != "PlayerControlled") return false;

            // Find corners of unit
            List<Vector2> points = new List<Vector2>();
            Vector2 position = new Vector2(transform.position.x, transform.position.z);
            Vector2 size = new Vector2(transform.localScale.x, transform.localScale.z);
            points.Add(position + (size / 2f));
            points.Add(position + new Vector2(size.x / 2f, 0) - new Vector2(0, size.y / 2f));
            points.Add(position - (size / 2f));
            points.Add(position - new Vector2(size.x / 2f, 0) + new Vector2(0, size.y / 2f));

            // Find all normals
            List<Vector2> allNormals = new List<Vector2>(castNormals);
            allNormals.Add(Vector2.Perpendicular(points[0] - points[points.Count - 1]).normalized);
            for (int i = 0; i < points.Count - 1; i++)
            {
                allNormals.Add(Vector2.Perpendicular(points[i + 1] - points[i]).normalized);
            }

            // Loop over all normals
            bool separated = false;
            foreach (Vector2 normal in allNormals)
            {
                // Project shapes onto normal
                float castMin = float.MaxValue;
                float castMax = float.MinValue;
                foreach (Vector2 point in castPoints)
                {
                    Vector2 projectedPoint = closestPointOnNormal(normal, point);
                    float distance = Vector2.Distance(Vector2.zero, projectedPoint);
                    if (Vector2.Dot(projectedPoint, normal) < 0) distance = -distance;
                    if (distance < castMin) castMin = distance;
                    if (distance > castMax) castMax = distance;
                }
                float unitMin = float.MaxValue;
                float unitMax = float.MinValue;
                foreach (Vector2 point in points)
                {
                    Vector2 projectedPoint = closestPointOnNormal(normal, point);
                    float distance = Vector2.Distance(Vector2.zero, projectedPoint);
                    if (Vector2.Dot(projectedPoint, normal) < 0) distance = -distance;
                    if (distance < unitMin) unitMin = distance;
                    if (distance > unitMax) unitMax = distance;
                }

                // Check if projected shapes overlap
                if (castMax < unitMin || unitMax < castMin)
                {
                    separated = true;
                    break;
                }
            }

            // Inside selection if not separated
            return !separated;
        }

        Vector2 closestPointOnNormal(Vector2 normal, Vector2 point)
        {
            Vector2 normalized = normal.normalized;
            float d = Vector2.Dot(point, normalized);
            return normalized * d;
        }

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
