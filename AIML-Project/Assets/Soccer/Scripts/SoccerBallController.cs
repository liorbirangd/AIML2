using UnityEngine;

public class SoccerBallController : MonoBehaviour
{
    public GameObject area;
    [HideInInspector] public SoccerEnvController envController;
    public HearingManager hearingManager;
    public string purpleGoalTag; //will be used to check if collided with purple goal
    public string blueGoalTag; //will be used to check if collided with blue goal
    [HideInInspector] public GameObject lastToucher;
    [HideInInspector] public GameObject assistAgent;

    private Rigidbody ballRb;
    private bool ballWasMoving; // Tracks if the ball is currently moving
    public bool previousBallWasMoving; // NEW: Tracks if the ball was moving before the current frame

    void Start()
    {
        envController = area.GetComponent<SoccerEnvController>();
        ballRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Save the current state of ball movement for the next frame
        previousBallWasMoving = ballWasMoving; // Cache the previous state

        // Update the current state of ball movement
        ballWasMoving = ballRb.velocity.magnitude > 0.1f; // Threshold for movement
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("blueAgent") || col.gameObject.CompareTag("purpleAgent")) // Check for agent tags
        {
            // Update assistAgent only if the ball was moving
            if (previousBallWasMoving)
            {
                assistAgent = lastToucher; // Store the last toucher as the assisting agent
                DebugFileLogger.Log($"Assist agent updated: {assistAgent?.name}");
            }

            // Update the last toucher to the current agent
            lastToucher = col.gameObject;
            DebugFileLogger.Log($"Last toucher updated: {lastToucher.name}");
        }

        if (col.gameObject.CompareTag(purpleGoalTag)) // Ball touched purple goal
        {
            // Ensure assistAgent and lastToucher are updated BEFORE calling GoalTouched
            envController.GoalTouched(Team.Blue);
        }

        if (col.gameObject.CompareTag(blueGoalTag)) // Ball touched blue goal
        {
            // Ensure assistAgent and lastToucher are updated BEFORE calling GoalTouched
            envController.GoalTouched(Team.Purple);
        }

        if (col.gameObject.CompareTag("wall"))
        {
            if (hearingManager != null)
                hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.ECollision, 5f);
        }
    }
}