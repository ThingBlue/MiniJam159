using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;

namespace MiniJam159.CommandCore
{
    public class ActionIndicatorInfo
    {
        public GameObject actionIndicatorObject;
        public List<Entity> actionEntities;
        public ActionType actionType;
        public Vector3 targetPosition;

        public ActionIndicatorInfo()
        {
            actionEntities = new List<Entity>();
        }
        public ActionIndicatorInfo(GameObject actionIndicatorObject, ActionType actionType, Vector3 targetPosition)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            actionEntities = new List<Entity>();
            this.actionType = actionType;
            this.targetPosition = targetPosition;
        }
        public ActionIndicatorInfo(GameObject actionIndicatorObject, Entity initialEntity, ActionType actionType, Vector3 targetPosition)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            actionEntities = new List<Entity> { initialEntity };
            this.actionType = actionType;
            this.targetPosition = targetPosition;
        }
    }

    public class ActionIndicatorManagerBase : MonoBehaviour
    {
        #region Inspector members

        public GameObject moveActionIndicatorPrefab;
        public GameObject attackActionIndicatorPrefab;
        public GameObject interactActionIndicatorPrefab;

        public GameObject rallyPointIndicatorPrefab;

        public Transform actionIndicatorParentTransform;

        #endregion

        public List<ActionIndicatorInfo> actionIndicators = new List<ActionIndicatorInfo>();

        // Singleton
        public static ActionIndicatorManagerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // Called upon new selection
        public virtual void refreshActionIndicators()
        {
            // See ActionIndicatorManager.refreshActionIndicators()
        }

        // Called upon new command received by unit
        public virtual void addAction(Entity entity, Action action)
        {
            // See ActionIndicatorManager.addAction(Entity entity, Action action)
        }

        // Called upon action completed by unit
        public virtual void completeAction(Entity entity, Action action)
        {
            // See ActionIndicatorManager.completeAction(Entity entity, Action action)
        }

        protected virtual ActionIndicatorInfo createActionIndicator(ActionType actionType, Vector3 targetPosition)
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
                case ActionType.IDLE:
                default:
                    break;
            }
            if (newActionIndicator == null) return null;
            newActionIndicator.transform.position = targetPosition;

            // Add new info to list
            ActionIndicatorInfo newActionIndicatorInfo = new ActionIndicatorInfo(newActionIndicator, actionType, targetPosition);
            actionIndicators.Add(newActionIndicatorInfo);

            return newActionIndicatorInfo;
        }
    }
}
