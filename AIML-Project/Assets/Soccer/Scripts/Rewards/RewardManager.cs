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
        public UnityEvent<Vector3,Rigidbody,GameObject> OnKickDirectionCheck;

        public RewardManager()
        {
            OnActionedPerformed = new UnityEvent<Position>();
            OnBallPositioningCheck = new UnityEvent<Team,Vector3>();
            OnBallTouched = new UnityEvent();
            OnKickDirectionCheck = new UnityEvent<Vector3,Rigidbody,GameObject>();
        }
    }
}