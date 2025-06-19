using System.Collections.Generic;
using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField]
        private Vector2 _size = new(64, 4);

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

        public Rect Area => new((Vector2)transform.position - _size / 2, _size);

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
            Gizmos.DrawWireCube(transform.position, _size);
        }

        private void Spawn()
        {
            var pos = RandomizeSpawnPos();
            var go = GameObjectPool.Spawn(_enemyPrefab, pos, Quaternion.identity);
            var sgo = go.GetOrAddComponent<SpawnedGameObject>();
            sgo.SpawnedFrom = this;
            _spawned.Add(sgo);

            RandomizeTimer();
        }

        private Vector3 RandomizeSpawnPos()
        {
            var area = this.Area;
            return new(
                Random.Range(area.xMin, area.xMax),
                Random.Range(area.yMin, area.yMax),
                transform.position.z
            );
        }
    }
}
