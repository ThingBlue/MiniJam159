using UnityEngine;

namespace MiniJam159.Resources
{
    public class GoldResource : MonoBehaviour, IResource
    {
        public Vector3 position { get; set; }
        public Vector3 size { get; set; }
        public float resourceAmount { get; set; }
        public float resourceReplenishRate { get; set; }
        public float harvestAmount { get; set; }

        void Start()
        {
            // Initialize with some default values or set via Inspector
            position = transform.position;
            size = new Vector3(1, 1, 1);
            resourceAmount = 300f;
            resourceReplenishRate = 2f;
            harvestAmount = 20f;
        }

        void Update()
        {
            replenishResource();
        }

        public float harvestResource()
        {
            float harvested = Mathf.Min(harvestAmount, resourceAmount);
            resourceAmount -= harvested;
            return harvested;
        }

        public void replenishResource()
        {
            resourceAmount += resourceReplenishRate * Time.deltaTime;
            resourceAmount = Mathf.Min(resourceAmount, 300f); // Assume 300 is the max resource amount
        }
    }
}
