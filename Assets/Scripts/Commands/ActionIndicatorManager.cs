using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using MiniJam159.GameCore;
using MiniJam159.PlayerCore;
using MiniJam159.CommandCore;
using MiniJam159.UnitCore;
using MiniJam159.StructureCore;

namespace MiniJam159.Commands
{
    public class ActionIndicatorManager : ActionIndicatorManagerBase
    {
        #region Inspector members

        public Transform actionIndicatorParentTransform;

        public GameObject moveActionIndicatorPrefab;
        public GameObject attackActionIndicatorPrefab;
        public GameObject interactActionIndicatorPrefab;

        public GameObject rallyPointIndicatorPrefab;

        public GameObject actionIndicatorLinePrefab;

        #endregion

        public List<ActionIndicatorData> actionIndicators = new List<ActionIndicatorData>();

        protected virtual ActionIndicatorData createActionIndicator(ActionType actionType, Vector3 targetPosition)
        {
            GameObject newActionIndicator = null;
            switch (actionType)
            {
                case ActionType.MOVE:
                    newActionIndicator = Instantiate(moveActionIndicatorPrefab, actionIndicatorParentTransform);
                    break;
                case ActionType.ATTACK:
                    newActionIndicator = Instantiate(attackActionIndicatorPrefab, actionIndicatorParentTransform);
                    break;
                case ActionType.ATTACK_MOVE:
                    newActionIndicator = Instantiate(attackActionIndicatorPrefab, actionIndicatorParentTransform);
                    break;
                case ActionType.HARVEST:
                    newActionIndicator = Instantiate(interactActionIndicatorPrefab, actionIndicatorParentTransform);
                    break;
                case ActionType.BUILD:
                    newActionIndicator = Instantiate(interactActionIndicatorPrefab, actionIndicatorParentTransform);
                    break;
                default:
                    break;
            }
            if (newActionIndicator == null) return null;
            newActionIndicator.transform.position = targetPosition;

            // Add new data to list
            ActionIndicatorData newActionIndicatorData = new ActionIndicatorData(newActionIndicator, actionType, targetPosition);
            actionIndicators.Add(newActionIndicatorData);

            return newActionIndicatorData;
        }

        // Called upon new selection
        public override void refreshActionIndicators()
        {
            // Keep a list of existing indicators so that we don't have to instantiate too many new ones
            List<ActionIndicatorData> existingActionIndicators = new List<ActionIndicatorData>(actionIndicators);
            foreach (ActionIndicatorData actionIndicator in existingActionIndicators)
            {
                actionIndicator.actionEntities.Clear();
                destroyLines(actionIndicator);
            }

            actionIndicators.Clear();

            // Loop through new selection
            foreach (GameObject entityObject in SelectionManager.instance.selectedObjects)
            {
                UnitBase unit = entityObject.GetComponent<UnitBase>();
                Structure structure = entityObject.GetComponent<Structure>();

                if (unit)
                {
                    for (int i = 0; i < unit.actionQueue.Count; i++)
                    {
                        // Get indicator properties from action
                        Vector3 targetPosition = unit.actionQueue.ElementAt(i).getTargetPosition();
                        ActionType actionType = unit.actionQueue.ElementAt(i).actionType;

                        // Check to see if an indicator with these parameters already exists
                        bool indicatorFound = false;
                        foreach (ActionIndicatorData actionIndicator in actionIndicators)
                        {
                            if (actionIndicator.targetPosition == targetPosition && actionIndicator.actionType == actionType)
                            {
                                actionIndicator.actionEntities.Add(unit);

                                // Create line for unit
                                actionIndicator.lineObjects.Add(createLine(unit, unit.actionQueue.ElementAt(i), actionIndicator.actionIndicatorObject.transform));

                                indicatorFound = true;
                                break;
                            }
                        }
                        if (indicatorFound) continue;

                        // Check to see if an indicator with these parameters was in the previous selection
                        foreach (ActionIndicatorData actionIndicator in existingActionIndicators)
                        {
                            if (actionIndicator.targetPosition == targetPosition && actionIndicator.actionType == actionType)
                            {
                                actionIndicators.Add(actionIndicator);
                                existingActionIndicators.Remove(actionIndicator);
                                actionIndicator.actionEntities.Add(unit);

                                // Create line for unit
                                actionIndicator.lineObjects.Add(createLine(unit, unit.actionQueue.ElementAt(i), actionIndicator.actionIndicatorObject.transform));

                                indicatorFound = true;
                                break;
                            }
                        }
                        if (indicatorFound) continue;

                        // No indicators with matching parameters exists, create new indicator
                        ActionIndicatorData newActionIndicatorData = createActionIndicator(actionType, targetPosition);
                        newActionIndicatorData.actionEntities.Add(unit);

                        // Create line for unit
                        newActionIndicatorData.lineObjects.Add(createLine(unit, unit.actionQueue.ElementAt(i), newActionIndicatorData.actionIndicatorObject.transform));
                    }
                }
                else if (structure)
                {
                    // Show rally point
                }
            }

            // Destroy leftover existing indicators
            foreach (ActionIndicatorData actionIndicator in existingActionIndicators) Destroy(actionIndicator.actionIndicatorObject);
            existingActionIndicators.Clear();
        }

        // Called upon new command received by unit
        public override void addAction(Entity entity, Action action)
        {
            // Get indicator properties from action
            Vector3 targetPosition = action.getTargetPosition();
            ActionType actionType = action.actionType;

            // Check to see if an indicator with these parameters already exists
            bool indicatorFound = false;
            foreach (ActionIndicatorData actionIndicator in actionIndicators)
            {
                if (actionIndicator.targetPosition == targetPosition && actionIndicator.actionType == actionType)
                {
                    actionIndicator.actionEntities.Add(entity);

                    // Create line for unit
                    UnitBase unit = entity as UnitBase;
                    if (unit != null) actionIndicator.lineObjects.Add(createLine(unit, action, actionIndicator.actionIndicatorObject.transform));

                    indicatorFound = true;
                    break;
                }
            }
            if (indicatorFound) return;

            // No indicators with matching parameters exists, create new indicator
            ActionIndicatorData newActionIndicatorData = createActionIndicator(actionType, targetPosition);
            newActionIndicatorData.actionEntities.Add(entity);

            // Create line for unit
            {
                UnitBase unit = entity as UnitBase;
                if (unit != null) newActionIndicatorData.lineObjects.Add(createLine(unit, action, newActionIndicatorData.actionIndicatorObject.transform));
            }
        }

        // Called upon action completed by unit
        public override void completeAction(Entity entity, Action action)
        {
            // Remove entity from the list of entities on the indicator matching provided action
            for (int i = 0; i < actionIndicators.Count; i++)
            {
                if (actionIndicators[i].targetPosition != action.getTargetPosition() ||
                    actionIndicators[i].actionType != action.actionType ||
                    !actionIndicators[i].actionEntities.Contains(entity))
                {
                    continue;
                }

                // Destroy line and remove entity
                {
                    int entityIndex = actionIndicators[i].actionEntities.IndexOf(entity);
                    Destroy(actionIndicators[i].lineObjects[entityIndex]);
                    actionIndicators[i].lineObjects.RemoveAt(entityIndex);
                    actionIndicators[i].actionEntities.RemoveAt(entityIndex);
                }

                if (actionIndicators[i].actionEntities.Count == 0)
                {
                    // If current entity is the last entity that was executing provided action, destroy the indicator
                    Destroy(actionIndicators[i].actionIndicatorObject);
                    actionIndicators.RemoveAt(i);
                    i--;
                }

                // If entity has more actions queued, find the lines matching those actions and set new start transform to entity's transform
                UnitBase unit = entity as UnitBase;
                if (unit != null && unit.actionQueue.Count > 0)
                {
                    Action nextAction = unit.actionQueue.Peek();
                    foreach (ActionIndicatorData actionIndicator in actionIndicators)
                    {
                        if (actionIndicator.targetPosition == nextAction.getTargetPosition() && actionIndicator.actionType == nextAction.actionType)
                        {
                            // Find line matching entity
                            int entityIndex = actionIndicator.actionEntities.IndexOf(entity);
                            actionIndicator.lineObjects[entityIndex].GetComponent<ActionIndicatorLine>().startTransform = entity.transform;
                        }
                    }
                }
            }
        }

        protected virtual GameObject createLine(UnitBase unit, Action action, Transform actionIndicatorTransform)
        {
            // Get index of action in unit's action queue
            int actionIndex = -1;
            for (int i = 0; i < unit.actionQueue.Count; i++)
            {
                if (unit.actionQueue.ElementAt(i) == action)
                {
                    actionIndex = i;
                    break;
                }
            }
            if (actionIndex == -1) return null;

            // Get start and end transforms
            Transform startTransform = unit.transform; // Default to unit transform
            Transform endTransform = actionIndicatorTransform;
            if (actionIndex != 0)
            {
                // Find transform of previous action indicator if action is queued
                Action previousAction = unit.actionQueue.ElementAt(actionIndex - 1);
                foreach (ActionIndicatorData actionIndicator in actionIndicators)
                {
                    if (actionIndicator.targetPosition == previousAction.getTargetPosition() && actionIndicator.actionType == previousAction.actionType)
                    {
                        startTransform = actionIndicator.actionIndicatorObject.transform;
                        break;
                    }
                }
            }

            GameObject newLineObject = Instantiate(actionIndicatorLinePrefab, actionIndicatorParentTransform);
            newLineObject.GetComponent<ActionIndicatorLine>().startTransform = startTransform;
            newLineObject.GetComponent<ActionIndicatorLine>().endTransform = endTransform;

            return newLineObject;
        }

        protected virtual void destroyLines(ActionIndicatorData actionIndicator)
        {
            foreach (GameObject lineObject in actionIndicator.lineObjects) Destroy(lineObject);
            actionIndicator.lineObjects.Clear();
        }

    }
}
