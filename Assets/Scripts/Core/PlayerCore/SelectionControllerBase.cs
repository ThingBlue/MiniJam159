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

        public Vector3 massSelectStartPosition;

        // Singleton
        public static SelectionControllerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            massSelectStartPosition = Vector3.zero;
        }

        public virtual void updateMouseHover()
        {
            // See SelectionController::updateMouseHover()
        }

        public virtual void updateMassSelectBox()
        {
            // See SelectionController::updateMassSelectBox()
        }

        public virtual void executeSingleSelect()
        {
            // See SelectionController::executeSingleSelect()
        }

        public virtual void executeMassSelect()
        {
            // See SelectionController::executeMassSelect()
        }

        // Takes a list of new selected objects and adds them to selection manager
        public virtual void executeSelect(List<GameObject> newSelection)
        {
            // See SelectionController::executeSelect(List<GameObject> newSelection)
        }

        public virtual void reselectSingle(int index)
        {
            // See SelectionController::reselectSingle(int index)
        }

        public virtual void reselectType(int index)
        {
            // See SelectionController::reselectType(int index)
        }

        public virtual void deselectSingle(int index)
        {
            // See SelectionController::deselectSingle(int index)
        }

        public virtual void deselectType(int index)
        {
            // See SelectionController::deselectType(int index)
        }

        public virtual void sortSelection()
        {
            // See SelectionController::sortSelection()
        }

        public virtual void populateCommands()
        {
            // See SelectionController::populateCommands()
        }

        public virtual void createSquadFromCurrentSelection()
        {
            // See SelectionController::createSquadFromCurrentSelection()
        }

        public virtual void retrieveSquad(Squad squad)
        {
            // See SelectionController::replaceSelectionWithSquad(Squad squad)
        }

    }
}
