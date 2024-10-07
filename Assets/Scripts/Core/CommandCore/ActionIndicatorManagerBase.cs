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

        public Action action;

        public List<Entity> actionEntities = new List<Entity>();
        public List<GameObject> lineObjects = new List<GameObject>();

        public ActionIndicatorData() { }
        public ActionIndicatorData(GameObject actionIndicatorObject, Action action)
        {
            this.actionIndicatorObject = actionIndicatorObject;
            this.action = action;
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
        protected virtual ActionIndicatorData createActionIndicator(Action action) { return null; }

        public virtual void refreshActionIndicators() { }                    // Called upon new selection
        public virtual void addAction(Action action, Entity entity) { }      // Called upon new command received by unit
        public virtual void completeAction(Action action, Entity entity) { } // Called upon action completed by unit

    }
}
