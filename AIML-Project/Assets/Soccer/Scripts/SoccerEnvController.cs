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

    private int lastLoggedStep= 0;
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

    public void LogScores(int currentStep, string currentLearningTeam)
    {
        string logEntry = $"Step {currentStep}: LearningTeam ({currentLearningTeam}) Goals = {learningTeamGoals}, Opponent Goals = {opposingTeamGoals}";

        // Write to file
        DebugFileLogger.Log(logEntry);
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

    // void FixedUpdate()
    // {
    //     m_ResetTimer += 1;
    //     int currentStep = Academy.Instance.StepCount;

    //     if (currentStep % 1000 == 0 && currentStep > 0 && currentStep != lastLoggedStep)
    // {
    //     LogScores(currentStep, currentLearningTeam);
    //     lastLoggedStep = currentStep;
    //     currentLearningTeam = (currentLearningTeam == "Blue") ? "Purple" : "Blue";
    //     ClearGoals();
    // }

    // //     if (m_ResetTimer % 1000 == 0 && m_ResetTimer > 0)
    // // {
    // //     //int stepToLog = (currentGlobalStep / 1000) * 1000;
    // //     LogScores(m_ResetTimer, currentLearningTeam);
    // //     //LogScores(stepToLog, currentLearningTeam);
        
    // //     // Switch teams
    // //     currentLearningTeam = (currentLearningTeam == "Blue") ? "Purple" : "Blue";
        
    // //     // Reset counters for the new phase
    // //     ClearGoals();
    // // }
    
    //     if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
    //     {
    //         m_BlueAgentGroup.GroupEpisodeInterrupted();
    //         m_PurpleAgentGroup.GroupEpisodeInterrupted();
    //         ResetScene();
    //     }
    // }

    void FixedUpdate()
{
    m_ResetTimer += 1;
    // int currentStep = Academy.Instance.StepCount;

    // // Log scores every 1000 steps
    // if (currentStep >= lastLoggedStep + 1000)
    // {
    //     LogScores(currentStep, currentLearningTeam);
    //     lastLoggedStep = currentStep;

    //     // Optionally switch teams every 1000 steps
    //     currentLearningTeam = (currentLearningTeam == "Blue") ? "Purple" : "Blue";
    //     ClearGoals();
    // }

    // Reset the environment if the max steps are reached
    if (m_ResetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
    {
        m_BlueAgentGroup.GroupEpisodeInterrupted();
        m_PurpleAgentGroup.GroupEpisodeInterrupted();
        ResetScene();
    }
}

//     void FixedUpdate()
// {
//     int currentGlobalStep = Academy.Instance.StepCount;
    
//     if (currentGlobalStep >= lastLoggedStep + 1000)
//     {
//         int stepToLog = (currentGlobalStep / 1000) * 1000;
//         LogScores(stepToLog, currentLearningTeam);
        
//         // Switch teams
//         currentLearningTeam = (currentLearningTeam == "Blue") ? "Purple" : "Blue";
        
//         // Reset counters for the new phase
//         ClearGoals();
        
//         lastLoggedStep = stepToLog;
//     }
    
//     if (currentGlobalStep >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
//     {
//         m_BlueAgentGroup.GroupEpisodeInterrupted();
//         m_PurpleAgentGroup.GroupEpisodeInterrupted();
//         ResetScene();
//     }
// }

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

        // Increment the appropriate counter based on the current learning team
        if (scoredTeam.ToString() ==  currentLearningTeam)
        {
            learningTeamGoals++;
        }
        else
        {
            opposingTeamGoals++;
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
