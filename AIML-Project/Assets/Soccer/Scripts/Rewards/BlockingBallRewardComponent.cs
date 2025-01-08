using System.Collections;
using Soccer.Scripts.Enums;
using UnityEngine;

namespace Soccer.Scripts.Rewards
{
    public class BlockingBallRewardComponent : RewardComponent
    {
        private const float BLOCK_REWARD = 0.5f;
        private Team team;

        public BlockingBallRewardComponent(RewardManager manager, IRewardableAgent agent,
            Team team) : base(manager, agent)
        {
            this.team = team;
            manager.OnKickDirectionCheck.AddListener(addReward);
        }

        private void addReward(Vector3 preContactVelocity, Rigidbody ballRb, GameObject ball)
        {
            // Determine goal tags
            string ownGoalTag = team == Team.Blue ? "blueGoal" : "purpleGoal";
            string opponentGoalTag = team == Team.Blue ? "purpleGoal" : "blueGoal";
            float rayDistance = 40f;

            // Check if the ball was heading toward the goal before contact
            DebugFileLogger.Log($"       Block Check...");
            DebugFileLogger.Log($"Before contact, the ball: ");
            bool wasHeadingToGoal =
                IsBallHeadingTowardsGoal(preContactVelocity, ball.transform.position, ownGoalTag, rayDistance);
            DebugFileLogger.Log($"so wasHeadingTowardGoal: {wasHeadingToGoal}");

            if (wasHeadingToGoal)
            {
                DebugFileLogger.Log($"After contact, the ball: ");
                Vector3 postContactVelocity = ballRb.velocity.normalized; // Get updated velocity
                bool isHeadingAwayFromGoal =
                    !IsBallHeadingTowardsGoal(postContactVelocity, ball.transform.position, ownGoalTag, rayDistance);
                DebugFileLogger.Log($"so isHeadingAwayFromGoal: {isHeadingAwayFromGoal}");

                // Reward for blocking a potential goal
                if (isHeadingAwayFromGoal)
                {
                    agent.addReward(BLOCK_REWARD); // Reward for blocking
                    DebugFileLogger.Log("Reward for blocking a potential goal.");
                }
                else
                {
                    DebugFileLogger.Log("Ball wasn't blocked successfully.");
                }
            }
            else
            {
                DebugFileLogger.Log("Since false, no further check needed.");
            }
        }
    }
}