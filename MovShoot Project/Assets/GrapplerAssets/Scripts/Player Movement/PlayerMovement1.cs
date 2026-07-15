using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement1 : MonoBehaviour {

    [SerializeField] private float _speed;
    private Vector3 _moveInputDirection;

    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private Transform _groundCheckTransform;

    private Rigidbody _rigidbody;
    private bool _isGrounded;
    private Vector2 CameraDirection;

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        UserInputManager.Instance.Controls.Player.Move.performed += StartCameraMove;
        UserInputManager.Instance.Controls.Player.Move.canceled += StopCameraMove;
        UserInputManager.Instance.Controls.Player.Jump.performed += Jump;
    }
    private void OnDisable()
    {
        UserInputManager.Instance.Controls.Player.Move.performed -= StartCameraMove;
        UserInputManager.Instance.Controls.Player.Move.canceled -= StopCameraMove;
        UserInputManager.Instance.Controls.Player.Jump.performed -= Jump;
    }

    private void StartCameraMove(InputAction.CallbackContext ctx)
    {
        CameraDirection = ctx.ReadValue<Vector2>();
    }

    private void StopCameraMove(InputAction.CallbackContext ctx)
    {
        CameraDirection = Vector2.zero;
    }

    private void Update() {
        _moveInputDirection = CameraDirection.x * transform.right + CameraDirection.y * transform.forward;
        _isGrounded = Physics.CheckSphere(_groundCheckTransform.position, .05f, 3);

        Move();
    }

    private void Move() {
        if (!_isGrounded) {
            return;
        }

        if (_moveInputDirection == Vector3.zero) { 
            _rigidbody.linearVelocity = new Vector3(0, _rigidbody.linearVelocity.y, 0);
        } else {
            _moveInputDirection.Normalize();
            _rigidbody.linearVelocity = _speed * _moveInputDirection;
        }
    }

    private void Jump(InputAction.CallbackContext ctx) {
        if (_isGrounded) {
            _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
        }
    }
}
