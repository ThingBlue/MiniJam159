using UnityEngine;

public class FoodResource : MonoBehaviour, IResource
{
    public Vector2 position { get; set; }
    public Vector2 size { get; set; }
    public float resourceAmount { get; set; }
    public float resourceReplenishRate { get; set; }
    public float harvestAmount { get; set; }

    void Start()
    {
        // Initialize with some default values or set via Inspector
        position = transform.position;
        size = new Vector2(1, 1);
        resourceAmount = 100f;
        resourceReplenishRate = 5f;
        harvestAmount = 10f;
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
        resourceAmount = Mathf.Min(resourceAmount, 100f); // Assume 100 is the max resource amount
    }
}
