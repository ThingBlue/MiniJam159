using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * STEPS FOR USING UNITY EVENTS:
 *     1. ADD NEW EVENT
 *         public UnityEvent eventName = new UnityEvent();
 *     1.5. INITIALIZE EVENT
 *         if (eventName == null) eventName = new UnityEvent();
 *     2. ADD LISTENER TO EVENT
 *         EventManager.instance.eventName.AddListener(eventCallbackName);
 *     3. INVOKE EVENT CALLBACKS
 *         EventManager.instance.eventName.Invoke();
 */

namespace MiniJam159.GameCore
{
    public class EventManager : MonoBehaviour
    {
        #region Events

        // Game events
        public UnityEvent pauseEvent = new UnityEvent();
        public UnityEvent unpauseEvent = new UnityEvent();

        // Build structure commands
        public UnityEvent buildNestCommandEvent = new UnityEvent();
        public UnityEvent buildWombCommandEvent = new UnityEvent();

        #endregion

        public static EventManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }
    }
}
