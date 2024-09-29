using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;

namespace MiniJam159.CommandCore
{
    public class ActionIndicatorInfo
    {
        public GameObject actionIndicatorObject;
        public ActionType actionType;
        public Vector3 targetPosition;
        public List<Entity> actionEntities = new List<Entity>();
        public List<GameObject> lineObjects = new List<GameObject>();

        public ActionIndicatorInfo() { }
        public ActionIndicatorInfo(GameObject actionIndicatorObject, ActionType actionType, Vector3 targetPosition)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            this.actionType = actionType;
            this.targetPosition = targetPosition;
        }
    }

    public class ActionIndicatorManagerBase : MonoBehaviour
    {
        #region Inspector members

        public Transform actionIndicatorParentTransform;

        public GameObject moveActionIndicatorPrefab;
        public GameObject attackActionIndicatorPrefab;
        public GameObject interactActionIndicatorPrefab;

        public GameObject rallyPointIndicatorPrefab;

        public GameObject actionIndicatorLinePrefab;

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

        protected virtual void destroyLines(ActionIndicatorInfo actionIndicator)
        {
            foreach (GameObject lineObject in actionIndicator.lineObjects) Destroy(lineObject);
            actionIndicator.lineObjects.Clear();
        }
    }
}
