using UnityEngine;
using System.Collections.Generic;
using MiniJam159.Resources;
using MiniJam159.AICore;
using MiniJam159.Commands;
using MiniJam159.Structures;

namespace MiniJam159.AI
{
    public enum WorkerJob
    {
        IDLE = 0,
        MOVE_TO_POSITION,
        RETURN_TO_BASE,
        HARVEST_RESOURCE,
        ATTACK,
        BUILD
    }

    public class WorkerAI : GroundMeleeAI
    {
        #region Inspector members

        public float harvestRange = 2f;
        public float depositRange = 2f;

        public StructureDataList structureDataList;

        #endregion

        public Vector3 basePosition; // Base location to return resources
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

        public void ReturnToBase(Vector3 basePosition)
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
                isReturningToBase = false;
            }
        }

        private void DepositResources()
        {
            // Logic to add resources to the base
            Debug.Log("Deposited " + carriedResources + " resources at the base.");
            carriedResources = 0;
        }

        public override void moveAICommand(Vector3 position)
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

        public void harvestAICommand(IResource newResource)
        {
            isMovingToPosition = false;
            HarvestResource(newResource);
        }

        public void setBasePosition(Vector3 newBasePosition)
        {
            basePosition = newBasePosition;
        }

        public void buildAICommand(Vector3 structure)
        {
            // Convert structure datas into commands
            List<CommandType> structureCommands = new List<CommandType>();
            foreach (StructureData structureData in structureDataList.structureDatas)
            {
                switch (structureData.structureType)
                {
                    case StructureType.NEST:
                        structureCommands.Add(CommandType.BUILD_NEST);
                        break;
                    case StructureType.WOMB:
                        structureCommands.Add(CommandType.BUILD_WOMB);
                        break;
                    case StructureType.NULL:
                        structureCommands.Add(CommandType.NULL);
                        break;
                }
            }

            // Populate command menu with buildable objects
            CommandManager.instance.populateCommands(structureCommands);
        }

        public void cancelBuildCommand(Vector3 structure)
        {
            // Populate command menu with standard commands
            base.populateCommands();
        }
    }
}
