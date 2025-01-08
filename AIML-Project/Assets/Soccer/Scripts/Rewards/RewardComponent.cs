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
    }
}