using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectableTarget : MonoBehaviour
{
    AwarenessSystem awarenessSystem;
    // Start is called before the first frame update
    void Awake()
    {
        awarenessSystem = GetComponent<AwarenessSystem>();
        awarenessSystem.detectableTargetManager.Register(this);
    }

    void OnDestroy()
    {
        if (awarenessSystem.detectableTargetManager != null) awarenessSystem.detectableTargetManager.Deregister(this);
    }
}