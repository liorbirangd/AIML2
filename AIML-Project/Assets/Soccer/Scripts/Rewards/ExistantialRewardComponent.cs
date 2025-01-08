using Soccer.Scripts.Enums;

namespace Soccer.Scripts.Rewards
{
    public class ExistantialRewardComponent : RewardComponent
    {
        private float existentialValue;

        public ExistantialRewardComponent(RewardManager manager, IRewardableAgent agent,
            SoccerEnvController envController) : base(manager, agent)
        {
            existentialValue = 1f / envController.MaxEnvironmentSteps;
            manager.OnActionedPerformed.AddListener(addReward);
        }

        public void addReward(Position position)
        {
            float reward;
            switch (position)
            {
                case Position.Striker:
                    reward = existentialValue;
                    break;
                case Position.Goalie:
                    reward = -existentialValue;
                    break;
                default:
                    reward = 0;
                    break;
            }
            agent.addReward(reward);
        }
    }
}