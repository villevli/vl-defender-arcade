using System.Collections.Generic;
using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private Rect _area = new(-32, -2, 64, 4);

        [SerializeField]
        private float _intervalMin = 2;
        [SerializeField]
        private float _intervalMax = 8;

        [SerializeField]
        private GameObject _enemyPrefab;

        [SerializeField]
        private int _maxSpawned = 6;

        private float _spawnTimer;
        private List<SpawnedGameObject> _spawned = new();

        private class SpawnedGameObject : MonoBehaviour
        {
            public EnemySpawner SpawnedFrom;

            private void OnDisable()
            {
                if (SpawnedFrom != null)
                    SpawnedFrom._spawned.Remove(this);
                SpawnedFrom = null;
            }
        }

        private void OnEnable()
        {
            RandomizeTimer();
        }

        private void RandomizeTimer()
        {
            _spawnTimer = Random.Range(_intervalMin, _intervalMax);
        }

        private void Update()
        {
            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0 && _spawned.Count < _maxSpawned)
            {
                Spawn();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + (Vector3)_area.center, _area.size);
        }

        private void Spawn()
        {
            var pos = transform.position + new Vector3(
                Random.Range(_area.xMin, _area.xMax),
                Random.Range(_area.yMin, _area.yMax)
            );
            var go = GameObjectPool.Spawn(_enemyPrefab, pos, Quaternion.identity);
            var sgo = go.GetOrAddComponent<SpawnedGameObject>();
            sgo.SpawnedFrom = this;
            _spawned.Add(sgo);

            RandomizeTimer();
        }
    }
}
