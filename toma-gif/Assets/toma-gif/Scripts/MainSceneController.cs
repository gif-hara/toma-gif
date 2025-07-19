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
        private int evidenceCount;

        [field: SerializeField]
        private List<Evidence> evidences;

        private PlayerController playerController;

        private readonly List<HKUIDocument> enemies = new();

        private UIViewInGame uiViewInGame;

        private List<Evidence> currentEvidences = new();

        private Evidence talkingEvidence;

        private bool talkingIsTrueTalk;

        void Start()
        {
            foreach (Transform spawnPoint in enemySpawnPointParent)
            {
                var enemyActor = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                enemies.Add(enemyActor);
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

                if (!isTrue)
                {
                    playerController.PlayAttackAnimation();
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
                var enemy = enemies[0];
                enemies.RemoveAt(0);
                Destroy(enemy.gameObject);
                for (var i = 0; i < enemies.Count; i++)
                {
                    var toPosition = enemySpawnPointParent.GetChild(i).position;
                    LMotion.Create(enemies[i].transform.position, toPosition, enemyMoveDuration)
                        .WithEase(enemyMoveEase)
                        .BindToPosition(enemies[i].transform)
                        .AddTo(enemies[i].transform)
                        .ToUniTask()
                        .Forget();
                }
                var newSpawnPoint = enemySpawnPointParent.GetChild(enemies.Count);
                var newEnemy = Instantiate(enemyPrefab, newSpawnPoint.position, Quaternion.identity);
                enemies.Add(newEnemy);
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
