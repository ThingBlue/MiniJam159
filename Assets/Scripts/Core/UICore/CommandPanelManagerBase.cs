using MiniJam159.CommandCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MiniJam159.UICore
{
    public class CommandPanelManagerBase : MonoBehaviour
    {
        // Singleton
        public static CommandPanelManagerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See CommandPanelManager for implementations
        public virtual void populateCommandButtons() { }
        public virtual void clearCommandButtons() { }
    }
}
