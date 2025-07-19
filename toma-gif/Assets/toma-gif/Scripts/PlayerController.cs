using HK;
using UnityEngine;

namespace tomagif
{
    public class PlayerController
    {
        private readonly HKUIDocument document;

        private readonly Animator animator;

        public PlayerController(HKUIDocument document)
        {
            this.document = document;
            this.animator = document.Q<Animator>("Animator");
        }

        public void PlayIdleAnimation()
        {
            animator.Play("Idle");
        }

        public void PlayAttackAnimation()
        {
            animator.Play("Attack");
        }
    }
}
