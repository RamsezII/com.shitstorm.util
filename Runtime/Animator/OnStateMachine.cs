using UnityEngine;

namespace _UTIL_
{
    public interface IOnStateMachine
    {
        void OnStateMachine(in AnimatorStateInfo stateInfo, in int layerIndex, in bool onEnter);
    }

    public class OnStateMachine : StateMachineBehaviour
    {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) => OnState(animator, stateInfo, layerIndex, true);
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) => OnState(animator, stateInfo, layerIndex, false);
        void OnState(in Animator animator, in AnimatorStateInfo stateInfo, in int layerIndex, in bool onEnter)
        {
            IOnStateMachine user = animator.GetComponentInParent<IOnStateMachine>(true);
            if (user == null)
                Debug.LogWarning($"{typeof(IOnStateMachine).FullName} not found on: {animator.transform.GetPath(true)}");
            else
                user.OnStateMachine(stateInfo, layerIndex, onEnter);
        }
    }
}