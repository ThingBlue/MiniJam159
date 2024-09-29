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
        // Called upon new selection
        public override void refreshActionIndicators()
        {
            // Keep a list of existing indicators so that we don't have to instantiate too many new ones
            List<ActionIndicatorInfo> existingActionIndicators = new List<ActionIndicatorInfo>(actionIndicators);
            foreach (ActionIndicatorInfo actionIndicator in existingActionIndicators) actionIndicator.actionEntities.Clear();

            actionIndicators.Clear();

            // Loop through new selection
            foreach (GameObject entityObject in SelectionManager.instance.selectedObjects)
            {
                Unit unit = entityObject.GetComponent<Unit>();
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
                        foreach (ActionIndicatorInfo actionIndicator in actionIndicators)
                        {
                            if (actionIndicator.targetPosition == targetPosition && actionIndicator.actionType == actionType)
                            {
                                actionIndicator.actionEntities.Add(unit);
                                indicatorFound = true;
                                break;
                            }
                        }
                        if (indicatorFound) continue;

                        // Check to see if an indicator with these parameters was in the previous selection
                        foreach (ActionIndicatorInfo actionIndicator in existingActionIndicators)
                        {
                            if (actionIndicator.targetPosition == targetPosition && actionIndicator.actionType == actionType)
                            {
                                actionIndicators.Add(actionIndicator);
                                existingActionIndicators.Remove(actionIndicator);
                                actionIndicator.actionEntities.Add(unit);
                                indicatorFound = true;
                                break;
                            }
                        }
                        if (indicatorFound) continue;

                        // No indicators with matching parameters exists, create new indicator
                        ActionIndicatorInfo newActionIndicatorInfo = createActionIndicator(actionType, targetPosition);
                        newActionIndicatorInfo.actionEntities.Add(unit);
                    }
                }
                else if (structure)
                {
                    // Show rally point
                }
            }

            // Destroy leftover existing indicators
            foreach (ActionIndicatorInfo actionIndicator in existingActionIndicators) Destroy(actionIndicator.actionIndicatorObject);
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
            foreach (ActionIndicatorInfo actionIndicator in actionIndicators)
            {
                if (actionIndicator.targetPosition == targetPosition && actionIndicator.actionType == actionType)
                {
                    actionIndicator.actionEntities.Add(entity);
                    indicatorFound = true;
                    break;
                }
            }
            if (indicatorFound) return;

            // No indicators with matching parameters exists, create new indicator
            ActionIndicatorInfo newActionIndicatorInfo = createActionIndicator(actionType, targetPosition);
            newActionIndicatorInfo.actionEntities.Add(entity);
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

                actionIndicators[i].actionEntities.Remove(entity);
                if (actionIndicators[i].actionEntities.Count == 0)
                {
                    // If current entity is the last entity that was executing provided action, destroy the indicator
                    Destroy(actionIndicators[i].actionIndicatorObject);
                    actionIndicators.RemoveAt(i);
                    i--;
                }
            }
        }

    }
}
