using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AwarnessSensorsParameters))]
public class ProximitySensor : MonoBehaviour
{
    AwarnessSensorsParameters unit;
    AwarenessSystem awarenessSystem;
    // Start is called before the first frame update
    void Start()
    {
        awarenessSystem = GetComponent<AwarenessSystem>();
        unit = GetComponent<AwarnessSensorsParameters>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < awarenessSystem.detectableTargetManager.Listeners.Count; ++i)
        {
            var candidateTarget = awarenessSystem.detectableTargetManager.Listeners[i];

            if (candidateTarget.gameObject == gameObject) continue;
            if (Vector3.Distance(unit.EyeLocation, candidateTarget.transform.position) <= unit.ProximityDetectionRange)
            {
                awarenessSystem.ReportInProximity(candidateTarget);
            }
        }
    }
}
