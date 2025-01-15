using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AwarnessSensorsParameters))]
public class HearingSensor : MonoBehaviour
{
    AwarnessSensorsParameters unit;
    AwarenessSystem awarenessSystem;
    // Start is called before the first frame update
    void Start()
    {
        unit = GetComponent<AwarnessSensorsParameters>();
        awarenessSystem = GetComponent<AwarenessSystem>();
        awarenessSystem.hearingManager.Register(this);
    }

    public void OnDestroy()
    {
        if (awarenessSystem != null && awarenessSystem.hearingManager != null) 
            awarenessSystem.hearingManager.Deregister(this);
    }

    public void OnHeardSound(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        //Outside hearing range
        if (Vector3.Distance(location, unit.EyeLocation) > unit.HearingRange)
            return;
        if (source != gameObject)
            awarenessSystem.ReportCanHear(source, location, category, intensity);
    }
}
