using Cysharp.Threading.Tasks;
using HK;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace tomagif
{
    public class EnemyController
    {
        private readonly HKUIDocument document;

        private readonly Animator animator;

        public GameObject RootGameObject => document.gameObject;

        public EnemyController(HKUIDocument document)
        {
            this.document = document;
            this.animator = document.Q<Animator>("Animator");
        }

        public UniTask MoveAsync(Vector3 toPosition, float enemyMoveDuration, Ease enemyMoveEase)
        {
            return LMotion.Create(document.transform.position, toPosition, enemyMoveDuration)
                .WithEase(enemyMoveEase)
                .BindToPosition(document.transform)
                .AddTo(document.transform)
                .ToUniTask();
        }

        public void PlayIdleAnimation()
        {
            animator.Play("Idle");
        }

        public void PlayGotoHellAnimation()
        {
            animator.Play("GotoHell");
        }

        public void PlayGotoHeavenAnimation()
        {
            animator.Play("GotoHeaven");
        }
    }
}
