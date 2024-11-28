using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class HearingSensor : MonoBehaviour
{
    UnitController Unit;
    // Start is called before the first frame update
    void Start()
    {
        Unit = GetComponent<UnitController>();
        HearingManager.Instance.Register(this);
    }

    public void OnDestroy()
    {
        if (HearingManager.Instance != null) 
            HearingManager.Instance.Deregister(this);
    }

    public void OnHeardSound(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        //Outside hearing range
        if (Vector3.Distance(location, Unit.EyeLocation) > Unit.HearingRange)
            return;
        if (source != gameObject)
            Unit.ReportCanHear(source, location, category, intensity);
    }
}
