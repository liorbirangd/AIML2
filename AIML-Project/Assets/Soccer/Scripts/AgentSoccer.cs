using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using System;
using static UnityEngine.GraphicsBuffer;
using System.Collections;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player

    private int teamChangeSteps = 200000; // Matches the `team_change` parameter in config
    private int currentStep = 0;

    private string currentLearningTeam = "Blue"; // Initial learning team

    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }


    private HearingSensorController hearingSensor;
    
    [HideInInspector] public Team team;

    float m_KickPower;

    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;
    float soundTimer = 0f;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;


    [HideInInspector] public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;
    private AwarenessSystem awarenessSystem;

    EnvironmentParameters m_ResetParams;

    SoccerEnvController envController;

    //private int positionalRewardStepCounter = 0; // Step counter for positional rewards
    //private const int positionalRewardStepInterval = 100; // Execute every 100 steps

    public override void Initialize()
    {
        envController = GetComponentInParent<SoccerEnvController>();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }

        if (position == Position.Goalie)
        {
            m_LateralSpeed = 0.5f;
            m_ForwardSpeed = 0.5f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 0.6f;
        }
        else
        {
            m_LateralSpeed = 0.2f;
            m_ForwardSpeed = 0.5f;
        }

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;
        hearingSensor = GetComponentInChildren<HearingSensorController>();

        m_ResetParams = Academy.Instance.EnvironmentParameters;
        awarenessSystem = GetComponentInChildren<AwarenessSystem>();
    }

    void ScoreLogger()
    {
        // Increment step counter
        currentStep = Academy.Instance.StepCount;
        // Detect when the learning team changes
        if (currentStep % teamChangeSteps == 0 && currentStep > 0)
        {
            // Log the results
            envController.LogScores(currentStep, currentLearningTeam);

            // Alternate the learning team
            currentLearningTeam = (currentLearningTeam == "Blue") ? "Purple" : "Blue";
            envController.SetCurrentLearningTeam(currentLearningTeam);

            // Reset counters for the new phase
            envController.ClearGoals();
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        ScoreLogger();

        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);

            // Execute the BallBasedPositionalReward only at specific intervals
            //positionalRewardStepCounter++;
            //if (positionalRewardStepCounter >= positionalRewardStepInterval)
            //{
            //    BallBasedPositionalReward();
            //    positionalRewardStepCounter = 0; // Reset counter
            //}
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }

        MoveAgent(actionBuffers.DiscreteActions);
    }


    //private void BallBasedPositionalReward()
    //{
    //    // Ball and goal references
    //    var ball = GameObject.FindWithTag("ball");
    //    if (ball == null) return;

    //    // Define the position of the goal
    //    GameObject ownGoal = team == Team.Blue
    //        ? envController.blueGoal
    //        : envController.purpleGoal;

    //    // Get the ball's position
    //    Vector3 ballPosition = ball.transform.position;


    //    // Define a defensive zone threshold (distance from the goal)
    //    float defensiveZoneRadius = 15.5f; // Adjust based on your field size
    //    // Calculate distance from ball to the goal
    //    float distanceBallToGoal = Vector3.Distance(ballPosition, ownGoal.transform.position);
    //    DebugFileLogger.Log($"Ball to Goal Distance: {distanceBallToGoal} < 15.5");
    //    // Only reward if the ball is within the defensive zone
    //    if (distanceBallToGoal <= defensiveZoneRadius)
    //    {
    //        float distanceGoalieToGoal = Vector3.Distance(transform.position, ownGoal.transform.position);

    //        // Reward the goalie for being closer to the goal than the ball
    //        if (distanceGoalieToGoal < distanceBallToGoal)
    //        {
    //            AddReward(0.05f); // Reward for being closer
    //            DebugFileLogger.Log($"Goalie rewarded for being closer to the goal than the ball. Distance: {distanceGoalieToGoal} < {distanceBallToGoal}");
    //        }
    //        else
    //        {
    //            DebugFileLogger.Log("No reward. Goalie is farther from the goal than the ball but no penalty applied.");
    //        }
    //    }
    //    else
    //    {
    //        DebugFileLogger.Log("No positional reward. Ball is outside the defensive zone.");
    //    }
    //}

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }

        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }

        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
    }

    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }

    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }

        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * m_BallTouch);
            DebugFileLogger.Log("Agent Touch Reward.");

            // Apply force to the ball
            var ballRb = c.gameObject.GetComponent<Rigidbody>();
            var dirToBall = c.contacts[0].point - transform.position; // Direction to the contact point
            dirToBall = dirToBall.normalized;
            // Save the ball's velocity before the collision
            Vector3 preContactVelocity = ballRb.velocity.normalized;

            ballRb.AddForce(dirToBall * force); // Apply force

            // Call the delayed reward check
            StartCoroutine(DelayedRewardCheck(preContactVelocity, ballRb, c.gameObject));

            // Emit sound event
            awarenessSystem.hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EKick, 3f);
        }
    }

    private IEnumerator DelayedRewardCheck(Vector3 preContactVelocity, Rigidbody ballRb, GameObject ball)
    {
        yield return new WaitForFixedUpdate(); // Wait for the physics system to update the velocity

        // Determine goal tags
        String ownGoalTag = team == Team.Blue ? "blueGoal" : "purpleGoal";
        String opponentGoalTag = team == Team.Blue ? "purpleGoal" : "blueGoal";
        float rayDistance = 40f;

        // Check if the ball was heading toward the goal before contact
        DebugFileLogger.Log($"       Block Check...");
        DebugFileLogger.Log($"Before contact, the ball: ");
        bool wasHeadingToGoal = IsBallHeadingTowardsGoal(preContactVelocity, ball.transform.position, ownGoalTag, rayDistance);
        DebugFileLogger.Log($"so wasHeadingTowardGoal: {wasHeadingToGoal}");

        if (wasHeadingToGoal)
        {
            DebugFileLogger.Log($"After contact, the ball: ");
            Vector3 postContactVelocity = ballRb.velocity.normalized; // Get updated velocity
            bool isHeadingAwayFromGoal = !IsBallHeadingTowardsGoal(postContactVelocity, ball.transform.position, ownGoalTag, rayDistance);
            DebugFileLogger.Log($"so isHeadingAwayFromGoal: {isHeadingAwayFromGoal}");

            // Reward for blocking a potential goal
            if (isHeadingAwayFromGoal)
            {
                AddReward(0.5f); // Reward for blocking
                DebugFileLogger.Log("Reward for blocking a potential goal.");
            }
            else
            {
                DebugFileLogger.Log("Ball wasn't blocked successfully.");
            }
        }
        else
        {
            DebugFileLogger.Log("Since false, no further check needed.");
        }

        DebugFileLogger.Log($"       Kick Direction Check...");
        // Check if the ball is heading toward the opponent's goal
        Vector3 postContactVelocityCheck = ballRb.velocity.normalized; // Recheck velocity
        DebugFileLogger.Log($"After Kicking the ball: ");
        if (IsBallHeadingTowardsGoal(postContactVelocityCheck, ball.transform.position, opponentGoalTag, rayDistance))
        {
            AddReward(0.2f); // Reward for kicking toward opponent's goal
            DebugFileLogger.Log("Reward for kicking toward opponent's goal area.");
        }
        else if (IsBallHeadingTowardsGoal(postContactVelocityCheck, ball.transform.position, ownGoalTag, 25f))
        {
            AddReward(-0.2f); // Penalty for kicking toward own goal
            DebugFileLogger.Log("Penalty for kicking toward own goal.");
        }
        else
        {
            DebugFileLogger.Log("No penalty or reward as kick direction is neutral.");
        }
    }

    private bool IsBallHeadingTowardsGoal(Vector3 ballDirection, Vector3 ballPosition, String goalTag, float rayDistance)
    {
        LayerMask targetLayer;
        targetLayer = LayerMask.GetMask("Goal");

        // Perform a raycast in the direction of the velocity
        Ray ray = new Ray(ballPosition, ballDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayer))
        {
            // Optional: Check if the ray hits the target
            if (hit.transform.CompareTag(goalTag))
            {
                DebugFileLogger.Log("was moving towards goal");
                return true; // Moving towards and ray hits target
            }
        }
        DebugFileLogger.Log("was not moving towards goal");
        return false; // Moving towards based on direction
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        if (hearingSensor != null)
            hearingSensor.AddObservations(sensor);
        if(awarenessSystem != null)
            awarenessSystem.AddObservations(sensor);
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 50f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);


        MovmentSound();
    }

    private void MovmentSound()
    {
        if (!awarenessSystem)
            return;
        //Limited how often a 'sound' is made because it caused framerate drops when there are multiple fields
        soundTimer++;
        if (soundTimer >= 15)
        {
            awarenessSystem.hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EFootstep,
                2f);
            soundTimer = 0;
        }
    }
}