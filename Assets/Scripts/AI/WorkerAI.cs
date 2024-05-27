using UnityEngine;
using System.Collections.Generic;
using MiniJam159.Resources;

namespace MiniJam159.AI
{
    public class WorkerAI : GroundMeleeAI
    {
        public Vector2 basePosition; // Base location to return resources
        private IResource currentResource; // Current resource being harvested
        private bool isReturningToBase = false; // Flag to check if returning to base
        private float carriedResources = 0; // Amount of resources currently carried

        protected override void Start()
        {
            base.Start();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            offensiveMovementUpdate();
        }

        protected override void offensiveMovementUpdate()
        {
            if (isMovingToPosition)
            {
                MoveTowardsPosition(moveSpeed);
            }
            else if (isReturningToBase)
            {
                MoveTowardsBase();
            }
            else if (currentResource != null)
            {
                MoveTowardsResource();
            }
            else
            {
                base.offensiveMovementUpdate();
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }

            if (moveIgnoreTargetTimer > 0)
            {
                moveIgnoreTargetTimer -= Time.deltaTime;
            }
        }

        public void HarvestResource(IResource resource)
        {
            currentResource = resource;
            isReturningToBase = false;
        }

        public void ReturnToBase(Vector2 basePosition)
        {
            this.basePosition = basePosition;
            isReturningToBase = true;
        }

        private void MoveTowardsResource()
        {
            if (currentResource == null || currentResource.resourceAmount <= 0)
            {
                ReturnToBase(basePosition);
            }

            Vector2 resourcePosition = currentResource.position;
            transform.position = Vector2.MoveTowards(transform.position, resourcePosition, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, resourcePosition) < 0.1f)
            {
                if (currentResource == null || currentResource.resourceAmount <= 0)
                {
                    ReturnToBase(basePosition);
                    return;
                }

                // Harvest the resource
                float harvestedAmount = currentResource.harvestResource();
                carriedResources += harvestedAmount;
                ReturnToBase(basePosition);
            }
        }

        private void MoveTowardsBase()
        {
            transform.position = Vector2.MoveTowards(transform.position, basePosition, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, basePosition) < 0.1f)
            {
                // Deposit resources
                DepositResources();
                isReturningToBase = false;
            }
        }

        private void DepositResources()
        {
            // Logic to add resources to the base
            Debug.Log("Deposited " + carriedResources + " resources at the base.");
            carriedResources = 0;
        }

        public override void moveAICommand(Vector2 position)
        {
            // Reset any harvesting state
            currentResource = null;
            isReturningToBase = false;
            carriedResources = 0;

            base.moveAICommand(position);
        }

        public override void holdAICommand()
        {
            // Reset any harvesting state
            currentResource = null;
            isReturningToBase = false;
            carriedResources = 0;

            base.holdAICommand();
        }

        public override void attackAICommand(Transform newTarget)
        {
            // Reset any harvesting state
            currentResource = null;
            isReturningToBase = false;
            carriedResources = 0;

            base.attackAICommand(newTarget);
        }

        public void harvestAICommand(Vector2 newBasePosition, IResource newResource)
        {
            this.basePosition = newBasePosition;
            HarvestResource(newResource);
        }

        public void buildAICommand(Vector2 structure)
        {

        }
    }
}
