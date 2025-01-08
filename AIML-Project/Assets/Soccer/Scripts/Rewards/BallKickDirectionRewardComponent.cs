using System.Collections;
using Soccer.Scripts.Enums;
using UnityEngine;

namespace Soccer.Scripts.Rewards
{
    public class BallKickDirectionRewardComponent : RewardComponent
    {
        private const float BLOCK_REWARD = 0.5f;
        private Team team;

        public BallKickDirectionRewardComponent(RewardManager manager, IRewardableAgent agent, Team team) : base(
            manager, agent)
        {
            this.team = team;
            manager.OnKickDirectionCheck.AddListener(addReward);
        }

        private void addReward(Vector3 preContactVelocity, Rigidbody ballRb, GameObject ball)
        {
            DebugFileLogger.Log($"       Kick Direction Check...");
            // Determine goal tags
            string ownGoalTag = team == Team.Blue ? "blueGoal" : "purpleGoal";
            string opponentGoalTag = team == Team.Blue ? "purpleGoal" : "blueGoal";
            float rayDistance = 40f;
            // Check if the ball is heading toward the opponent's goal
            Vector3 postContactVelocityCheck = ballRb.velocity.normalized; // Recheck velocity
            DebugFileLogger.Log($"After Kicking the ball: ");
            if (IsBallHeadingTowardsGoal(postContactVelocityCheck, ball.transform.position, opponentGoalTag,
                    rayDistance))
            {
                agent.addReward(0.2f); // Reward for kicking toward opponent's goal
                DebugFileLogger.Log("Reward for kicking toward opponent's goal area.");
            }
            else if (IsBallHeadingTowardsGoal(postContactVelocityCheck, ball.transform.position, ownGoalTag, 25f))
            {
                agent.addReward(-0.2f); // Penalty for kicking toward own goal
                DebugFileLogger.Log("Penalty for kicking toward own goal.");
            }
            else
            {
                DebugFileLogger.Log("No penalty or reward as kick direction is neutral.");
            }
        }
    }
}