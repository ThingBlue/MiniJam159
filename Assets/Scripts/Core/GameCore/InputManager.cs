using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MiniJam159.GameCore
{
    [Serializable]
    public class KeyBind
    {
        public string name;
        public List<KeyCode> keys;

        public KeyBind(string name, List<KeyCode> keys)
        {
            this.name = name;
            this.keys = keys;
        }
    }

    public class InputManager : MonoBehaviour
    {
        #region Inspector members

        public EventSystem eventSystem;
        public float mouseRaycastDistance;

        #endregion

        public Dictionary<string, List<KeyCode>> keyMap;
        public List<string> keysInMap;

        // Singleton
        public static InputManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // Initialize key map
            keyMap = new Dictionary<string, List<KeyCode>>();
            keysInMap = new List<string>();
        }

        #region Getters

        public bool getKey(string key)
        {
            if (!keyMap.ContainsKey(key)) return false;

            foreach (KeyCode keyCode in keyMap[key])
            {
                if (Input.GetKey(keyCode)) return true;
            }
            return false;
        }

        public bool getKeyDown(string key)
        {
            if (!keyMap.ContainsKey(key)) return false;

            foreach (KeyCode keyCode in keyMap[key])
            {
                if (Input.GetKeyDown(keyCode)) return true;
            }
            return false;
        }

        public bool getKeyUp(string key)
        {
            if (!keyMap.ContainsKey(key)) return false;

            foreach (KeyCode keyCode in keyMap[key])
            {
                if (Input.GetKeyUp(keyCode)) return true;
            }
            return false;
        }

        // Getter for key map
        public List<KeyCode> getKeysInMap(string key)
        {
            if (!keyMap.ContainsKey(key)) return new List<KeyCode>();

            return keyMap[key];
        }

        // Getter for mouse position in world space
        public Vector3 getMousePositionInWorld()
        {
            // Create plane at zero
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            // Cast ray onto plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Return hit location
            if (plane.Raycast(ray, out float enter)) return ray.GetPoint(enter);
            return Vector3.zero; // Should never execute
        }

        // Returns first object hit by raycast
        public GameObject mouseRaycastObject(LayerMask layerMask)
        {
            // Raycast from mouse and grab first hit
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, mouseRaycastDistance, layerMask)) return hit.collider.gameObject;
            return null;
        }

        // Returns all objects hit by raycast
        public List<GameObject> mouseRaycastAll(LayerMask layerMask)
        {
            // Raycast from mouse and grab all hits
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(mouseRay, mouseRaycastDistance, layerMask);

            // Sort by distance
            hits = hits.OrderBy(hit => hit.distance).ToArray();

            // Convert hits to a list of gameobjects
            List<GameObject> hitObjects = new List<GameObject>();
            foreach (RaycastHit hit in hits) hitObjects.Add(hit.collider.gameObject);
            return hitObjects;
        }

        // Returns all objects hit by event system raycast
        public List<GameObject> mouseRaycastAllUI()
        {
            // Raycast from mouse and grab all hits
            PointerEventData raycastData = new PointerEventData(eventSystem) { position = Input.mousePosition };
            List<RaycastResult> hits = new List<RaycastResult>();

            eventSystem.RaycastAll(raycastData, hits);

            // Convert hits to a list of gameobjects
            List<GameObject> hitObjects = new List<GameObject>();
            foreach (RaycastResult hit in hits) hitObjects.Add(hit.gameObject);
            return hitObjects;
        }

        // Returns position of first raycast hit
        public Vector3 mouseRaycastPosition(LayerMask layerMask)
        {
            // Raycast from mouse and grab first hit
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit, mouseRaycastDistance, layerMask)) return hit.point;
            return Vector3.zero;
        }

        #endregion

        #region Setters

        // Clears all binds for a single key in map
        public void clearKeyListInMap(string key)
        {
            if (!keyMap.ContainsKey(key)) return;

            keyMap[key].Clear();
        }

        // Clears the entire map
        public void clearKeyMap()
        {
            foreach (string key in keysInMap) clearKeyListInMap(key);
            keysInMap.Clear();
        }

        public void addKeyToMap(string key, KeyCode value)
        {
            // Create new keycode mapping if it doesn't exist
            if (!keyMap.ContainsKey(key))
            {
                keyMap.Add(key, new List<KeyCode>());
                keysInMap.Add(key);
            }

            // Check if current value already exists in the list
            if (keyMap[key].Contains(value)) return;

            keyMap[key].Add(value);
        }

        public void setKeyListInMap(string key, List<KeyCode> value)
        {
            // Create new keycode mapping if it doesn't exist
            if (!keyMap.ContainsKey(key))
            {
                keyMap.Add(key, value);
                keysInMap.Add(key);
            }
            else keyMap[key] = value;
        }

        #endregion
    }
}
