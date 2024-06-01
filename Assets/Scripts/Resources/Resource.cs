using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Resources
{
    public interface IResource
    {
        Vector3 position { get; set; }
        Vector3 size { get; set; }

        float resourceAmount { get; set; }
        float resourceReplenishRate { get; set; }
        float harvestAmount { get; set; }

        float harvestResource(); // Harvests the resource and returns the amount harvested
        void replenishResource(); // Replenishes the resource over time
    }
}
