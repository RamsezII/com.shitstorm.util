using UnityEngine;

namespace _UTIL_
{
    public interface IOnStateMachine
    {
        void OnStateMachine(in AnimatorStateInfo stateInfo, in int layerIndex, in bool onEnter);
    }

    public class OnStateMachine : StateMachineBehaviour
    {
        [SerializeField] bool defaut_b;

        //----------------------------------------------------------------------------------------------------------

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (defaut_b)
                animator.SetLayerWeight(layerIndex, 0);
            OnState(animator, stateInfo, layerIndex, true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (defaut_b)
                animator.SetLayerWeight(layerIndex, 1);
            OnState(animator, stateInfo, layerIndex, false);
        }

        void OnState(in Animator animator, in AnimatorStateInfo stateInfo, in int layerIndex, in bool onEnter)
        {
            IOnStateMachine iUser = animator.GetComponentInParent<IOnStateMachine>();
            if (iUser == null)
                Debug.LogWarning($"{typeof(IOnStateMachine).FullName} not found on: {animator.transform.GetPath(true)}");
            else
                iUser.OnStateMachine(stateInfo, layerIndex, onEnter);
        }
    }
}