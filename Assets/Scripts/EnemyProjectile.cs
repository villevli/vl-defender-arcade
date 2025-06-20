using Unity.Collections;
using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 4.0f;
        [SerializeField]
        private float _lifetime = 5.0f;

        private Map _map;
        private float _elapsed = 0;

        public void Launch(Vector2 direction)
        {
            if (TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = direction * _speed;
            }
        }

        private void Start()
        {
            _map = Map.Find(gameObject);
        }

        private void OnEnable()
        {
            _elapsed = 0;
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            // Movement done by setting rigidbody velocity

            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                GameObjectPool.Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<PlayerShipController>(out var player))
            {
                player.Damage();
                // projectile gets destroyed
                GameObjectPool.Destroy(gameObject);
                return;
            }
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
                Shift(-_map.Width);
            }
            if (pos.x - camPos.x < -_map.Width / 2)
            {
                pos.x += _map.Width;
                Shift(_map.Width);
            }
            transform.position = pos;
        }

        private void Shift(float amount)
        {
        }
    }
}
