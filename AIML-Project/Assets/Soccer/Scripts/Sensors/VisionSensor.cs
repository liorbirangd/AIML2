using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AwarnessSensorsParameters))]
public class VisionSensor : MonoBehaviour
{
    [SerializeField] LayerMask DetectionMask = ~0;

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
        //Check all candidates
        for (int i = 0; i < awarenessSystem.detectableTargetManager.Listeners.Count; ++i) 
        {
            var candidateTarget = awarenessSystem.detectableTargetManager.Listeners[i];

            //Skip if the candidate is ourselves
            if (candidateTarget.gameObject == gameObject) continue;

            var vectorToTarget = candidateTarget.transform.position - unit.EyeDirection;

            vectorToTarget.Normalize();

            //If out of range, cannot see
            if (vectorToTarget.sqrMagnitude > (unit.VisionConeRange * unit.VisionConeRange)) continue;

            //If out of vision cone, cannot see
            if (Vector3.Dot(vectorToTarget, unit.EyeDirection) < unit.CosVisionConeAngle) continue;

            //Raycast to target passes?
            RaycastHit hitResult;
            if (Physics.Raycast(unit.EyeLocation, vectorToTarget, out hitResult, unit.VisionConeRange, DetectionMask, QueryTriggerInteraction.Collide))
            {
                if (hitResult.collider.GetComponentInParent<DetectableTarget>() == candidateTarget) 
                    awarenessSystem.ReportCanSee(candidateTarget);
            }
        }
    }
}
