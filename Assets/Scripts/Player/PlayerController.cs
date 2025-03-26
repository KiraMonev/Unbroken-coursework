using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 50f;
    [SerializeField] private float _velocityPower = 0.9f;
    [SerializeField] private LayerMask _wallLayer;

    private Vector2 _moveInput;
    private Rigidbody2D _rigidbody;
    private Vector2 _velocity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        // Если есть ввод и путь заблокирован в направлении движения
        if (_moveInput.magnitude > 0.01f && IsPathBlocked(_moveInput.normalized))
        {
            _velocity = Vector2.zero; // Останавливаем только если путь заблокирован
            Debug.Log("Wall hit in move direction");
            return;
        }

        // Расчет целевой скорости
        Vector2 targetVelocity = _moveInput * _maxSpeed;
        Vector2 velocityDiff = targetVelocity - _velocity;
        float accelerateRate = (targetVelocity.magnitude > 0.01f) ? _acceleration : _deceleration;
        Vector2 movement = velocityDiff * (accelerateRate * Time.fixedDeltaTime);

        _velocity += movement;
        _velocity = Vector2.ClampMagnitude(_velocity, _maxSpeed);
        _velocity *= Mathf.Pow(1f - _velocityPower, Time.fixedDeltaTime);

        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public Vector2 GetMoveInput()
    {
        return _moveInput;
    }

    private bool IsPathBlocked(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(_rigidbody.position, direction, 0.4f, _wallLayer);
        // Debug.DrawRay(_rigidbody.position, direction * 0.4f, Color.red); // Визуализация луча
        return hit.collider != null;
    }
}