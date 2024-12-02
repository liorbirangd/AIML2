using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

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

    private int positionalRewardStepCounter = 0; // Step counter for positional rewards
    private const int positionalRewardStepInterval = 50; // Execute every 50 steps

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
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

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);

            // Execute the BallBasedPositionalReward only at specific intervals
            positionalRewardStepCounter++;
            if (positionalRewardStepCounter >= positionalRewardStepInterval)
            {
                BallBasedPositionalReward();
                positionalRewardStepCounter = 0; // Reset counter
            }
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }

        MoveAgent(actionBuffers.DiscreteActions);
    }


    private void BallBasedPositionalReward()
    {
        // Ball and goal references
        var ball = GameObject.FindWithTag("ball");
        if (ball == null) return;

        // Define the position of the goal
        Vector3 ownGoalPosition = team == Team.Blue
            ? new Vector3(-1650, -25, -1.525f) // Blue goal
            : new Vector3(1650, -25, -1.525f); // Purple goal

        // Get the ball's position
        Vector3 ballPosition = ball.transform.position;


        // Define a defensive zone threshold (distance from the goal)
        float defensiveZoneRadius = 30.0f; // Adjust based on your field size
        // Calculate distance from ball to the goal
        float distanceBallToGoal = Vector3.Distance(ballPosition, ownGoalPosition);
        // Only reward if the ball is within the defensive zone
        if (distanceBallToGoal <= defensiveZoneRadius)
        {
            float distanceGoalieToGoal = Vector3.Distance(transform.position, ownGoalPosition);

            // Reward the goalie for being closer to the goal than the ball
            if (distanceGoalieToGoal < distanceBallToGoal)
            {
                AddReward(0.1f); // Reward for being closer
                DebugFileLogger.Log($"Goalie rewarded for being closer to the goal than the ball. Distance: {distanceGoalieToGoal} < {distanceBallToGoal}");
            }
            else
            {
                DebugFileLogger.Log("No reward. Goalie is farther from the goal than the ball but no penalty applied.");
            }
        }
        else
        {
            DebugFileLogger.Log("No positional reward. Ball is outside the defensive zone.");
        }
    }

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
            ballRb.AddForce(dirToBall * force); // Apply force

            // Define goal centers
            Vector3 opponentGoalCenter = team == Team.Blue
                ? new Vector3(1650, -25, -1.525f) // Purple goal
                : new Vector3(-1650, -25, -1.525f); // Blue goal

            Vector3 ownGoalCenter = team == Team.Blue
                ? new Vector3(-1650, -25, -1.525f) // Blue goal
                : new Vector3(1650, -25, -1.525f); // Purple goal

            // Compute directions to goals
            Vector3 ballToOpponentGoal = opponentGoalCenter - c.contacts[0].point; // Vector to opponent's goal
            ballToOpponentGoal = ballToOpponentGoal.normalized;

            Vector3 ballToOwnGoal = ownGoalCenter - c.contacts[0].point; // Vector to own goal
            ballToOwnGoal = ballToOwnGoal.normalized;

            // Check ball's velocity alignment
            Vector3 ballVelocity = ballRb.velocity.normalized;
            float alignmentWithOpponentGoal = Vector3.Dot(ballToOpponentGoal, ballVelocity);
            float alignmentWithOwnGoal = Vector3.Dot(ballToOwnGoal, ballVelocity);

            DebugFileLogger.Log($"Alignment with opponent goal direction: {alignmentWithOpponentGoal}");
            DebugFileLogger.Log($"Alignment with own goal direction: {alignmentWithOwnGoal}");

            // Reward for kicking toward opponent's goal
            if (alignmentWithOpponentGoal > 0.9f) // Threshold for alignment
            {
                AddReward(0.5f); // Reward for kicking toward opponent's goal
                DebugFileLogger.Log("Reward for kicking toward opponent's goal area.");
            }
            // Penalty for kicking toward own goal
            else if (alignmentWithOwnGoal > 0.9f) // Threshold for alignment
            {
                AddReward(-0.5f); // Penalty for kicking toward own goal
                DebugFileLogger.Log("Penalty for kicking toward own goal");
            }
            else
            {
                DebugFileLogger.Log("No penalty or reward as kick direction is neutral.");
            }

            // Emit sound event
            awarenessSystem.hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EKick, 3f);
        }
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