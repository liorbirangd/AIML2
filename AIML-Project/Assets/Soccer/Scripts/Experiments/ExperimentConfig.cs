using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ExperimentConfig : MonoBehaviour
{
    public GameObject _activator;
    public GameObject _experiment;

<<<<<<< Updated upstream
=======
    private Activator activator;
    private Experiment experiment;

>>>>>>> Stashed changes
    private string file = "Assets\\Soccer\\Scripts\\Experiments\\Configuration.txt";

    private bool useConfig;
    private bool runExperiment;
    private string[] sensorSettings = new string[10];
    private int numberOfGames;
<<<<<<< Updated upstream
=======
    private bool[] rewardsSettings = new bool[5];
>>>>>>> Stashed changes

    // Start is called before the first frame update
    void Start()
    {
        ReadConfigFile();
        TestConfig();
<<<<<<< Updated upstream
        if (useConfig)
        {
            SetSensors();
=======
        activator = _activator.GetComponent<Activator>();
        experiment = _experiment.GetComponent<Experiment>();
        if (useConfig)
        {
            SetSensors();
            SetRewards();
>>>>>>> Stashed changes
            if (runExperiment) StartExperiment();
        }
    }

    //Read the configuration.txt file
    private void ReadConfigFile()
    {
        StreamReader reader = new StreamReader(file);
        int count = 0;
        while (reader.Peek() != -1)
        {
            string line = reader.ReadLine();
            SetVariables(line, count);
            count++;
        }
    }

    //Set relevant variables according to config file
    private void SetVariables(string line, int count)
    {
        switch (count)
        {
            case 0:
                useConfig = GetConfigBool(line);
                break;
            case < 11:
                sensorSettings[count-1] = GetConfigString(line);
                break;
            case 11:
                runExperiment = GetConfigBool(line);
                break;
            case 12:
                numberOfGames = GetConfigInt(line);
                break;
<<<<<<< Updated upstream
=======
            case < 18:
                rewardsSettings[count-13] = GetConfigBool(line);
                break;
>>>>>>> Stashed changes
        }
    }

    //Takes config file line and returns a bool
    private bool GetConfigBool(string line)
    {
        return bool.Parse((line.Split('=')[1]).Split('\n')[0]);
    }

    //Takes config file line and returns an int
    private int GetConfigInt(string line)
    {
        return int.Parse((line.Split('=')[1]).Split('\n')[0]);
    }

    //Takes config file line and returns a string
    private string GetConfigString(string line)
    {
        return (line.Split('=')[1]).Split('\n')[0];
    }

    //Test configuration
    private void TestConfig()
    {
        Debug.Log(useConfig);
        for (int i = 0; i < sensorSettings.Length; i++) Debug.Log(sensorSettings[i]);
        Debug.Log(runExperiment);
        Debug.Log(numberOfGames);
    }

    private void SetSensors()
    {
<<<<<<< Updated upstream
        Activator activator = _activator.GetComponent<Activator>();
=======
        //Activator activator = _activator.GetComponent<Activator>();
>>>>>>> Stashed changes
        if (sensorSettings[0] != null) activator.ActivateAllHearingSensors(sensorSettings[0]);
        if (sensorSettings[1] != null) activator.ActivateAllAwarenessSystem(sensorSettings[1]);
        activator.ActivateBlueStriker1Sensors(sensorSettings[2], sensorSettings[3]);
        activator.ActivateBlueStriker2Sensors(sensorSettings[4], sensorSettings[5]);
        activator.ActivatePurpleStriker1Sensors(sensorSettings[6], sensorSettings[7]);
        activator.ActivatePurpleStriker2Sensors(sensorSettings[8], sensorSettings[9]);
    }

<<<<<<< Updated upstream
    private void StartExperiment()
    {
        Experiment experiment = _experiment.GetComponent<Experiment>();
=======
    private void SetRewards()
    {
        activator.ActivateRewards(rewardsSettings);
    }

    private void StartExperiment()
    {
        //Experiment experiment = _experiment.GetComponent<Experiment>();
>>>>>>> Stashed changes
        experiment.StartExperiment(numberOfGames);
    }
}
