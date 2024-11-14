using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class ProximitySensor : MonoBehaviour
{
    UnitController Unit;
    // Start is called before the first frame update
    void Start()
    {
        Unit = GetComponent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < DetectableTargetManager.Instance.AllTargets.Count; ++i)
        {
            var candidateTarget = DetectableTargetManager.Instance.AllTargets[i];

            if (candidateTarget.gameObject == gameObject) continue;
            if (Vector3.Distance(Unit.EyeLocation, candidateTarget.transform.position) <= Unit.ProximityDetectionRange)
            {
                Unit.ReportInProximity(candidateTarget);
            }
        }
    }
}
