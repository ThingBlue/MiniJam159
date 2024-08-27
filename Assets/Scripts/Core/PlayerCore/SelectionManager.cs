using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;

using MiniJam159.GameCore;
using MiniJam159.AICore;

namespace MiniJam159.PlayerCore
{
    public class SelectionManager : MonoBehaviour
    {
        #region Inspector members

        public Material defaultMaterial;
        public Material selectedOutlineMaterial;

        public Color hoveredOutlineColor;
        public Color selectedOutlineColor;
        public Color deselectOutlineColor;

        #endregion

        public List<GameObject> selectedObjects;
        public List<List<GameObject>> Squads;
        public int focusSortPriority = -1;

        // Singleton
        public static SelectionManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            selectedObjects = new List<GameObject>();
        }

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.selectionCompleteEvent.AddListener(onSelectionCompleteCallback);
        }

        public void addSelectedObject(GameObject selectedObject)
        {
            selectedObjects.Add(selectedObject);

            // Add outline
            selectedObject.GetComponent<Entity>().setOutline(selectedOutlineMaterial, selectedOutlineColor);
        }

        public void setSelectedObjects(List<GameObject> newSelectedObjects)
        {
            selectedObjects = new List<GameObject>(newSelectedObjects);

            // Add outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                selectedObject.GetComponent<Entity>().setOutline(selectedOutlineMaterial, selectedOutlineColor);
            }
        }

        public void removeSelectedObject(GameObject selectedObject)
        {
            // Remove outline
            selectedObject.GetComponent<Entity>().clearOutline(selectedOutlineMaterial);

            selectedObjects.Remove(selectedObject);
        }

        public void removeSelectedObjectAtIndex(int index)
        {
            // Remove outline
            selectedObjects[index].GetComponent<Entity>().clearOutline(selectedOutlineMaterial);

            selectedObjects.RemoveAt(index);
        }

        public void clearSelectedObjects()
        {
            // Clear outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                selectedObject.GetComponent<Entity>().clearOutline(selectedOutlineMaterial);
            }

            selectedObjects.Clear();
            focusSortPriority = -1;
        }

        public void addOutlinesToSelectedObjects(Color outlineColor)
        {
            if (selectedObjects.Count == 0) return;

            // Add outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                selectedObject.GetComponent<Entity>().setOutline(selectedOutlineMaterial, outlineColor);
            }
        }

        public int getFocusIndex()
        {
            // No focus
            if (focusSortPriority == -1) return -1;

            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i].GetComponent<Entity>().sortPriority == focusSortPriority) return i;
            }

            // No focus found
            return -1;
        }

        public int getFocusIndexWithSortPriority(int sortPriority)
        {
            for (int i = 0; i < selectedObjects.Count; i++)
            {
                if (selectedObjects[i].GetComponent<Entity>().sortPriority == sortPriority) return i;
            }

            // No focus found
            return -1;
        }

        public int getSortPriorityWithIndex(int index)
        {
            return selectedObjects[index].GetComponent<Entity>().sortPriority;
        }

        private void onSelectionCompleteCallback()
        {
            // Sort selection
            selectedObjects.Sort(new EntityGameObjectComparer());

            // Clear focus if nothing is selected
            if (selectedObjects.Count == 0) focusSortPriority = 0;

            // Set focus to first entity is there is none
            if (selectedObjects.Count > 0 && getFocusIndex() == -1) focusSortPriority = selectedObjects[0].GetComponent<Entity>().sortPriority;

            EventManager.instance.selectionSortedEvent.Invoke();
        }

    }
}
