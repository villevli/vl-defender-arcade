using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// A flying projectile for destroying enemies.
    /// </summary>
    public class PlayerProjectile : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 30.0f;
        [SerializeField]
        private float _lifetime = 1.0f;

        private float _inheritedSpeed = 0;
        private PlayerShipController _player;
        private float _elapsed = 0;
        private Enemy _hitEnemy;

        public void SetPlayer(PlayerShipController player)
        {
            _player = player;
        }

        public void InheritSpeed(float speed)
        {
            _inheritedSpeed = speed;
        }

        private void OnEnable()
        {
            _elapsed = 0;
            _inheritedSpeed = 0;
            _player = null;
            _hitEnemy = null;
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            // Hit effect after raycast is delayed by one frame so we get to render one frame with the projectile hitting
            if (_hitEnemy)
            {
                _hitEnemy.Kill();
                _player.GiveScore(_hitEnemy.Score);
                _hitEnemy = null;
                // projectile gets destroyed
                GameObjectPool.Destroy(gameObject);
                return;
            }

            var pos = (Vector2)transform.position;
            var dir = (Vector2)transform.right;
            var deltaLength = (_speed + _inheritedSpeed) * Time.deltaTime;

            // Using raycast instead of having collider on the projectile for more reliable hits when moving fast
            var hit = Physics2D.Raycast(pos, dir, deltaLength);
            if (hit && hit.transform.TryGetComponent<Enemy>(out var enemy))
            {
                _hitEnemy = enemy;
                deltaLength = Mathf.Min(deltaLength, hit.distance + 0.1f);
            }

            transform.position = pos + dir * deltaLength;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                GameObjectPool.Destroy(gameObject);
            }
        }
    }
}
