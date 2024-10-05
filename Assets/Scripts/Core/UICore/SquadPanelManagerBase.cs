using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MiniJam159.GameCore;
using MiniJam159.PlayerCore;

namespace MiniJam159.UICore
{
    public class SquadPanelManagerBase : MonoBehaviour
    {
        #region Inspector members

        public List<GameObject> squadSlotBoxes;

        #endregion

        public List<GameObject> squadDisplayBoxes;

        // Singleton
        public static SquadPanelManagerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See SquadPanelManager for implementations
        public virtual GameObject createSquadDisplayBox(Squad squad) { return null; }
        public virtual GameObject getSquadDisplayBox(Squad squad) { return null; }
        public virtual void updateSquadDisplayBoxes() { }
        public virtual void toggleRaycastBoxes(bool enable) { }
        public virtual int getDropTarget(out GameObject targetObject)
        {
            targetObject = null;
            return -1;
        }
        public virtual void togglePanel(bool show) { }

    }
}
