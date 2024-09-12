using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159.GameCore
{
    [CreateAssetMenu]
    public class SettingsData : ScriptableObject
    {
        [Header("CONTROLS")]
        public List<KeyBind> keyBinds;
        public float doubleClickMaxDelay;
    }
}
