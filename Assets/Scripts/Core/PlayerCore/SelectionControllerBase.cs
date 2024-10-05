using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using MiniJam159.UnitCore;
using MiniJam159.GameCore;
using UnityEngine.UI;

namespace MiniJam159.PlayerCore
{
    public class SelectionControllerBase : MonoBehaviour
    {
        #region Inspector members

        public float massSelectDelay;
        public float massSelectMouseMoveDistance;

        #endregion

        public Vector3 massSelectStartPosition = Vector3.zero;

        // Singleton
        public static SelectionControllerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See SelectionController for implementations

        // Default selections
        public virtual void updateMouseHover() { }
        public virtual void updateMassSelectBox() { }
        public virtual void executeSingleSelect() { }
        public virtual void executeMassSelect() { }

        // Reselects/deselects
        public virtual void reselectSingle(int index) { }
        public virtual void reselectType(int index) { }
        public virtual void deselectSingle(int index) { }
        public virtual void deselectType(int index) { }

        // Squads
        public virtual void createSquadFromCurrentSelection() { }
        public virtual void retrieveSquad(Squad squad) { }

        // Utilities
        public virtual void sortSelection() { }
        public virtual void populateCommands() { }
    }
}
