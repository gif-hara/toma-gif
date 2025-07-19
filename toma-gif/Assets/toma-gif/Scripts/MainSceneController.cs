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
        private Transform cameraTransform;


        [field: SerializeField]
        private Transform enemySpawnPointParent;

        [field: SerializeField]
        private HKUIDocument inGameDocument;

        [field: SerializeField]
        private float enemyMoveDuration;

        [field: SerializeField]
        private Ease enemyMoveEase;

        [field: SerializeField]
        private int difficultyLevelUpThreshold;

        [field: SerializeField]
        private int evidenceCountMax;

        [field: SerializeField]
        private List<Evidence> evidences;

        [field: SerializeField]
        private float cameraShakeDuration;

        [field: SerializeField]
        private Vector3 cameraShakeStrength;

        [field: SerializeField]
        private AudioManager audioManager;

        private PlayerController playerController;

        private readonly List<EnemyController> enemies = new();

        private UIViewInGame uiViewInGame;

        private List<Evidence> currentEvidences = new();

        private Evidence talkingEvidence;

        private bool talkingIsTrueTalk;

        private int experience = 0;

        private int currentDifficultyLevel = 0;

        private int score;

        private int combo;

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
            score = 0;
            combo = 0;
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
                    enemy.PlayGotoHellAnimation();
                    LMotion.Shake.Create(Vector3.zero, cameraShakeStrength, cameraShakeDuration)
                        .BindToPosition(cameraTransform)
                        .AddTo(cameraTransform)
                        .ToUniTask(cancellationToken: cancellationToken)
                        .Forget();
                    uiViewInGame.SetActiveLieMessage(true);
                    audioManager.PlaySfx("GotoHell");
                }
                else
                {
                    enemy.PlayGotoHeavenAnimation();
                    audioManager.PlaySfx("GotoHeaven");
                }

                if (isTrue && talkingIsTrueTalk || !isTrue && !talkingIsTrueTalk)
                {
                    experience++;
                    if (experience >= difficultyLevelUpThreshold)
                    {
                        experience = 0;
                        currentDifficultyLevel = Mathf.Min(currentDifficultyLevel + 1, evidenceCountMax - 1);
                    }
                    audioManager.PlaySfx("Correct");
                    score += (currentDifficultyLevel + 1) * 100 + combo * 10;
                    combo++;
                    await uiViewInGame.ShowEffectCorrectAsync(cancellationToken);
                }
                else
                {
                    audioManager.PlaySfx("Incorrect");
                    combo = 0;
                    await uiViewInGame.ShowEffectIncorrectAsync(cancellationToken);
                }

                uiViewInGame.SetActiveLieMessage(false);
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
                .Take(currentDifficultyLevel + 1)
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
