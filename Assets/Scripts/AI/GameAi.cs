using UnityEngine;
using System.Collections.Generic;

public abstract class GameAI : MonoBehaviour
{
    public static List<GameAI> allAIs = new List<GameAI>(); // List of all AI instances

    protected virtual void Start()
    {
        allAIs.Add(this);
    }

    protected virtual void OnDestroy()
    {
        allAIs.Remove(this);
    }
}
