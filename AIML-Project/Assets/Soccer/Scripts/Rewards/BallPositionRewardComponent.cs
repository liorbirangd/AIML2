using UnityEngine;
using Soccer.Scripts.Enums;

namespace Soccer.Scripts.Rewards
{
    public class BallPositionRewardComponent : RewardComponent
    {
        private const float BALL_POSITIONAL_REWARD = 0.05f;
        private SoccerEnvController envController;

        public BallPositionRewardComponent(RewardManager manager, IRewardableAgent agent,
            SoccerEnvController envController) : base(manager, agent)
        {
            this.envController = envController;
            manager.OnBallPositioningCheck.AddListener(addReward);
        }

        public void addReward(Team team, Vector3 position)
        {
            // Ball and goal references
            GameObject ball = envController.ball;
            if (ball == null) return;

            // Define the position of the goal
            GameObject ownGoal = team == Team.Blue
                ? envController.blueGoal
                : envController.purpleGoal;

            // Get the ball's position
            Vector3 ballPosition = ball.transform.position;


            // Define a defensive zone threshold (distance from the goal)
            float defensiveZoneRadius = 15.5f; // Adjust based on your field size
            // Calculate distance from ball to the goal
            float distanceBallToGoal = Vector3.Distance(ballPosition, ownGoal.transform.position);
            DebugFileLogger.Log($"Ball to Goal Distance: {distanceBallToGoal} < 15.5");
            // Only reward if the ball is within the defensive zone
            if (distanceBallToGoal <= defensiveZoneRadius)
            {
                float distanceGoalieToGoal = Vector3.Distance(position, ownGoal.transform.position);

                // Reward the goalie for being closer to the goal than the ball
                if (distanceGoalieToGoal < distanceBallToGoal)
                {
                    agent.addReward(BALL_POSITIONAL_REWARD); // Reward for being closer
                    DebugFileLogger.Log(
                        $"Goalie rewarded for being closer to the goal than the ball. Distance: {distanceGoalieToGoal} < {distanceBallToGoal}");
                }
                else
                {
                    DebugFileLogger.Log(
                        "No reward. Goalie is farther from the goal than the ball but no penalty applied.");
                }
            }
            else
            {
                DebugFileLogger.Log("No positional reward. Ball is outside the defensive zone.");
            }
        }
    }
}