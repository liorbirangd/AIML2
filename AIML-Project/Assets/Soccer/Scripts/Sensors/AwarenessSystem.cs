using MLAgentsExamples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class TrackedTarget
{
    public DetectableTarget Detectable;
    public Vector3 RawPosition;

    public float LastSensedTime = -1f;
    public float Awareness; //0 = not aware (culled)
                            //0-1 = rough idea (no location)
                            //1-2 = liekly target (location)
                            //2 = fully detected
    public bool UpdateAwareness(DetectableTarget target, Vector3 position, float awareness, float minAwareness)
    {
        var oldAwareness = Awareness;

        if (target != null) 
            Detectable = target;
        RawPosition = position;
        LastSensedTime = Time.time;
        Awareness = Mathf.Clamp(Mathf.Max(Awareness, minAwareness) + awareness, 0f, 2f);

        if (oldAwareness < 2f && Awareness >= 2f)
            return true;
        if (oldAwareness < 1f && Awareness >= 1f)
            return true;
        if (oldAwareness <= 0f && Awareness >= 0f)
            return true;

        return false;
    }

    public bool DecayAwareness(float decayTime, float amount)
    {
        //Detected to recently
        if ((Time.time - LastSensedTime) < decayTime)
            return false;

        var oldAwareness = Awareness;

        Awareness -= amount;

        if (oldAwareness >= 2f && Awareness < 2f)
            return true;
        if (oldAwareness >= 1f && Awareness < 1f)
            return true;
        
        return Awareness <= 0f;
    }

}

[RequireComponent(typeof(UnitController))]
public class AwarenessSystem : MonoBehaviour
{
    [SerializeField] AnimationCurve VisionSensitivity;
    [SerializeField] float VisionMinimumAwareness = 1f;
    [SerializeField] float VisionAwarenessBuildRate = 10f; //How fast an agent becomes aware if something is right in front of it
                                                           //If 10, reacts in 1/10 of a second

    [SerializeField] float HearingMinimumAwareness = 0f;
    [SerializeField] float HearingAwarenessBuildRate = 5f;

    [SerializeField] float ProximityMinimumAwareness = 0f;
    [SerializeField] float ProximityAwarenessBuildRate = 1f;

    [SerializeField] float AwarenessDecayDelay = 0.1f;
    [SerializeField] float AwarenessDecayRate = 0.1f;

    Dictionary<GameObject, TrackedTarget> Targets = new Dictionary<GameObject, TrackedTarget>();

    UnitController Unit;

    // Start is called before the first frame update
    void Start()
    {
        Unit = GetComponent<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        List<GameObject> toCleanUp = new List<GameObject>();
        foreach(var targetGO in Targets.Keys)
        {
            if (Targets[targetGO].DecayAwareness(AwarenessDecayDelay, AwarenessDecayRate * Time.deltaTime))
            {
                if (Targets[targetGO].Awareness <= 0f)
                {
                    Unit.OnLostSuspicion();
                    toCleanUp.Add(targetGO);
                }
                else 
                {
                    if (Targets[targetGO].Awareness >= 1f)
                        Unit.OnLostFullDetection(targetGO);
                    else Unit.OnLostDetection();
                }
            }
        }

        //Clean up targets that are no longer detected
        foreach (var target in toCleanUp)        
            Targets.Remove(target);        
    }

    void UpdateAwareness(GameObject targetGO, DetectableTarget target, Vector3 position, float awareness, float minAwareness)
    {
        //Not in targets
        if (!Targets.ContainsKey(targetGO))
            Targets[targetGO] = new TrackedTarget();

        //Update target awareness
        if (Targets[targetGO].UpdateAwareness(target, position, awareness, minAwareness))
        {
            if (Targets[targetGO].Awareness >= 2f)
                Unit.OnFullyDetected(targetGO);
            else if (Targets[targetGO].Awareness >= 1f) 
                Unit.OnDetected(targetGO);
            else 
                Unit.OnSuspicious();
        }
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
}
