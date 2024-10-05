using MiniJam159.GameCore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MiniJam159.UICore
{
    public class SelectionDisplayManagerBase : MonoBehaviour
    {
        public List<List<GameObject>> selectionDisplayBoxes = new List<List<GameObject>>();

        // Singleton
        public static SelectionDisplayManagerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        // See SelectionDisplayManager for implementations
        public virtual void clearSelectionDisplayBoxes() { }
        public virtual void showSelectionDisplayBoxes() { }
        public virtual void updateSelectionDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false) { }

        public virtual void onSelectionDisplayBoxClicked(int index) { }
    }
}
