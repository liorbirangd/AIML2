using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment : MonoBehaviour
{
    public GameObject ball;
    private SoccerBallController ballController;

    public GameObject field;

    private int numberOfGames;
    private int gamesPlayed;
    private int blueTeamGoals;
    private int purpleTeamGoals;

    public void StartExperiment(int numberOfGames)
    {
        ballController = ball.GetComponent<SoccerBallController>();
        ballController.SetExperiment(this);
        this.numberOfGames = numberOfGames;
        gamesPlayed = 0;
        blueTeamGoals = 0;
        purpleTeamGoals = 0;
    }

    public void BlueTeamScored()
    {
        gamesPlayed++;
        blueTeamGoals++;
        if (gamesPlayed == numberOfGames) EndExperiment();
    }

    public void PurpleTeamScored()
    {
        gamesPlayed++;
        purpleTeamGoals++;
        if (gamesPlayed == numberOfGames) EndExperiment();
    }

    private void EndExperiment()
    {
        Debug.Log("Experiment Results\nGames Played: " + gamesPlayed + "\nBlue Team Score: " + blueTeamGoals + "\nPurple Team Score: " + purpleTeamGoals);
        field.SetActive(false);
    }
}
