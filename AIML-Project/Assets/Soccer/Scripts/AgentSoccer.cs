using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

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
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        m_ResetParams = Academy.Instance.EnvironmentParameters;
        awarenessSystem = GetComponentInParent<AwarenessSystem>();
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {
        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }

        MoveAgent(actionBuffers.DiscreteActions);
    }

<<<<<<< Updated upstream
=======
    public void SetRewardsSettings(bool[] rewardsSettings)
    {

    }

    private void BallBasedPositionalReward()
    {
        // Ball and goal references
        var ball = GameObject.FindWithTag("ball");
        if (ball == null) return;

        // Define the position of the goal
        GameObject ownGoal = team == Team.Blue
            ? envController.blueGoal
            : envController.purpleGoal;

        // Get the ball's position
        Vector3 ballPosition = ball.transform.position;


        // Define a defensive zone threshold (distance from the goal)
        float defensiveZoneRadius = 15.5f; // Adjust based on your field size
        // Calculate distance from ball to the goal
        float distanceBallToGoal = Vector3.Distance(ballPosition, ownGoal.transform.position);
        DebugFileLogger.Log($"Ball to Goal Distance: {distanceBallToGoal} < 15.5");
        // Only reward if the ball is within the defensive zone
        if (distanceBallToGoal <= defensiveZoneRadius)
        {
            float distanceGoalieToGoal = Vector3.Distance(transform.position, ownGoal.transform.position);

            // Reward the goalie for being closer to the goal than the ball
            if (distanceGoalieToGoal < distanceBallToGoal)
            {
                AddReward(0.05f); // Reward for being closer
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

>>>>>>> Stashed changes
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
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
            awarenessSystem.hearingManager.OnSoundEmitted(gameObject, transform.position, EHeardSoundCategory.EKick,
                3f);
        }
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

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);


        MakeSound();
    }

    private void MakeSound()
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