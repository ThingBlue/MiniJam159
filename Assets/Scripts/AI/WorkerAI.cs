using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using MiniJam159.AICore;
using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using MiniJam159.Resources;
using MiniJam159.Structures;

namespace MiniJam159.AI
{
    public class WorkerAI : GroundMeleeAI
    {
        #region Inspector members

        public float harvestRange = 2f;
        public float depositRange = 2f;
        public float buildRange = 2f;

        public List<CommandType> buildMenuCommands;

        #endregion

        public Vector3 basePosition; // Base location to return resources
        private IResource currentResource; // Current resource being harvested
        private GameObject currentBuildStructureObject;
        
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
            switch (currentAIJob)
            {
                case AIJob.HARVEST_RESOURCE:
                    MoveTowardsResource();
                    break;
                case AIJob.RETURN_TO_BASE:
                    MoveTowardsBase();
                    break;
                case AIJob.BUILD:
                    MoveTowardsStructure();
                    break;
                default:
                    if (currentAIJob == AIJob.IDLE && currentResource != null)
                    {
                        // Resource deposited, resume harvest
                        currentAIJob = AIJob.HARVEST_RESOURCE;
                    }

                    // Current job is a default job, let the base class handle it
                    base.offensiveMovementUpdate();
                    break;
            }
        }

        public void HarvestResource(IResource resource)
        {
            currentAIJob = AIJob.HARVEST_RESOURCE;
            currentResource = resource;
        }

        public void ReturnToBase(Vector3 basePosition)
        {
            currentAIJob = AIJob.RETURN_TO_BASE;
            this.basePosition = basePosition;
        }

        private void MoveTowardsResource()
        {
            if (currentResource == null || currentResource.resourceAmount <= 0)
            {
                ReturnToBase(basePosition);
            }

            transform.position = Vector3.MoveTowards(transform.position, currentResource.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentResource.position) <= harvestRange)
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
            transform.position = Vector3.MoveTowards(transform.position, basePosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, basePosition) < depositRange)
            {
                // Deposit resources
                DepositResources();
                currentAIJob = AIJob.IDLE;
            }
        }

        private void MoveTowardsStructure()
        {
            transform.position = Vector3.MoveTowards(transform.position, currentBuildStructureObject.transform.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentBuildStructureObject.transform.position) < buildRange)
            {

            }
        }

        private void DepositResources()
        {
            // Logic to add resources to the base
            Debug.Log("Deposited " + carriedResources + " resources at the base.");
            carriedResources = 0;
        }

        public void setBasePosition(Vector3 newBasePosition)
        {
            basePosition = newBasePosition;
        }

        public override void moveAICommand(Vector3 position)
        {
            // Reset any harvesting state
            currentResource = null;

            base.moveAICommand(position);
        }

        public override void holdAICommand()
        {
            // Reset any harvesting state
            currentResource = null;

            base.holdAICommand();
        }

        public override void attackAICommand(Transform newTarget)
        {
            // Reset any harvesting state
            currentResource = null;

            base.attackAICommand(newTarget);
        }

        public void harvestAICommand(IResource newResource)
        {
            HarvestResource(newResource);
        }

        public void buildStructureCommand(GameObject structureObject)
        {
            // Move towards structure
            currentAIJob = AIJob.BUILD;
            currentBuildStructureObject = structureObject;

            // Reset harvesting state
            currentResource = null;
        }

        public void openBuildMenuAICommand()
        {
            CommandManagerBase.instance.populateCommands(buildMenuCommands);
        }

    }
}
