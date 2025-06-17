using UnityEngine;

namespace VLDefenderArcade
{
    /// <summary>
    /// A flying projectile for destroying enemies.
    /// </summary>
    public class PlayerProjectile : MonoBehaviour
    {
        [SerializeField]
        private float _speed = 10.0f;
        [SerializeField]
        private float _lifetime = 1.0f;

        private float _elapsed = 0;

        private void OnEnable()
        {
            _elapsed = 0;
        }

        private void OnDisable()
        {
            // Needed to fix the effect when pooled
            if (TryGetComponent<TrailRenderer>(out var tr))
                tr.Clear();
        }

        private void Update()
        {
            transform.Translate(_speed * Time.deltaTime, 0, 0);

            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                GameObjectPool.Destroy(gameObject);
            }
        }
    }
}
