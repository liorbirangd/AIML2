using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif //UNITY_EDITOR

[RequireComponent(typeof(AwarenessSystem))]
public class UnitController : MonoBehaviour
{

    [SerializeField] TextMeshProUGUI FeedbackDisplay;

    [SerializeField] float _VisionConeAngle = 60f;
    [SerializeField] float _VisionConeRange = 30f;
    [SerializeField] Color _VisionConeColour = new Color(1f, 0f, 0f, 0.25f);

    [SerializeField] float _HearingRange = 20f;
    [SerializeField] Color _HearingRangeColour = new Color(1f, 1f, 0f, 0.25f);

    [SerializeField] float _ProximityDetectionRange = 3f;
    [SerializeField] Color _ProximityRangeColour = new Color(1f, 1f, 1f, 0.25f);

    public Vector3 EyeLocation => transform.position;
    public Vector3 EyeDirection => transform.forward;

    public float VisionConeAngle => _VisionConeAngle;
    public float VisionConeRange => _VisionConeRange;
    public Color VisionConeColour => _VisionConeColour;

    public float HearingRange => _HearingRange;
    public Color HearingRangeColour => _HearingRangeColour;

    public float ProximityDetectionRange => _ProximityDetectionRange;
    public Color ProximityDetectionColour => _ProximityRangeColour;

    public float CosVisionConeAngle { get; private set; } = 0f;

    AwarenessSystem Awareness;

    void Awake() 
    {
        CosVisionConeAngle = Mathf.Cos(VisionConeAngle * Mathf.Deg2Rad);
        Awareness = GetComponent<AwarenessSystem>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReportCanSee(DetectableTarget seen)
    {
        Awareness.ReportCanSee(seen);
        //Debug.Log("Can see " + seen.gameObject.name);
    }

    public void ReportCanHear(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        Awareness.ReportCanHear(source, location, category, intensity);
        //Debug.Log("Heard sound " + category + " at " + location.ToString() + " with intensity " + intensity);
    }

    public void ReportInProximity(DetectableTarget target)
    {
        Awareness.ReportInProximity(target);
        //Debug.Log("Can sense " + target.gameObject.name);
    }


    public void OnSuspicious()
    {
        Debug.Log("Gained Suspicion");
        //What to do when it knows somethings is close but does not know where it is; Awareness increased to 0-1
    }

    public void OnDetected(GameObject target)
    {
        Debug.Log("Detected Target");
        //What to do when a target is seen; Awareness increased to 1-2
    }

    public void OnFullyDetected(GameObject target)
    {
        Debug.Log("Fully Detected");
        //Awareness goes above 2
    }

    public void OnLostFullDetection(GameObject target)
    {
        Debug.Log("Lost Full Detection");
        //What to do when losing sight of a target; Awareness decreased to 1-2
    }

    public void OnLostDetection()
    {
        Debug.Log("Lost Detection");
        //What to do when completely losing track of a target; Awareness decreased to 0-1
    }

    public void OnLostSuspicion()
    {
        Debug.Log("Fully lost target");
        //Target is removed from tracked target list; Awareness decreased to 0
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnitController))]
public class EnemyAIEditor : Editor
{
    public void OnSceneGUI()
    {
        var ai = target as UnitController;

        // draw the detectopm range
        Handles.color = ai.ProximityDetectionColour;
        Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.ProximityDetectionRange);

        // draw the hearing range
        Handles.color = ai.HearingRangeColour;
        Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.HearingRange);

        // work out the start point of the vision cone
        Vector3 startPoint = Mathf.Cos(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.forward +
                             Mathf.Sin(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.right;

        // draw the vision cone
        Handles.color = ai.VisionConeColour;
        Handles.DrawSolidArc(ai.transform.position, Vector3.up, startPoint, ai.VisionConeAngle * 2f, ai.VisionConeRange);
    }
}
#endif // UNITY_EDITOR
