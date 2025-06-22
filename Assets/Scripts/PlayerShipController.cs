using System.Collections.Generic;
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
        private Vector2 _acceleration = new(50.0f, 10000);
        [SerializeField]
        private Vector2 _breakDamping = new(2, 100);
        [SerializeField]
        private float _fireRate = 0.1f;

        [SerializeField]
        private float _cameraOffsetX = 4;
        [SerializeField]
        private float _cameraFollowSpeed = 10;

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

        public static List<PlayerShipController> Players = new();

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            Players.Add(this);
        }

        private void OnDisable()
        {
            Players.Remove(this);
        }

        private void Start()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _attackAction = InputSystem.actions.FindAction("Attack");

            _map = Map.Find(gameObject);
        }

        private void Update()
        {
            if (!IsGameOver)
                UpdateShip();

            UpdateCamera();
        }

        private void UpdateShip()
        {
            var moveInput = _moveAction.ReadValue<Vector2>();
            var maxSpeed = _maxSpeed;

            var deadzoneX = 0.2f;
            moveInput.x = Mathf.Sign(moveInput.x) * Mathf.InverseLerp(deadzoneX, 1, Mathf.Abs(moveInput.x));

            // Make speed more controllable with stick input
            if (Mathf.Abs(moveInput.x) > 0f)
            {
                maxSpeed.x *= Mathf.Lerp(deadzoneX, 1, Mathf.Abs(moveInput.x));
            }

            var acceleration = moveInput * _acceleration;

            // Make turning faster
            if (Mathf.Sign(_velocity.x) != Mathf.Sign(moveInput.x))
                acceleration.x *= 2;

            _velocity += acceleration * Time.deltaTime;

            // Y is fixed to match input (this works best with stick input)
            _velocity.y = moveInput.y * maxSpeed.y;

            // Max speed
            _velocity.x = Mathf.Clamp(_velocity.x, -maxSpeed.x, maxSpeed.x);
            _velocity.y = Mathf.Clamp(_velocity.y, -maxSpeed.y, maxSpeed.y);

            // Breaking
            if (moveInput.x == 0)
                _velocity.x += -_velocity.x * Mathf.Min(1, _breakDamping.x * Time.deltaTime);
            if (moveInput.y == 0)
                _velocity.y += -_velocity.y * Mathf.Min(1, _breakDamping.y * Time.deltaTime);

            var deltaPos = _velocity * Time.deltaTime;
            transform.Translate(deltaPos.x, deltaPos.y, 0);

            var pos = transform.position;
            // Clamp player inside vertical play area
            pos.y = Mathf.Clamp(pos.y,
                _map.Area.yMin + 0.3f,
                _map.Area.yMax + -0.3f
            );
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

            var camPos = cam.transform.position;
            var playerDir = _spriteRenderer.flipX ? -1 : 1;

            // Camera follows player ship
            var targetX = transform.position.x + _cameraOffsetX * playerDir;
            var deltaX = targetX - camPos.x;
            if (Mathf.Abs(deltaX) > 10)
            {
                camPos.x = targetX;
            }
            else
            {
                // Camera follows slower when ship is moving slower or backwards. Feels better when the player turns
                float followSpeed = Mathf.Lerp(_cameraFollowSpeed * 0.3f, _cameraFollowSpeed, Mathf.Clamp01(_velocity.x * playerDir / _maxSpeed.x));
                camPos.x += deltaX * Mathf.Min(1, followSpeed * Time.deltaTime);
            }

            // Shift to keep everything near origin
            var area = _map.Area;
            if (camPos.x > area.xMax)
            {
                camPos.x -= area.width;
                Shift(-area.width);
            }
            if (camPos.x < area.xMin)
            {
                camPos.x += area.width;
                Shift(area.width);
            }

            cam.transform.position = camPos;
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
            if (IsGameOver)
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
