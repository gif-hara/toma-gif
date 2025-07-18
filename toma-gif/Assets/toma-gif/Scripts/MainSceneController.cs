using System.Collections.Generic;
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

        private readonly List<Actor> enemies = new();

        void Start()
        {
            foreach (Transform spawnPoint in enemySpawnPointParent)
            {
                var enemyActor = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
                enemies.Add(enemyActor);
            }
        }
    }
}
