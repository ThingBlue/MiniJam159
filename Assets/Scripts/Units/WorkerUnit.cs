using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using MiniJam159.Common;
using MiniJam159.UnitCore;
using MiniJam159.CommandCore;
using MiniJam159.GameCore;
using MiniJam159.Resources;
using MiniJam159.Structures;

using TMPro;

namespace MiniJam159.Units
{
    public class WorkerUnit : MeleeUnit
    {
        #region Inspector members

        public float harvestRange = 2f;
        public float depositRange = 2f;
        public float buildRange = 2f;

        public float harvestRate = 20f;
        public float depositRate = 20f;
        public float buildRate = 20f;
        public float harvestInterval = 1f;
        public float depositInterval = 0.5f;
        public float buildInterval = 0.5f;

        public float resourceCarryCapacity = 100f;

        public List<CommandType> buildMenuCommands;

        public TMP_Text debugText;

        #endregion

        public bool lockDepositPoint = false;
        public GameObject depositPointObject; // Deposit location to return resources

        private GameObject targetResourceObject;
        private GameObject targetStructureObject;
        
        private float carriedResources = 0; // Amount of resources currently carried

        private bool depositing = false;

        private float harvestTimer = 0;
        private float depositTimer = 0;
        private float buildTimer = 0;

        protected override void Start()
        {
            base.Start();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            debugText.text = carriedResources.ToString();
        }

        #region Action handlers

        protected override void handleActions()
        {
            if (actionQueue.Count == 0) return;

            // Handle current action
            Action currentAction = actionQueue.Peek();
            switch (currentAction.actionType)
            {
                case ActionType.HARVEST:
                    handleHarvestAction(currentAction as HarvestAction);
                    break;
                case ActionType.BUILD:
                    handleBuildAction(currentAction as BuildAction);
                    break;
                default:
                    base.handleActions();
                    break;
            }

            // We remove actions from the queue after completing them
        }

        protected virtual void handleHarvestAction(HarvestAction action)
        {

        }

        protected virtual void handleBuildAction(BuildAction action)
        {

        }

        #endregion

        #region Command handlers

        public override void harvestCommand(bool addToQueue, GameObject targetObject)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new HarvestAction(targetObject);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(this, newAction);
        }

        public override void buildCommand(bool addToQueue, GameObject targetObject)
        {
            // Clear queue if queue action button not held
            if (!addToQueue) clearActionQueue();

            // Enqueue new action
            Action newAction = new BuildAction(targetObject);
            actionQueue.Enqueue(newAction);

            // Add new action to indicators
            ActionIndicatorManagerBase.instance.addAction(this, newAction);
        }

        public virtual void placeStructureCommand(bool addToQueue, StructurePlacementData placementData)
        {
            
        }

        public override void openBuildMenuCommand()
        {
            CommandManagerBase.instance.populateCommands(buildMenuCommands);
        }

        #endregion

        /*
        private void handleHarvestResourceJob()
        {
            // HELP WHAT DO WE DO HERE 
            // AFK AS TEMP SOLUTION
            if (targetResourceObject == null) currentAIJob = ActionType.IDLE;
            //if (targetResourceObject == null) ReturnToBase(basePosition);

            Resource targetResource = targetResourceObject.GetComponent<Resource>();
            // AFK AS TEMP SOLUTION
            if (targetResource == null || targetResource.resourceAmount <= 0) currentAIJob = ActionType.IDLE;
            //if (targetResource == null || targetResource.resourceAmount <= 0) ReturnToBase(basePosition);

            // Pouch not full, harvest resource
            if (carriedResources < resourceCarryCapacity && !depositing)
            {
                // Reset deposit timer
                depositTimer = 0;

                // Reset deposit point if not locked
                if (!lockDepositPoint) depositPointObject = null;

                // Harvest resource
                if (Vector3.Distance(transform.position, targetResourceObject.transform.position) <= harvestRange)
                {
                    if (harvestTimer > harvestInterval)
                    {
                        // Perform harvest and reset timer
                        carriedResources += targetResource.harvestResource(harvestRate);
                        harvestTimer = 0;
                    }
                    else
                    {
                        // Increment timer
                        harvestTimer += Time.fixedDeltaTime;
                    }
                }
                // Move towards resource
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetResourceObject.transform.position, moveSpeed * Time.deltaTime);
                }
            }
            // Pouch full, return to base
            else
            {
                // Reset harvest timer
                harvestTimer = 0;

                // Find closest deposit point if null
                if (!depositPointObject)
                {
                    GameObject closestPoint = null;
                    float closestDistance = -1f;
                    foreach (var point in StructureManager.instance.depositPointStructures)
                    {
                        float distance = Vector3.Distance(transform.position, point.transform.position);
                        if (distance < closestDistance || closestDistance == -1)
                        {
                            closestPoint = point;
                            closestDistance = distance;
                        }
                    }
                    depositPointObject = closestPoint;
                }

                // No deposit point, all points destroyed
                if (depositPointObject == null)
                {
                    // Return to IDLE
                    currentAIJob = ActionType.IDLE;
                    return;
                }

                Vector3 depositPointPosition = depositPointObject.transform.position;

                // Deposit resources
                if (Vector3.Distance(transform.position, depositPointPosition) < depositRange)
                {
                    if (depositTimer > depositInterval)
                    {
                        // Deposit resources and reset timer
                        carriedResources -= Mathf.Min(depositRate, carriedResources);
                        // ADD RESOURCES TO STORAGE
                        depositTimer = 0;
                    }
                    else
                    {
                        // Increment timer
                        depositTimer += Time.fixedDeltaTime;
                    }

                    // Set depositing status
                    // Prevents early exit from deposit
                    depositing = carriedResources > 0;
                }
                // Move towards base
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, depositPointPosition, moveSpeed * Time.deltaTime);
                }
            }

        }

        private void handleBuildJob()
        {
            // Build structure
            if (Vector3.Distance(transform.position, targetStructureObject.transform.position) < buildRange)
            {
                Structure targetStructure = targetStructureObject.GetComponent<Structure>();

                // Check if build is complete
                if (targetStructure.buildProgress >= targetStructure.maxBuildProgress)
                {
                    // Reset and return to idle
                    currentAIJob = ActionType.IDLE;
                    buildTimer = 0;
                }

                if (buildTimer > buildInterval)
                {
                    // Contribute build progress and reset build timer
                    targetStructure.addBuildProgress(buildRate);
                    buildTimer = 0;
                }
                else
                {
                    // Increment timer
                    buildTimer += Time.fixedDeltaTime;
                }
            }
            // Move towards structure
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetStructureObject.transform.position, moveSpeed * Time.deltaTime);
            }
        }
        */

        public void setDepositPoint(GameObject depositPointObject, bool lockDepositPoint)
        {
            this.depositPointObject = depositPointObject;
            this.lockDepositPoint = lockDepositPoint;
        }

    }
}
