using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTarget : MonoBehaviour
{
    [SerializeField] private DetectableTargetManager detectableTargetManager;

    // Start is called before the first frame update
    void Awake()
    {
        detectableTargetManager.Register(this);
    }

    void OnDestroy()
    {
        if (detectableTargetManager != null) detectableTargetManager.Deregister(this);
    }
}