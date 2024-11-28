using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTargetManager : MonoBehaviour
{
    public List<DetectableTarget> Listeners { get; private set; } = new List<DetectableTarget>();

    public void Register(DetectableTarget target)
    {
        Listeners.Add(target);
    }

    public void Deregister(DetectableTarget target)
    {
        Listeners.Remove(target);
    }
}