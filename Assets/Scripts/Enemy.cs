using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 2;

        [SerializeField]
        private float _fireRate = 2;
        [SerializeField]
        private GameObject _projectilePrefab;

        [SerializeField]
        private GameObject _deathFxPrefab;

        [SerializeField]
        private int _score = 100;

        private Vector2 _moveDirection = new(1, -0.5f);
        private Map _map;
        private float _fireTimer;

        public int Score => _score;

        private void Start()
        {
            _map = Map.Find(gameObject);
        }

        private void OnEnable()
        {
            _fireTimer = 0;
        }

        private void Update()
        {
            // FIXME: might be framerate dependent and has no minimum frequency of triggering
            if (Random.value < 0.5f * Time.deltaTime)
                _moveDirection.x *= -1;

            var pos = transform.position;
            if (pos.y > _map.Area.yMax - 0.5f)
                _moveDirection.y = -0.5f;
            if (pos.y < _map.Area.yMin + 0.5f)
                _moveDirection.y = 0.5f;

            pos += (Vector3)(_speed * Time.deltaTime * _moveDirection);
            transform.position = pos;

            bool shootEnabled = true;
            // Shoot only when close enough to currently visible play area
            var cam = Camera.main;
            if (cam != null)
            {
                shootEnabled = Mathf.Abs(cam.transform.position.x - pos.x) < cam.orthographicSize;
            }

            if (shootEnabled && _fireTimer > _fireRate)
            {
                ShootProjectile();
            }
            _fireTimer += Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.TryGetComponent<PlayerShipController>(out var player))
            {
                player.Damage();
            }
        }

        public void Kill()
        {
            GameObjectPool.Spawn(_deathFxPrefab, transform.position, Quaternion.identity);
            GameObjectPool.Destroy(gameObject);
        }

        private void ShootProjectile()
        {
            var go = GameObjectPool.Spawn(_projectilePrefab, transform.position, Quaternion.identity);
            if (go.TryGetComponent<EnemyProjectile>(out var projectile))
            {
                Vector2 dir;
                var player = FindNearestPlayer();
                if (player != null)
                    dir = (player.transform.position - transform.position).normalized;
                else
                    dir = Random.onUnitSphere;

                projectile.Launch(dir);
            }
            _fireTimer = 0;
        }

        private PlayerShipController FindNearestPlayer()
        {
            Vector2 pos = transform.position;
            float minDist = float.PositiveInfinity;
            PlayerShipController minPlayer = null;
            foreach (var player in PlayerShipController.Players)
            {
                Vector2 pPos = player.transform.position;
                var dist = Mathf.Abs(pPos.x - pos.x);
                if (dist < minDist)
                {
                    minDist = dist;
                    minPlayer = player;
                }
            }
            return minPlayer;
        }
    }
}
