namespace Soccer.Scripts.Rewards
{
    public class ExistantialRewardComponent:RewardComponent
    {
        private float existentialValue;
        RewardManager manager;
        public ExistantialRewardComponent(RewardManager manager, SoccerEnvController envController)
        {
            this.manager = manager;
            existentialValue= 1f / envController.MaxEnvironmentSteps;
        }
        public void addReward()
        {
            throw new System.NotImplementedException();
        }
    }
}