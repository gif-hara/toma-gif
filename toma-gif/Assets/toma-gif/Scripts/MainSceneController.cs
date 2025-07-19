using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using HK;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace tomagif
{
    public class MainSceneController : MonoBehaviour
    {
        [field: SerializeField]
        private HKUIDocument player;

        [field: SerializeField]
        private HKUIDocument enemyPrefab;

        [field: SerializeField]
        private Transform enemySpawnPointParent;

        [field: SerializeField]
        private HKUIDocument inGameDocument;

        [field: SerializeField]
        private float enemyMoveDuration;

        [field: SerializeField]
        private Ease enemyMoveEase;

        [field: SerializeField]
        private float enemyDefeatOffsetPosition;

        [field: SerializeField]
        private int enemyDefeatDuration;

        [field: SerializeField]
        private Ease enemyDefeatEase;

        [field: SerializeField]
        private int evidenceCount;

        [field: SerializeField]
        private List<Evidence> evidences;

        private PlayerController playerController;

        private readonly List<EnemyController> enemies = new();

        private UIViewInGame uiViewInGame;

        private List<Evidence> currentEvidences = new();

        private Evidence talkingEvidence;

        private bool talkingIsTrueTalk;

        void Start()
        {
            foreach (Transform spawnPoint in enemySpawnPointParent)
            {
                var enemyActor = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                var enemyController = new EnemyController(enemyActor);
                enemyController.PlayIdleAnimation();
                enemies.Add(enemyController);
            }
            uiViewInGame = new UIViewInGame(inGameDocument);
            uiViewInGame.Initialize();
            uiViewInGame.Activate();
            playerController = new PlayerController(player);
            playerController.PlayIdleAnimation();
            SetupEvidence();
            BeginObserveJudgementButtonAsync(CancellationToken.None).Forget();
        }

        private async UniTask BeginObserveJudgementButtonAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var isTrue = await uiViewInGame.OnClickJudgementButtonAsync(cancellationToken);
                var enemy = enemies[0];
                enemies.RemoveAt(0);

                if (!isTrue)
                {
                    playerController.PlayAttackAnimation();
                    enemy.PlayDeadAnimation();
                }

                if (isTrue && talkingIsTrueTalk || !isTrue && !talkingIsTrueTalk)
                {
                    await uiViewInGame.ShowEffectCorrectAsync(cancellationToken);
                }
                else
                {
                    await uiViewInGame.ShowEffectIncorrectAsync(cancellationToken);
                }

                playerController.PlayIdleAnimation();
                Destroy(enemy.RootGameObject);
                for (var i = 0; i < enemies.Count; i++)
                {
                    var toPosition = enemySpawnPointParent.GetChild(i).position;
                    enemies[i].MoveAsync(toPosition, enemyMoveDuration, enemyMoveEase)
                        .Forget();
                }
                var newSpawnPoint = enemySpawnPointParent.GetChild(enemies.Count);
                var newEnemy = Instantiate(enemyPrefab, newSpawnPoint.position, Quaternion.identity);
                var newEnemyController = new EnemyController(newEnemy);
                newEnemyController.PlayIdleAnimation();
                enemies.Add(newEnemyController);
                SetupEvidence();
            }
        }

        private void SetupEvidence()
        {
            currentEvidences = evidences
                .OrderBy(_ => Random.value)
                .Take(evidenceCount)
                .ToList();
            talkingEvidence = currentEvidences[0];
            var isPositive = Random.value > 0.5f;
            var evidenceMessage = isPositive
                ? talkingEvidence.PositiveEvidences[Random.Range(0, talkingEvidence.PositiveEvidences.Count)]
                : talkingEvidence.NegativeEvidences[Random.Range(0, talkingEvidence.NegativeEvidences.Count)];
            var talkMessage = talkingEvidence.Messages[Random.Range(0, talkingEvidence.Messages.Count)];
            talkingIsTrueTalk = talkMessage.IsPositive == isPositive;
            var evidenceMessages = new List<string>
            {
                evidenceMessage
            };
            for (var i = 1; i < currentEvidences.Count; i++)
            {
                var evidence = currentEvidences[i];
                var message = isPositive
                    ? evidence.PositiveEvidences[Random.Range(0, evidence.PositiveEvidences.Count)]
                    : evidence.NegativeEvidences[Random.Range(0, evidence.NegativeEvidences.Count)];
                evidenceMessages.Add(message);
            }
            uiViewInGame.SetupEvidences(evidenceMessages.OrderBy(x => Random.value).ToList(), talkMessage.Message);
        }
    }
}
