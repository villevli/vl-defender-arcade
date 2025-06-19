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
        private GameObject _deathFxPrefab;

        private Vector2 _moveDirection = new(1, -0.5f);
        private Map _map;

        private void Start()
        {
            _map = Map.Find(gameObject);
        }

        private void Update()
        {
            // FIXME: might be framerate dependent and has no minimum frequency of triggering
            if (Random.value < 0.5f * Time.deltaTime)
                _moveDirection.x *= -1;

            var pos = transform.position;
            if (pos.y > _map.Rect.yMax - 1)
                _moveDirection.y = -0.5f;
            if (pos.y < _map.Rect.yMin + 1)
                _moveDirection.y = 0.5f;

            pos += (Vector3)(_speed * Time.deltaTime * _moveDirection);
            transform.position = pos;
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

        public void Kill()
        {
            Instantiate(_deathFxPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
