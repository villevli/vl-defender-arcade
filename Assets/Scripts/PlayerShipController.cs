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
        }

        private void ShootProjectile()
        {
            var pos = _cannon.position;
            if (_spriteRenderer.flipX)
                pos.x *= -1;
            var rot = _spriteRenderer.flipX ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
            _projectilePool.Spawn(_projectilePrefab, pos, rot);
            _fireTimer = 0;
        }
    }
}
