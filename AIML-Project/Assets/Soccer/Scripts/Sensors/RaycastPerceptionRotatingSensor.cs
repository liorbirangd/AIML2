using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RaycastPerceptionRotatingSensor : RayPerceptionSensorComponent3D
{
    private float glanceMinAngle = -110f;
    private float glanceMaxAngle = 110f;

    private float glanceDuration = 0f;
    private float maxGlanceDuration = 5f;
    private float midGlanceDuration = 2f;
    private float minGlanceDuration = 0f;

    private float visionAngle = 0f;

    private float timeBetweenGlances = 1f;
    private float minTimeBetweenGlances = 0f;
    private float midTimeBetweenGlances = 2f;
    private float maxTimeBetweenGlances = 10f;

    private float minRandomParameter = 0f;
    private float maxRandomParameter = 1f;
    private float halfRandomParameter = 0.5f;

    private float Axis = 0f;

    void Update()
    {
        HandleGlance();
    }

    private void HandleGlance()
    {
        if (timeBetweenGlances > minTimeBetweenGlances)
        {
            timeBetweenGlances -= Time.deltaTime;
            ApplyVisionAngle(visionAngle);
            return;
        }

        if (glanceDuration <= minGlanceDuration)
        {
            float glanceChance = Random.Range(minRandomParameter, maxRandomParameter);
            //glance at all
            if (glanceChance < halfRandomParameter)
            {
                glanceDuration = Random.Range(midGlanceDuration, maxGlanceDuration);
                if (Random.Range(minRandomParameter, maxRandomParameter) < halfRandomParameter)
                {
                    //glance left
                    visionAngle = Random.Range(-MaxRayDegrees, glanceMinAngle);
                }
                else
                {
                    //glance right
                    visionAngle = Random.Range(MaxRayDegrees, glanceMaxAngle);
                }
            }
            else
            {
                timeBetweenGlances = Random.Range(midTimeBetweenGlances, maxTimeBetweenGlances);
                ApplyVisionAngle(visionAngle);
                return;
            }
        }
        if (glanceDuration > minGlanceDuration)
        {
            ApplyVisionAngle(visionAngle);
            glanceDuration -= Time.deltaTime;
            if (glanceDuration <= minGlanceDuration)
                timeBetweenGlances = Random.Range(midTimeBetweenGlances, maxTimeBetweenGlances);
        }
        else
        {
            ApplyVisionAngle(visionAngle);
        }
    }

    private void ApplyVisionAngle(float angle)
    {
        if (glanceDuration > minGlanceDuration)
        {
            //apply the glance angle to the child only (raycast direction)
            transform.localRotation = Quaternion.Euler(Axis, angle, Axis);
        }
        else
        {
            //reset the child to align with the agent's forward direction
            transform.localRotation = Quaternion.identity;
        }
        Vector3 visionDirection = transform.forward;
    }
}