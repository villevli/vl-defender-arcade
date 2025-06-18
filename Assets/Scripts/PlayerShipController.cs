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
        private float _speed = 5.0f;
        [SerializeField]
        private float _fireRate = 0.1f;

        [SerializeField]
        private float _yMin = -10;
        [SerializeField]
        private float _yMax = 10;

        [SerializeField]
        private float _cameraOffsetX = 4;

        [SerializeField]
        private GameObject _projectilePrefab;
        [SerializeField, Tooltip("Projectiles are launched from this")]
        private Transform _cannon;
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        private InputAction _moveAction;
        private InputAction _attackAction;

        private float _fireTimer;
        private GameObjectPool _projectilePool = new();

        private void Reset()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _attackAction = InputSystem.actions.FindAction("Attack");
        }

        private void Update()
        {
            var moveValue = _moveAction.ReadValue<Vector2>() * _speed * Time.deltaTime;
            transform.Translate(moveValue.x, moveValue.y, 0);

            var pos = transform.position;
            // clamp player inside vertical play area
            pos.y = Mathf.Clamp(pos.y, _yMin, _yMax);
            transform.position = pos;

            // Face the ship towards movement direction
            if (moveValue.x != 0)
            {
                _spriteRenderer.flipX = moveValue.x < 0;
            }

            if (_fireTimer > _fireRate && _attackAction.IsPressed())
            {
                ShootProjectile();
            }
            _fireTimer += Time.deltaTime;

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            var targetX = transform.position.x + _cameraOffsetX * (_spriteRenderer.flipX ? -1 : 1);
            var pos = cam.transform.position;
            if (Mathf.Abs(targetX - pos.x) > 10)
                pos.x = targetX;
            else
                pos.x = Mathf.MoveTowards(pos.x, targetX, Time.deltaTime * _speed * 2);
            cam.transform.position = pos;
        }

        private void ShootProjectile()
        {
            var pos = _cannon.localPosition;
            if (_spriteRenderer.flipX)
                pos.x *= -1;
            pos = transform.position + pos;
            var rot = _spriteRenderer.flipX ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            _projectilePool.Spawn(_projectilePrefab, pos, rot);
            _fireTimer = 0;
        }
    }
}
