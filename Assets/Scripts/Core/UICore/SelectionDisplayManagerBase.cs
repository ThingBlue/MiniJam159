using MiniJam159.GameCore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MiniJam159.UICore
{
    public class SelectionDisplayManagerBase : MonoBehaviour
    {
        #region Inspector members

        public GameObject selectionDisplayPanelObject;

        public GameObject displayBoxPrefab;
        public float displayCenterHeight;
        public float displayBoxDefaultSize;
        public float displayBoxHoveredSize;

        #endregion

        public List<List<GameObject>> selectionDisplayBoxes;

        // Singleton
        public static SelectionDisplayManagerBase instance;

        protected virtual void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // Intialize lists
            selectionDisplayBoxes = new List<List<GameObject>>();

            // Hide display panel at the start
            selectionDisplayPanelObject.SetActive(false);
        }

        protected virtual void Update()
        {
            /*
            // Calculate display panel background size and position
            RectTransform displayPanelTransform = selectionDisplayPanel.GetComponent<RectTransform>();

            float minimapPanelWidth = minimapPanel.GetComponent<RectTransform>().sizeDelta.x;
            float commandPanelWidth = commandPanel.GetComponent<RectTransform>().sizeDelta.x;

            float newWidth = Screen.width - minimapPanelWidth - commandPanelWidth;
            float newPosition = minimapPanelWidth + ((Screen.width - commandPanelWidth) - minimapPanelWidth) / 2f - (Screen.width / 2f);
            displayPanelTransform.localPosition = new Vector3(newPosition, displayPanelTransform.localPosition.y, 0f);
            displayPanelTransform.sizeDelta = new Vector2(newWidth, displayPanelTransform.sizeDelta.y);
            */

            // Check if an update is required for display boxes
            if (InputManager.instance.getKeyDown("TypeSelect") || InputManager.instance.getKeyDown("Deselect") ||
                InputManager.instance.getKeyUp("TypeSelect") || InputManager.instance.getKeyUp("Deselect"))
            {
                updateSelectionDisplayBoxes();
            }
        }

        public virtual void clearSelectionDisplayBoxes()
        {
            foreach (List<GameObject> row in selectionDisplayBoxes)
            {
                foreach (GameObject displayBox in row)
                {
                    Destroy(displayBox);
                }
                row.Clear();
            }
            selectionDisplayBoxes.Clear();
        }

        public virtual void showSelectionDisplayBoxes()
        {
            // See SelectionDisplayManager::showDisplayBoxes()
        }

        public virtual void updateSelectionDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false)
        {
            // See SelectionDisplayManager::updateDisplayBoxes(bool doPositionUpdate = true, bool setPosition = false)
        }

        public virtual void onSelectionDisplayBoxClicked(int index)
        {
            // See SelectionDisplayManager::onDisplayBoxClicked(int index)
        }
    }
}
