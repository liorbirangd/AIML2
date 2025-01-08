using System;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(AwarnessSensorsParameters))]
public class AwarenessSystem : MonoBehaviour
{
    [SerializeField] AnimationCurve VisionSensitivity;
    [SerializeField] float VisionMinimumAwareness = 1f;
    [SerializeField] public DetectableTargetManager detectableTargetManager;
    [SerializeField] public HearingManager hearingManager;

    [SerializeField]
    float VisionAwarenessBuildRate = 10f; //How fast an agent becomes aware if something is right in front of it
    //If 10, reacts in 1/10 of a second

    [SerializeField] float HearingMinimumAwareness = 0f;
    [SerializeField] float HearingAwarenessBuildRate = 5f;

    [SerializeField] float ProximityMinimumAwareness = 0f;
    [SerializeField] float ProximityAwarenessBuildRate = 1f;

    [SerializeField] float AwarenessDecayDelay = 0.1f;
    [SerializeField] float AwarenessDecayRate = 0.1f;

    Dictionary<GameObject, TrackedTarget> targets = new Dictionary<GameObject, TrackedTarget>();

    AwarnessSensorsParameters Unit;

    // Start is called before the first frame update
    void Start()
    {
        Unit = GetComponent<AwarnessSensorsParameters>();
        List<DetectableTarget> initialTargets = detectableTargetManager.Listeners;
        foreach (DetectableTarget target in initialTargets)
        {
            if (target.gameObject != gameObject)
                targets.Add(target.gameObject, new TrackedTarget(target, target.transform.position));
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var targetGO in targets.Keys)
        {
            targets[targetGO].DecayAwareness(AwarenessDecayDelay, AwarenessDecayRate * Time.deltaTime);
        }
    }

    void UpdateAwareness(GameObject targetGO, DetectableTarget target, Vector3 position, float awareness,
        float minAwareness)
    {
        if (targetGO != gameObject)
            targets[targetGO].UpdateAwareness(target, position, awareness, minAwareness);
    }

    public void ReportCanSee(DetectableTarget seen)
    {
        //Determine where in the field of view, something is seen
        var vectorToTarget = (seen.transform.position - Unit.EyeLocation).normalized;
        var dotProduct = Vector3.Dot(vectorToTarget, Unit.EyeDirection);

        //Calculate awareness contribution
        var awareness = VisionSensitivity.Evaluate(dotProduct) * VisionAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(seen.gameObject, seen, seen.transform.position, awareness, VisionMinimumAwareness);
    }

    public void ReportCanHear(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        //Calculate awareness contribution
        var awareness = intensity * HearingAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(source, null, location, awareness, HearingMinimumAwareness);
    }

    public void ReportInProximity(DetectableTarget target)
    {
        //Calculate awareness contribution
        var awareness = ProximityAwarenessBuildRate * Time.deltaTime;

        UpdateAwareness(target.gameObject, target, target.transform.position, awareness, ProximityMinimumAwareness);
    }

    public void AddObservations(VectorSensor sensor)
    {
        foreach (var target in targets.Keys)
        {
            TrackedTarget trackedTarget = targets[target];
            sensor.AddObservation(GetTagValue(target.tag));
            sensor.AddObservation(trackedTarget.RawPosition);
            sensor.AddObservation(trackedTarget.awareness);
        }
    }

    private static int GetTagValue(string tag)
    {
        switch (tag)
        {
            case "purpleAgent": return 0;
            case "blueAgent": return 1;
            case "ball": return 2;
            default: return -1;
        }
    }
}