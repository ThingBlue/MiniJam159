using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/*
 * STEPS FOR USING UNITY EVENTS:
 *     1. ADD NEW EVENT
 *         public UnityEvent eventName;
 *     2. INITIALIZE EVENT
 *         if (eventName == null) eventName = new UnityEvent();
 *     3. ADD LISTENER TO EVENT
 *         EventManager.instance.eventName.AddListener(eventCallbackName);
 *     4. INVOKE EVENT CALLBACKS
 *         EventManager.instance.eventName.Invoke();
 */

namespace MiniJam159.GameCore
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager instance;

        #region Events

        // Game events
        public UnityEvent pauseEvent;
        public UnityEvent unpauseEvent;

        // Command events
        public UnityEvent holdCommandEvent;
        public UnityEvent openBuildMenuCommandEvent;
        public UnityEvent cancelBuildMenuCommandEvent;

        // Build structure commands
        public UnityEvent buildNestCommandEvent;
        public UnityEvent buildWombCommandEvent;

        #endregion

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // Initialize events
            // Game events
            if (pauseEvent == null) pauseEvent = new UnityEvent();
            if (unpauseEvent == null) unpauseEvent = new UnityEvent();

            // Command events
            if (holdCommandEvent == null) holdCommandEvent = new UnityEvent();
            if (openBuildMenuCommandEvent == null) openBuildMenuCommandEvent = new UnityEvent();
            if (cancelBuildMenuCommandEvent == null) cancelBuildMenuCommandEvent = new UnityEvent();

            // Build structure commands
            if (buildNestCommandEvent == null) buildNestCommandEvent = new UnityEvent();
            if (buildWombCommandEvent == null) buildWombCommandEvent = new UnityEvent();
        }
    }
}
