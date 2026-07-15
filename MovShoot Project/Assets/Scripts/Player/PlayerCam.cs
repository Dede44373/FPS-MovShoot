using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class PlayerCam : MonoBehaviour
{
    public float senX;
    public float senY;

    public Camera cam;
    public Transform orientation;
    public Transform camHolder;
    private UserInputs Controls;

    float xRotation;
    float yRotation;

    private Vector2 LookValue;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        QualitySettings.vSyncCount = -1;

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
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void DoFov(float endValue)
    {
        Debug.Log("FOV");
        cam.DOFieldOfView(endValue, 0.25f);
    }
     
    public void DoTilt (float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}
