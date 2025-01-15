using UnityEngine;

public class SoccerBallController : MonoBehaviour
{
    public GameObject area;
    [HideInInspector] public SoccerEnvController envController;
    public HearingManager hearingManager;
    public string purpleGoalTag; //will be used to check if collided with purple goal
    public string blueGoalTag; //will be used to check if collided with blue goal

    private Experiment experiment;

    void Start()
    {
        envController = area.GetComponent<SoccerEnvController>();
<<<<<<< Updated upstream
=======
        ballRb = GetComponent<Rigidbody>();
    }

    public void SetExperiment(Experiment experiment)
    {
        this.experiment = experiment;
    }

    void FixedUpdate()
    {
        // Save the current state of ball movement for the next frame
        previousBallWasMoving = ballWasMoving; // Cache the previous state

        // Update the current state of ball movement
        ballWasMoving = ballRb.velocity.magnitude > 0.1f; // Threshold for movement
>>>>>>> Stashed changes
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag(purpleGoalTag)) //ball touched purple goal
        {
            envController.GoalTouched(Team.Blue);
            experiment.BlueTeamScored();
        }

        if (col.gameObject.CompareTag(blueGoalTag)) //ball touched blue goal
        {
            envController.GoalTouched(Team.Purple);
            experiment.PurpleTeamScored();
        }

        if (col.gameObject.CompareTag("wall"))
        {
            if (hearingManager != null)
                hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.ECollision, 5f);
        }
    }
}