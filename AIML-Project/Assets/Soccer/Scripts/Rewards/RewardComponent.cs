using UnityEngine;

namespace Soccer.Scripts.Rewards
{
    public abstract class RewardComponent
    {
        protected RewardManager manager;
        protected IRewardableAgent agent;

        public RewardComponent(RewardManager manager, IRewardableAgent agent)
        {
            this.manager = manager;
            this.agent = agent;
        }

        protected bool IsBallHeadingTowardsGoal(Vector3 ballDirection, Vector3 ballPosition, string goalTag,
            float rayDistance)
        {
            LayerMask targetLayer;
            targetLayer = LayerMask.GetMask("Goal");

            // Perform a raycast in the direction of the velocity
            Ray ray = new Ray(ballPosition, ballDirection);
            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, targetLayer))
            {
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
}