using Soccer.Scripts.Enums;

namespace Soccer.Scripts.Rewards
{
    public class BallTouchRewardComponent : RewardComponent
    {
        private const float BALL_TOUCH_REWARD = 0.05f;

        public BallTouchRewardComponent(RewardManager manager, IRewardableAgent agent) : base(manager, agent)
        {
            manager.OnBallTouched.AddListener(addReward);
        }

        public void addReward()
        {
            //AddReward(.2f * m_BallTouch);
            agent.addReward(BALL_TOUCH_REWARD);
            DebugFileLogger.Log("Agent Touch Reward.");
        }
    }
}