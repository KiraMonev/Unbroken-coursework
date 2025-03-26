using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _deceleration = 50f;
    [SerializeField] private float _velocityPower = 0.9f;



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
}
