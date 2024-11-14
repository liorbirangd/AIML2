using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnitController))]
public class VisionSensor : MonoBehaviour
{
    [SerializeField] LayerMask DetectionMask = ~0;

    UnitController Unit;

    // Start is called before the first frame update
    void Start()
    {
        Unit = GetComponent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check all candidates
        for (int i = 0; i < DetectableTargetManager.Instance.AllTargets.Count; ++i) 
        {
            var candidateTarget = DetectableTargetManager.Instance.AllTargets[i];

            //Skip if the candidate is ourselves
            if (candidateTarget.gameObject == gameObject) continue;

            var vectorToTarget = candidateTarget.transform.position - Unit.EyeDirection;

            vectorToTarget.Normalize();

            //If out of range, cannot see
            if (vectorToTarget.sqrMagnitude > (Unit.VisionConeRange * Unit.VisionConeRange)) continue;

            //If out of vision cone, cannot see
            if (Vector3.Dot(vectorToTarget, Unit.EyeDirection) < Unit.CosVisionConeAngle) continue;

            //Raycast to target passes?
            RaycastHit hitResult;
            if (Physics.Raycast(Unit.EyeLocation, vectorToTarget, out hitResult, Unit.VisionConeRange, DetectionMask, QueryTriggerInteraction.Collide))
            {
                if (hitResult.collider.GetComponentInParent<DetectableTarget>() == candidateTarget) 
                    Unit.ReportCanSee(candidateTarget);
            }
        }
    }
}
