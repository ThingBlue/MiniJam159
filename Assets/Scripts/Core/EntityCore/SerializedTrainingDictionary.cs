using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

using MiniJam159.EntityCore;

namespace MiniJam159.EntityCore
{
    public struct EntityTrainingData
    {
        public GameObject prefab;
        public float time;
    }

    [CreateAssetMenu]
    public class SerializedTrainingDictionary : SerializedScriptableObject
    {
        public Dictionary<string, EntityTrainingData> data;
    }
}
