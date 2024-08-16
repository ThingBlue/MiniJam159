using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.Resources
{
    public class Resource: MonoBehaviour
    {
        #region Inspector members

        public float maxResourceAmount;
        public float resourceReplenishRate;

        #endregion

        public float resourceAmount;

        protected virtual void Start()
        {
            // Set to max amount at start
            resourceAmount = maxResourceAmount;
        }

        protected virtual void FixedUpdate()
        {
            replenishResource();
        }

        // Harvests the resource and returns the amount harvested
        public virtual float harvestResource(float harvestRate)
        {
            float harvested = Mathf.Min(harvestRate, resourceAmount);
            resourceAmount -= harvested;
            return harvested;
        }

        // Replenishes the resource over time
        public virtual void replenishResource()
        {
            resourceAmount += resourceReplenishRate * Time.fixedDeltaTime;
            resourceAmount = Mathf.Min(resourceAmount, maxResourceAmount);
        }
    }
}
