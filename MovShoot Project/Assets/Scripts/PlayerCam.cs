using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    public float senX;
    public float senY;

    public Transform orientation;
    private UserInputs Controls;

    float xRotation;
    float yRotation;

    private Vector2 LookValue;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Look.performed += ChangeCameraMovement;
        Controls.Player.Look.canceled += StopCameraMovement;
    }

    private void OnDisable()
    {
        Controls.Player.Look.performed -= ChangeCameraMovement;
        Controls.Player.Look.canceled -= StopCameraMovement;
    }

    private void ChangeCameraMovement(InputAction.CallbackContext ctx)
    {
        LookValue = ctx.ReadValue<Vector2>();
    }

    private void StopCameraMovement(InputAction.CallbackContext ctx)
    {
        LookValue = Vector2.zero;
    }
    /*
    private void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * senX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * senY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
    */  
    private void Update()
    {
        // Get mouse input
        float mouseX = LookValue.x * Time.deltaTime * senX;
        float mouseY = LookValue.y * Time.deltaTime * senY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        //Rotate camera and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

}
