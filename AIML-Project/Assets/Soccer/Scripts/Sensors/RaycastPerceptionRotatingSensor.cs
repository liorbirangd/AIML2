using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class RaycastPerceptionRotatingSensor : RayPerceptionSensorComponent3D
{
    private float GlanceMinAngle = -110f;
    private float GlanceMaxAngle = 110f;


    private float glanceDuration = 0f;
    private float maxGlanceDuration = 5f;
    private float timeBetweenGlances = 1f;
    private float visionAngle = 0f;


    void Update()
    {
        HandleGlance();
    }

    private void HandleGlance()
    {
        if (timeBetweenGlances > 0f)
        {
            timeBetweenGlances -= Time.deltaTime;
            ApplyVisionAngle(0f);
            return;
        }

        if (glanceDuration <= 0f)
        {
            float glanceChance = Random.Range(0f, 1f);
            //glance at all
            if (glanceChance < 0.5f)
            {
                glanceDuration = Random.Range(2f, maxGlanceDuration);
                if (Random.Range(0f, 1f) < 0.5f)
                {
                    //glance left
                    visionAngle = Random.Range(-MaxRayDegrees, GlanceMinAngle);
                }
                else
                {
                    //glance right
                    visionAngle = Random.Range(MaxRayDegrees, GlanceMaxAngle);
                }
            }
            else
            {
                timeBetweenGlances = Random.Range(2f, 10f);
                ApplyVisionAngle(0f);
                return;
            }
        }
        if (glanceDuration > 0f)
        {
            ApplyVisionAngle(visionAngle);
            glanceDuration -= Time.deltaTime;
            if (glanceDuration <= 0f)
                timeBetweenGlances = Random.Range(2f, 10f);
        }
        else
        {
            ApplyVisionAngle(0f);
        }
    }

    private void ApplyVisionAngle(float angle)
    {
        if (glanceDuration > 0f)
        {
            //apply the glance angle to the child only (raycast direction)
            transform.localRotation = Quaternion.Euler(0, angle, 0);
        }
        else
        {
            //reset the child to align with the agent's forward direction
            transform.localRotation = Quaternion.identity;
        }
        Vector3 visionDirection = transform.forward;
    }
}