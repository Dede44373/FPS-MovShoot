using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour {

    [SerializeField] private float _mouseSensitivity;
    public PlayerMovement1 p;

    private float _xRotation = 0f;
    private Vector2 CameraDirection;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        if (UserInputManager.Instance != null && UserInputManager.Instance.Controls != null)
        {
            UserInputManager.Instance.Controls.Player.Look.performed += StartCameraMove;
            UserInputManager.Instance.Controls.Player.Look.canceled += StopCameraMove;
        }
    }
    private void OnDisable()
    {
        if (UserInputManager.Instance != null && UserInputManager.Instance.Controls != null)
        {
            UserInputManager.Instance.Controls.Player.Look.performed -= StartCameraMove;
            UserInputManager.Instance.Controls.Player.Look.canceled -= StopCameraMove;
        }
    }

    private void StartCameraMove(InputAction.CallbackContext ctx)
    {
        CameraDirection = ctx.ReadValue<Vector2>();
    }

    private void StopCameraMove(InputAction.CallbackContext ctx)
    {
        CameraDirection = Vector2.zero;
    }

    void Update() {
        // Get mouse input
        float mouseX = CameraDirection.x * _mouseSensitivity;
        float mouseY = CameraDirection.y * _mouseSensitivity;

        // Adjust the x rotation (pitch) and clamp it to avoid flipping the camera
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        // Apply rotations
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.parent.Rotate(Vector3.up * mouseX);
    }

}
