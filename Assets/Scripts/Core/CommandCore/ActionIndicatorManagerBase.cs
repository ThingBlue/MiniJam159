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

        // Move indicators
        public Vector3 targetPosition;

        // Interact indicators
        public GameObject targetObject;
        public float radius;

        public List<Entity> actionEntities = new List<Entity>();
        public List<GameObject> lineObjects = new List<GameObject>();

        public ActionIndicatorData() { }

        // Move indicators
        public ActionIndicatorData(GameObject actionIndicatorObject, ActionType actionType, Vector3 targetPosition)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            this.actionType = actionType;
            this.targetPosition = targetPosition;
        }

        // Interact indicators
        public ActionIndicatorData(GameObject actionIndicatorObject, ActionType actionType, GameObject targetObject, float radius)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            this.actionType = actionType;
            this.targetObject = targetObject;
            this.radius = radius;
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

        // See ActionIndicatorManager for implementations
        protected virtual ActionIndicatorData createMoveActionIndicator(ActionType actionType, Vector3 targetPosition) { return null; }
        protected virtual ActionIndicatorData createInteractActionIndicator(ActionType actionType, GameObject targetObject, float radius) { return null; }

        // Wrapper for creating all action indicator types
        protected virtual ActionIndicatorData createActionIndicator(Entity entity, Action action) { return null; }

        public virtual void refreshActionIndicators() { }                    // Called upon new selection
        public virtual void addAction(Entity entity, Action action) { }      // Called upon new command received by unit
        public virtual void completeAction(Entity entity, Action action) { } // Called upon action completed by unit

    }
}
