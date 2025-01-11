using System.IO;
using UnityEngine;

public static class GoalLogger
{
    private static string logFilePath = "goals.csv";

    //Step,LearningTeamGoals,OpponentGoals
    
    public static void Log(int step, int learningTeamGoals, int opponentGoals,string currentLearningTeam)
    {
        
        string csvRow = $"{step},{learningTeamGoals},{opponentGoals},{currentLearningTeam}";

        //Debug.Log(csvRow); // Logs to Unity Console
        File.AppendAllText(logFilePath, csvRow + "\n"); // Writes to the CSV file
    }
}
