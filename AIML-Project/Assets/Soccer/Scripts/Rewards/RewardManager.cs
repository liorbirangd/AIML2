using Soccer.Scripts.Enums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Soccer.Scripts.Rewards
{
    public class RewardManager
    {
        public UnityEvent<Position> OnActionedPerformed;
        public UnityEvent<Team,Vector3> OnBallPositioningCheck;
        public UnityEvent OnBallTouched;
    }
}