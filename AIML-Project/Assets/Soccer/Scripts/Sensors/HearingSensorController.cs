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

    private int observNmbr = 4;
    private float minVelMag = 0.01f;

    private String tagBall = "ball";
    private String tagBA = "blueAgent";
    private String tagPA = "purpleAgent";

    //Create list of observations
    private void Start()
    {
        observations = new Observation[observNmbr];
        for (int i = 0; i < observations.Length; i++)
        {
            observations[i] = new Observation();
        }
        agentTransform=transform.parent;
    }
    
    //When something collides with the object, add to list of collisions
    private void OnTriggerStay(Collider other)
    {
        if(!(other.tag==tagBall || other.tag==tagBA || other.tag==tagPA))
            return;
        if (other.transform == agentTransform)
            return;
        if (!collisions.Contains(other.transform))
            collisions.Add(other.transform);
    }

    //When no longer colliding, remove from list of collisions
    private void OnTriggerExit(Collider other)
    {
        if(!(other.tag==tagBall || other.tag==tagBA || other.tag==tagPA))
            return;
        if (other.transform == agentTransform)
            return;
        if (collisions.Contains(other.transform))
            collisions.Remove(other.transform);
    }

    //Adds all observations to a sensor
    public void AddObservations(VectorSensor sensor)
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
        observations = new Observation[observNmbr];
        int i = 0;
        foreach (Transform t in collisions)
        {
            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb == null || rb.velocity.magnitude<minVelMag) continue;
            if (i < observations.Length)
                observations[i] = new Observation(transform,t);
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
    public const float MAX_RADIUS = 1;
    public bool isObserved = false;
    public Vector3 position = Vector3.zero;
    public float radius = 0;

    public Observation()
    {
    }

    public Observation(Transform agent, Transform observed)
    {
        isObserved = true;
        float distance = Vector3.Distance(agent.position, observed.position);
        CalcRadius(distance);
        float randomX = UnityEngine.Random.Range(-radius, radius);
        float randomZ = UnityEngine.Random.Range(-radius, radius);
        position = new Vector3(observed.position.x + randomX, observed.position.y, observed.position.z + randomZ);   
    }

    private float CalcRadius(float distance)
    {
        //y=\frac{1}{1+e^{-\left(x-c\right)}}\cdot0.6 (desmos)
        //radius = 1/(1+ e^(2.5-distance))*0.6 * max_radius
        return radius = MAX_RADIUS * 0.6f * (float)(1 / (1 + Math.Pow(Math.E, (2.5 - distance))));
    }

    public override string ToString()
    {
        return "IsObserved: " + isObserved + " Position: (" + position + ")";
    }
}