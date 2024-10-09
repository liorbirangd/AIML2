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

    [HideInInspector]
    public Team team;
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;

    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

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
            //goalies are usually slower, hence I lowered their speed.
            m_LateralSpeed = 0.8f;
            m_ForwardSpeed = 0.8f;
        }
        else if (position == Position.Striker)
        {
            //it's really important for strikers to be able to shift side to side rapidly to find openings,
            //so i enhanced their lateral speed
            m_LateralSpeed = 0.5f;
            //strikers are usually faster moving forward, hence the increase here
            m_ForwardSpeed = 1.5f;
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
    }

    private float timeSinceLastTouch = 0f;
    private float touchThreshold = 3.0f;  //3 seconds

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {
        timeSinceLastTouch += Time.deltaTime; //keeps track of time

        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);

            //reward for when the golie blocks the ball
            if (BlockedBall())
            {
                AddReward(0.5f);  // Reward for blocking the ball
            }
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);

            //penalty for missing the ball
            if (MissedBall())
            {
                AddReward(-0.3f);
            }

            //reward for touching the ball
            if (TouchedBall())
            {
                //resets the timer when the ball is touched
                timeSinceLastTouch = 0f;  
                AddReward(0.2f);
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

    private bool touchedBall = false;

    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            touchedBall = true; //true when the ball is touched
            AddReward(.2f * m_BallTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    private bool TouchedBall()
    {
        //reset to false after every check
        bool wasTouched = touchedBall;
        touchedBall = false;
        return wasTouched;
    }

    private bool BlockedBall()
    {
        GameObject ball = GameObject.FindWithTag("ball");
        if (ball != null)
        {
            //ball's position & velocity
            Vector3 ballPos = ball.transform.position;
            Vector3 ballVelocity = ball.GetComponent<Rigidbody>().velocity;

            //goalie's position
            Vector3 agentPos = transform.position;

            //checks if the ball is coming toward the goalie's goal
            bool isBallApproaching = Vector3.Dot((ballPos - agentPos).normalized, ballVelocity.normalized) < 0;

            //checks if the ball is within blocking distance
            float distance = Vector3.Distance(agentPos, ballPos);
            if (isBallApproaching && distance < 2.5f) //2.5f = the block distance threshold
            {
                //block detected
                return true;
            }
        }
        return false;
    }

    private bool MissedBall()
    {
        GameObject ball = GameObject.FindWithTag("ball");
        if (ball != null)
        {
            //if agent close to the ball
            float distanceToBall = Vector3.Distance(transform.position, ball.transform.position);

            if (distanceToBall < 3.0f && timeSinceLastTouch > touchThreshold)
            {
                return true;  //ball missed
            }
        }
        return false;
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
    }

}
