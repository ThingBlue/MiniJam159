using MiniJam159.GameCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniJam159
{
    [CreateAssetMenu]
    public class SettingsData : ScriptableObject
    {
        [Header("CONTROLS")]
        public List<KeyBind> keyBinds;
    }
}
