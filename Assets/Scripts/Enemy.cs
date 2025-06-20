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

            if (_fireTimer > _fireRate)
            {
                ShootProjectile();
            }
            _fireTimer += Time.deltaTime;
        }

        private void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            // Shift
            var pos = transform.position;
            var camPos = cam.transform.position;
            if (pos.x - camPos.x > _map.Width / 2)
            {
                pos.x -= _map.Width;
            }
            if (pos.x - camPos.x < -_map.Width / 2)
            {
                pos.x += _map.Width;
            }
            transform.position = pos;
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
                projectile.Launch(new(1.0f, 0f));
            }
            _fireTimer = 0;
        }
    }
}
