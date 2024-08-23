using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.PlayerCore
{
    public enum PlayerMode
    {
        NORMAL = 0,
        MASS_SELECT,
        STRUCTURE_PLACEMENT,
        MOVE_TARGET,
        ATTACK_TARGET,
        HARVEST_TARGET
    }

    public class PlayerModeManager : MonoBehaviour
    {
        public PlayerMode playerMode = PlayerMode.NORMAL;

        // Singleton
        public static PlayerModeManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void FixedUpdate()
        {
            //print(playerMode);
        }
    }
}
