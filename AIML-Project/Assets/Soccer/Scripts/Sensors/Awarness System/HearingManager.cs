using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EHeardSoundCategory
{
    EFootstep,
    EKick,
    ECollision
}

public class HearingManager : MonoBehaviour
{

    public List<HearingSensor> AllSensors { get; private set; } = new List<HearingSensor>();
    
    public void Register(HearingSensor sensor)
    {
        AllSensors.Add(sensor);
    }

    public void Deregister(HearingSensor sensor)
    {
        AllSensors.Remove(sensor);
    }

    public void OnSoundEmitted(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        //Notify all sensors
        foreach (var sensor in AllSensors)
        {
            sensor.OnHeardSound(source, location, category, intensity);
        }
    }
}
