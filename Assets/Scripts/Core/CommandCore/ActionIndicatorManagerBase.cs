using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;

namespace MiniJam159.CommandCore
{
    public class ActionIndicatorData
    {
        public GameObject actionIndicatorObject;
        public ActionType actionType;
        public Vector3 targetPosition;
        public List<Entity> actionEntities = new List<Entity>();
        public List<GameObject> lineObjects = new List<GameObject>();

        public ActionIndicatorData() { }
        public ActionIndicatorData(GameObject actionIndicatorObject, ActionType actionType, Vector3 targetPosition)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            this.actionType = actionType;
            this.targetPosition = targetPosition;
        }
    }

    public class ActionIndicatorManagerBase : MonoBehaviour
    {
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
    }
}
