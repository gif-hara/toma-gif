using System.Collections.Generic;
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

        private readonly List<Actor> enemies = new();

        private UIViewInGame uiViewInGame;

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
            BeginObserveJudgementButtonAsync(CancellationToken.None).Forget();
        }

        private async UniTask BeginObserveJudgementButtonAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var isTrue = await uiViewInGame.OnClickJudgementButtonAsync(cancellationToken);
                if (isTrue)
                {
                    // Handle true button click
                    Debug.Log("True button clicked");
                }
                else
                {
                    // Handle false button click
                    Debug.Log("False button clicked");
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
            }
        }
    }
}
