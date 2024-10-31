using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HearingSensorController : MonoBehaviour
{
    private Observation[] observations;
    private List<Transform> collisions = new List<Transform>();
    private Transform agentTransform;

    private void Start()
    {
        observations = new Observation[4];
        for (int i = 0; i < observations.Length; i++)
        {
            observations[i] = new Observation();
        }
        agentTransform=transform.parent;
    }
    
    private void OnTriggerStay(Collider other)
    {
        
        if (other.transform == agentTransform)
            return;
        if (!collisions.Contains(other.transform))
            collisions.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == agentTransform)
            return;
        if (collisions.Contains(other.transform))
            collisions.Remove(other.transform);
    }

    public void CollectObservations(VectorSensor sensor)
    {
        DetectCollisions();
        foreach (Observation observation in observations)
        {
            sensor.AddObservation(observation.isObserved);
            sensor.AddObservation(observation.position.x);
            sensor.AddObservation(observation.position.z);
        }
    }

    private void DetectCollisions()
    {
        observations = new Observation[4];
        int i = 0;
        Debug.Log("Detecting collisions"+ collisions.Count);
        foreach (Transform t in collisions)
        {
            if (i < observations.Length)
                observations[i] = new Observation(t);
            i++;
        }

        for (; i < observations.Length; i++)
        {
            observations[i] = new Observation();
        }
    }
}

internal class Observation
{
    public bool isObserved = false;
    public Vector3 position = Vector3.zero;

    public Observation()
    {
    }

    public Observation(Transform t)
    {
        isObserved = true;
        position = t.position;
    }

    public override string ToString()
    {
        return "IsObserved: " + isObserved + " Ppsition: (" + position + ")";
    }
}