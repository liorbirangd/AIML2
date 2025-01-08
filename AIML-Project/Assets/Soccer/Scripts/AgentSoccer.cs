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
using Soccer.Scripts.Enums;
using Soccer.Scripts.Rewards;

public class AgentSoccer : Agent, IRewardableAgent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player

    private RewardManager rewardManager;
    private List<RewardComponent> rewardComponents;
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

    private int positionalRewardStepCounter = 0; // Step counter for positional rewards
    private const int positionalRewardStepInterval = 100; // Execute every 100 steps

    public override void Initialize()
    {
        envController = GetComponentInParent<SoccerEnvController>();
        rewardManager = new RewardManager();
        if (envController == null) throw new Exception("SoccerEnvController not found");
        InitializeRewardComponents();

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

    private void InitializeRewardComponents()
    {
        rewardComponents = new List<RewardComponent>();
        rewardComponents.Add(new ExistantialRewardComponent(rewardManager, this, envController));
        rewardComponents.Add(new BallPositionRewardComponent(rewardManager, this, envController));
        rewardComponents.Add(new BallTouchRewardComponent(rewardManager, this));
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Existential bonus for Goalies.
        //AddReward(m_Existential);
        rewardManager.OnActionedPerformed.Invoke(position);
        if (position == Position.Goalie)
        {
            // Execute the BallBasedPositionalReward only at specific intervals
            positionalRewardStepCounter++;
            if (positionalRewardStepCounter >= positionalRewardStepInterval)
            {
                rewardManager.OnBallPositioningCheck.Invoke(team, transform.position);
                positionalRewardStepCounter = 0; // Reset counter
            }
        }

        MoveAgent(actionBuffers.DiscreteActions);
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
            //AddReward(.2f * m_BallTouch);
            //DebugFileLogger.Log("Agent Touch Reward.");
            rewardManager.OnBallTouched.Invoke();

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
            awarenessSystem.hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EKick,
                3f);
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
        bool wasHeadingToGoal =
            IsBallHeadingTowardsGoal(preContactVelocity, ball.transform.position, ownGoalTag, rayDistance);
        DebugFileLogger.Log($"so wasHeadingTowardGoal: {wasHeadingToGoal}");

        if (wasHeadingToGoal)
        {
            DebugFileLogger.Log($"After contact, the ball: ");
            Vector3 postContactVelocity = ballRb.velocity.normalized; // Get updated velocity
            bool isHeadingAwayFromGoal =
                !IsBallHeadingTowardsGoal(postContactVelocity, ball.transform.position, ownGoalTag, rayDistance);
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

    private bool IsBallHeadingTowardsGoal(Vector3 ballDirection, Vector3 ballPosition, String goalTag,
        float rayDistance)
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
        if (awarenessSystem != null)
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

    public void addReward(float value)
    {
        AddReward(value);
    }
}