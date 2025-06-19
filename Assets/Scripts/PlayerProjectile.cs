using Unity.Collections;
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

        private Map _map;
        private float _inheritedSpeed = 0;
        private float _elapsed = 0;
        private Enemy _hitEnemy;

        public void InheritSpeed(float speed)
        {
            _inheritedSpeed = speed;
        }

        private void Start()
        {
            _map = Map.Find(gameObject);
        }

        private void OnEnable()
        {
            _elapsed = 0;
            _inheritedSpeed = 0;
            _hitEnemy = null;
        }

        private void OnDisable()
        {
            // Needed to fix the effect when pooled
            if (TryGetComponent<TrailRenderer>(out var tr))
                tr.Clear();
        }

        private void Update()
        {
            // Hit effect after raycast is delayed by one frame so we get to render one frame with the projectile hitting
            if (_hitEnemy)
            {
                _hitEnemy.Kill();
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
            if (TryGetComponent<TrailRenderer>(out var tr))
            {
                using var buffer = new NativeArray<Vector3>(tr.positionCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                tr.GetPositions(buffer);
                var span = buffer.AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    span[i].x += amount;
                }
                tr.SetPositions(buffer);
            }
        }
    }
}
