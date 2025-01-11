using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class SoccerEnvController : MonoBehaviour
{
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

    //List of Agents On Platform
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings m_SoccerSettings;


    private SimpleMultiAgentGroup m_BlueAgentGroup;
    private SimpleMultiAgentGroup m_PurpleAgentGroup;

    private int m_ResetTimer;

    private int learningTeamGoals = 0;
    private int opposingTeamGoals = 0;
    private string currentLearningTeam = "Blue";
    //private bool hasLoggedThisInterval = false;
    private int lastLoggedStep = 0;

     private int teamChangeSteps = 10000; // Matches the `team_change` parameter in config

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

    // public void LogScores(int currentStep, string currentLearningTeam)
    // {
    //     //string logEntry = $"Step {currentStep}: LearningTeam ({currentLearningTeam}) Goals = {learningTeamGoals}, Opponent Goals = {opposingTeamGoals}";

    //     // Write to file
    //     //DebugFileLogger.Log(logEntry);
    //     GoalLogger.Log(currentStep, learningTeamGoals, opposingTeamGoals,currentLearningTeam);
    //     //Debug.Log($"{currentStep},{learningTeamGoals},{opposingTeamGoals}");
    //     Debug.Log($"Interval complete - Learning team {currentLearningTeam}: {learningTeamGoals} goals, Opposing: {opposingTeamGoals} goals");
    // }

    public void ClearGoals() 
    {
        learningTeamGoals = 0;
        opposingTeamGoals = 0;
    }

    public void SetCurrentLearningTeam(string learningTeam)
    {
        currentLearningTeam = learningTeam;
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

    public void CheckAndLogScores(int currentStep)
    {

        if (currentStep % teamChangeSteps == 0 && 
            currentStep > 0 && 
            currentStep != lastLoggedStep)
        {
            Debug.Log($"=== Logging scores at step {currentStep} ===");
            
            // Log the results
            GoalLogger.Log(currentStep, learningTeamGoals, opposingTeamGoals, currentLearningTeam);
            
            // Alternate the learning team
            currentLearningTeam = (currentLearningTeam == "Blue") ? "Purple" : "Blue";
            
            // Reset counters for the new phase
            learningTeamGoals = 0;
            opposingTeamGoals = 0;
            
            // Mark this interval as logged
            lastLoggedStep = currentStep;
            
            Debug.Log($"=== Switched to team {currentLearningTeam} for next interval ===");
        }
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

        //GoalLogger.Log($"Goal scored by {scoredTeam} team!");
        
        // Increment the appropriate counter based on the current learning team
        if (scoredTeam.ToString() ==  currentLearningTeam)
        {
            learningTeamGoals++;
            //Debug.Log($"Learning team goal! Total: {learningTeamGoals}");
        }
        else
        {
            opposingTeamGoals++;
            //Debug.Log($"Opposing team goal! Total: {opposingTeamGoals}");
        }

        if (scoredTeam == Team.Blue)
        {
            m_BlueAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_PurpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            m_PurpleAgentGroup.AddGroupReward(1 - (float)m_ResetTimer / MaxEnvironmentSteps);
            m_BlueAgentGroup.AddGroupReward(-1);
        }
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
