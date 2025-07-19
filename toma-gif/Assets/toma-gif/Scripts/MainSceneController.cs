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
        private Actor player;

        [field: SerializeField]
        private Actor enemyPrefab;

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

        private readonly List<Actor> enemies = new();

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
            SetupEvidence();
            BeginObserveJudgementButtonAsync(CancellationToken.None).Forget();
        }

        private async UniTask BeginObserveJudgementButtonAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var isTrue = await uiViewInGame.OnClickJudgementButtonAsync(cancellationToken);
                if (isTrue && talkingIsTrueTalk || !isTrue && !talkingIsTrueTalk)
                {
                    Debug.Log("正解");
                }
                else
                {
                    Debug.Log("不正解");
                }

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
            var evidenceMessages = new List<string>();
            evidenceMessages.Add(evidenceMessage);
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
