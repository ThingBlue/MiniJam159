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

        public UnityEvent pauseEvent;
        public UnityEvent unpauseEvent;

        public UnityEvent selectionStartEvent;
        public UnityEvent selectionCompleteEvent;
        public UnityEvent populateCommandsStartEvent;
        public UnityEvent populateCommandsCompleteEvent;

        public UnityEvent buildStructureEvent;

        #endregion

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            // Initialize events
            if (pauseEvent == null) pauseEvent = new UnityEvent();
            if (unpauseEvent == null) unpauseEvent = new UnityEvent();

            if (selectionStartEvent == null) selectionStartEvent = new UnityEvent();
            if (selectionCompleteEvent == null) selectionCompleteEvent = new UnityEvent();
            if (populateCommandsStartEvent == null) populateCommandsStartEvent = new UnityEvent();
            if (populateCommandsCompleteEvent == null) populateCommandsCompleteEvent = new UnityEvent();
        }
    }
}
