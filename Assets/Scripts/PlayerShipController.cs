using UnityEngine;
using UnityEngine.InputSystem;

namespace VLDefenderArcade
{
    /// <summary>
    /// Control player movement and actions of the ship.
    /// </summary>
    public class PlayerShipController : MonoBehaviour
    {
        [SerializeField]
        private int _lives = 5;

        [SerializeField]
        private Vector2 _maxSpeed = new(20.0f, 10.0f);
        [SerializeField]
        private Vector2 _acceleration = new(100.0f, 10000);
        [SerializeField]
        private Vector2 _breakDamping = new(3, 100);
        [SerializeField]
        private float _fireRate = 0.1f;

        [SerializeField]
        private float _yMin = -10;
        [SerializeField]
        private float _yMax = 10;

        [SerializeField]
        private float _cameraOffsetX = 4;
        [SerializeField]
        private float _cameraFollowSpeed = 5;

        [SerializeField]
        private GameObject _projectilePrefab;
        [SerializeField, Tooltip("Projectiles are launched from this")]
        private Transform _cannon;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;
        [SerializeField]
        private GameObject _deathFxPrefab;

        private InputAction _moveAction;
        private InputAction _attackAction;

        private Map _map;

        private Vector2 _velocity;
        private float _fireTimer;
        private int _score;

        public int Lives => _lives;
        public bool IsGameOver => Lives <= 0;
        public int Score => _score;

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _attackAction = InputSystem.actions.FindAction("Attack");

            _map = Map.Find(gameObject);
        }

        private void Update()
        {
            if (_lives > 0)
                UpdateShip();

            UpdateCamera();
        }

        private void UpdateShip()
        {
            var moveInput = _moveAction.ReadValue<Vector2>();

            var acceleration = moveInput * _acceleration;

            // Make turning faster
            if (Mathf.Sign(_velocity.x) != Mathf.Sign(moveInput.x))
                acceleration.x *= 2;

            _velocity += acceleration * Time.deltaTime;

            // Max speed
            _velocity.x = Mathf.Clamp(_velocity.x, -_maxSpeed.x, _maxSpeed.x);
            _velocity.y = Mathf.Clamp(_velocity.y, -_maxSpeed.y, _maxSpeed.y);

            // Breaking
            if (moveInput.x == 0)
                _velocity.x += -_velocity.x * Mathf.Min(1, _breakDamping.x * Time.deltaTime);
            if (moveInput.y == 0)
                _velocity.y += -_velocity.y * Mathf.Min(1, _breakDamping.y * Time.deltaTime);

            var deltaPos = _velocity * Time.deltaTime;
            transform.Translate(deltaPos.x, deltaPos.y, 0);

            var pos = transform.position;
            // Clamp player inside vertical play area
            pos.y = Mathf.Clamp(pos.y, _yMin, _yMax);
            transform.position = pos;

            // Face the ship towards movement input
            if (moveInput.x != 0)
            {
                _spriteRenderer.flipX = moveInput.x < 0;
            }

            if (_fireTimer > _fireRate && _attackAction.IsPressed())
            {
                ShootProjectile();
            }
            _fireTimer += Time.deltaTime;
        }

        private void UpdateCamera()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            var targetX = transform.position.x + _cameraOffsetX * (_spriteRenderer.flipX ? -1 : 1);
            var pos = cam.transform.position;
            var deltaX = targetX - pos.x;
            if (Mathf.Abs(deltaX) > 10)
                pos.x = targetX;
            else
                pos.x += deltaX * Mathf.Min(1, _cameraFollowSpeed * Time.deltaTime);

            // Shift to keep everything near origin
            if (pos.x > _map.Width / 2)
            {
                pos.x -= _map.Width;
                Shift(-_map.Width);
            }
            if (pos.x < -_map.Width / 2)
            {
                pos.x += _map.Width;
                Shift(_map.Width);
            }

            cam.transform.position = pos;
        }

        private void Shift(float amount)
        {
            var pos = transform.position;
            pos.x += amount;
            transform.position = pos;
        }

        private void ShootProjectile()
        {
            var pos = _cannon.localPosition;
            var playerDirection = _spriteRenderer.flipX ? -1f : 1f;
            pos.x *= playerDirection;
            pos = transform.position + pos;
            var rot = _spriteRenderer.flipX ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            var go = GameObjectPool.Spawn(_projectilePrefab, pos, rot);
            if (go.TryGetComponent<PlayerProjectile>(out var projectile))
            {
                projectile.SetPlayer(this);

                if (Mathf.Sign(_velocity.x) == playerDirection)
                    projectile.InheritSpeed(Mathf.Abs(_velocity.x));
            }
            _fireTimer = 0;
        }

        public void GiveScore(int score)
        {
            _score += score;
        }

        public void Damage()
        {
            if (_lives <= 0)
                return;

            _lives--;
            if (_lives <= 0)
            {
                GameOver();
            }
        }

        private void GameOver()
        {
            GameObjectPool.Spawn(_deathFxPrefab, transform.position, Quaternion.identity);
            _spriteRenderer.enabled = false;
        }
    }
}
