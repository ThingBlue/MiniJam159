using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    public enum GameState
    {
        NONE = 0,
        MAIN_MENU,
        LOADING,
        GAME,
        PAUSED
    }

    public class SettingsManager : MonoBehaviour
    {
        #region Inspector members

        public SettingsData settingsData;

        #endregion

        // Singleton
        public static SettingsManager instance;

        public static GameState gameState = GameState.NONE;

        private float timeScaleBeforePause = 0;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // Set fullscreen
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

            // TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP
            gameState = GameState.GAME;
        }

        private void Start()
        {
            // Subscribe to events
            EventManager.instance.pauseEvent.AddListener(onPauseCallback);
            EventManager.instance.unpauseEvent.AddListener(onUnpauseCallback);

            // Apply keybinds
            applyKeybinds();
        }

        private void Update()
        {
            // Pause
            if (gameState == GameState.GAME && InputManager.instance.getKeyDown("pause")) EventManager.instance.pauseEvent.Invoke();
            // Unpause
            else if (gameState == GameState.PAUSED && InputManager.instance.getKeyDown("pause")) EventManager.instance.unpauseEvent.Invoke();
        }

        private void applyKeybinds()
        {
            // Clear current key binds
            InputManager.instance.clearKeyMap();

            // Add every key bind in list to input manger
            foreach (KeyBind keyBind in settingsData.keyBinds)
            {
                InputManager.instance.setKeyListInMap(keyBind.name, keyBind.keys);
            }
        }


        #region Event system callbacks

        private void onPauseCallback()
        {
            gameState = GameState.PAUSED;
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0;
        }

        private void onUnpauseCallback()
        {
            gameState = GameState.GAME;
            Time.timeScale = timeScaleBeforePause;
        }

        #endregion
    }
}
