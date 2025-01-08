using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;
using System;
using static UnityEngine.GraphicsBuffer;

public class Dummy : MonoBehaviour
{
    public GameObject ball;
    private Rigidbody ballRb;
    public GameObject blueGoal;


    // Start is called before the first frame update
    void Start()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        //ballRb.velocity = (ball.transform.position - blueGoal.transform.position) *0.1f;
        ballRb.velocity =(blueGoal.transform.position - ball.transform.position)*500;
        Debug.Log(ballRb.velocity);

        Debug.DrawRay(ball.transform.position, ballRb.velocity.normalized * 7000f, Color.red, 5f);

        bool check = IsBallHeadingTowardsGoal(ballRb.velocity, ball.transform.position, "blueGoal", 7000f);
        Debug.Log(check);
    }

    private bool IsBallHeadingTowardsGoal(Vector3 ballDirection, Vector3 ballPosition, string goalTag, float rayDistance)
    {
        DebugFileLogger.Log("Checking...");
        LayerMask targetLayer;
        targetLayer = LayerMask.GetMask("Goal");

        // Perform a raycast in the direction of the velocity
        Ray ray = new Ray(ballPosition, ballDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayer))
        {
            Debug.Log($"Ray Hit: {hit.transform.name}, Tag: {hit.transform.tag}");
            // Optional: Check if the ray hits the target
            if (hit.transform.CompareTag(goalTag))
            {
                DebugFileLogger.Log("was moving towards goal");
                return true; // Moving towards and ray hits target
            }
        }
        DebugFileLogger.Log("was not moving towards goal");
        return false; // Moving towards based on direction
    }
}
