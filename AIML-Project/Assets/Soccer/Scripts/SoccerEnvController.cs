using System.Collections.Generic;
using System.IO;
using Unity.MLAgents;
using UnityEngine;

public class SoccerEnvController : MonoBehaviour
{

    private int learningTeamGoals = 0;
    private int opposingTeamGoals = 0;
    private string currentLearningTeam = "Blue";

    //private StreamWriter logFile;

    [System.Serializable]
    public class PlayerInfo
    {
        public AgentSoccer Agent;
        [HideInInspector]
        public Vector3 StartingPos;
        [HideInInspector]
        public Quaternion StartingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }


    /// <summary>
    /// Max Academy steps before this platform resets
    /// </summary>
    /// <returns></returns>
    [Tooltip("Max Environment Steps")] public int MaxEnvironmentSteps = 25000;

    /// <summary>
    /// The area bounds.
    /// </summary>

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>

    public GameObject ball;
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 m_BallStartingPos;

    public GameObject purpleGoal;
    public GameObject blueGoal;

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private int m_ResetTimer;

    void Start()
    {

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManager
        m_BlueAgentGroup = new SimpleMultiAgentGroup();
        m_PurpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        m_BallStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
        foreach (var item in AgentsList)
        {
            item.StartingPos = item.Agent.transform.position;
            item.StartingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.team == Team.Blue)
            {
                m_BlueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                m_PurpleAgentGroup.RegisterAgent(item.Agent);
            }
        }
        ResetScene();
    }

    void FixedUpdate()
    {
        m_ResetTimer += 1;
        if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            m_BlueAgentGroup.GroupEpisodeInterrupted();
            m_PurpleAgentGroup.GroupEpisodeInterrupted();
            ResetScene();
        }
    }

   

    public void LogScores(int currentStep, string currentLearningTeam)
    {
        string logEntry = $"Step {currentStep}: LearningTeam ({currentLearningTeam}) Goals = {learningTeamGoals}, Opponent Goals = {opposingTeamGoals}";

        // Write to file
        DebugScoreFileLogger.Log(logEntry);
    }

    public void ClearGoals() 
    {
        learningTeamGoals = 0;
        opposingTeamGoals = 0;
    }

    public void SetCurrentLearningTeam(string learningTeam)
    {
        currentLearningTeam = learningTeam;
    }


    public void ResetBall()
    {
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = m_BallStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }


    public void GoalTouched(Team scoredTeam)
    {
        DebugFileLogger.Log($"Goal scored by {scoredTeam} team!");
        
        // Increment the appropriate counter based on the current learning team
        if (scoredTeam.ToString() ==  currentLearningTeam)
        {
            learningTeamGoals++;
        }
        else
        {
            opposingTeamGoals++;
        }

        var ballController = ball.GetComponent<SoccerBallController>();
        if (scoredTeam == Team.Blue)
        {
            // Reward Blue Team
            m_BlueAgentGroup.AddGroupReward(2.0f); // Group reward for scoring
            m_PurpleAgentGroup.AddGroupReward(-1.0f); // Penalty for the opposing team
            DebugFileLogger.Log("Blue team rewarded.");
            //DebugFileLogger.Log($"{ballController.lastToucher.GetComponent<AgentSoccer>().name} scored!");

            // Reward the scoring agent
            if (ballController.lastToucher != null)
            {
                var scorerAgent = ballController.lastToucher.GetComponent<AgentSoccer>();
                DebugFileLogger.Log($"{scorerAgent.name} scored!");

                // Ensure the scorer is from the blue team
                if (scorerAgent != null)
                {
                    if (scorerAgent.team == Team.Blue)
                    {
                        scorerAgent.AddReward(0.2f); // Scoring Reward
                        DebugFileLogger.Log($"{scorerAgent.name} rewarded for scoring!");
                    }
                    else if (scorerAgent.team == Team.Purple) {
                        scorerAgent.AddReward(-0.2f); // Scoring Penalty
                        DebugFileLogger.Log($"{scorerAgent.name} penalized for scoring!");
                    }
                }
            }
        }
        else
        {
            // Reward Purple Team
            m_PurpleAgentGroup.AddGroupReward(2.0f); // Group reward for scoring
            m_BlueAgentGroup.AddGroupReward(-1.0f); // Penalty for the opposing team
            DebugFileLogger.Log("Purple team rewarded.");
            //DebugFileLogger.Log($"{ballController.lastToucher.GetComponent<AgentSoccer>().name} scored!");

            // Reward the scoring agent
            if (ballController.lastToucher != null)
            {
                var scorerAgent = ballController.lastToucher.GetComponent<AgentSoccer>();
                DebugFileLogger.Log($"{scorerAgent.name} scored!");

                // Ensure the scorer is from the purple team
                if (scorerAgent != null)
                {
                    if (scorerAgent.team == Team.Purple)
                    {
                        scorerAgent.AddReward(0.2f); // Scoring Reward
                        DebugFileLogger.Log($"{scorerAgent.name} rewarded for scoring!");
                    }
                    else if (scorerAgent.team == Team.Blue)
                    {
                        scorerAgent.AddReward(-0.2f); // Scoring Penalty
                        DebugFileLogger.Log($"{scorerAgent.name} penalized for scoring!");
                    }
                }
            }
        }

        // End the episode and reset the scene
        m_PurpleAgentGroup.EndGroupEpisode();
        m_BlueAgentGroup.EndGroupEpisode();
        ResetScene();
    }


    public void ResetScene()
    {
        m_ResetTimer = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Ball
        ResetBall();
    }
}
