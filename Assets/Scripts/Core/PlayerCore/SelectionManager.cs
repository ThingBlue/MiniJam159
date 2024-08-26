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

        public void clearSelectedObjects()
        {
            // Clear outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                MeshRenderer renderer = selectedObject.transform.Find("Mesh").GetComponent<MeshRenderer>();
                Material[] newMaterials = new Material[2];
                renderer.materials.CopyTo(newMaterials, 0);
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    if (renderer.materials[i].name == selectedOutlineMaterial.name + " (Instance)")
                    {
                        //renderer.materials[i] = defaultMaterial;
                        newMaterials[i] = null;
                    }
                }
                renderer.materials = newMaterials;
            }

            selectedObjects.Clear();
        }

        public void addOutlinesToSelectedObjects()
        {
            if (selectedObjects.Count == 0) return;

            // Add outlines
            foreach (GameObject selectedObject in selectedObjects)
            {
                MeshRenderer renderer = selectedObject.transform.Find("Mesh").GetComponent<MeshRenderer>();
                Material[] newMaterials = renderer.materials;
                bool hasOutlineMaterial = false;
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    if (newMaterials[i].name == selectedOutlineMaterial.name + " (Instance)")
                    {
                        hasOutlineMaterial = true;
                        break;
                    }
                }
                if (!hasOutlineMaterial)
                {
                    newMaterials[0] = new Material(selectedOutlineMaterial);
                    renderer.materials = newMaterials;
                }
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
