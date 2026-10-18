using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteract : MonoBehaviour
{
    public UserInputs Controls;
    public PlayerCam cameraControls;
    public PlayerAttack playerAttack;
    public GameObject levelUpMenu;
    public GameObject interactPrompt;
    public bool activateInteract;
    public bool inMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inMenu = false;
        activateInteract = false;
        Reset();
    }

    private void OnEnable()
    {
        Controls = UserInputManager.Instance.Controls;
        Controls.Player.Interact.started += InteractStart;
    }   
    private void OnDisable()
    {
        Controls.Player.Interact.started -= InteractStart;
    }

    private async void InteractStart(InputAction.CallbackContext ctx)
    {
        if (activateInteract) return;
        if (!inMenu)
        {
            activateInteract = true;
            await Awaitable.WaitForSecondsAsync(0.5f);
            activateInteract = false;
        }
        else if (inMenu)
        {
            Reset();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        { 
            interactPrompt.SetActive(true);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Checkpoint") && activateInteract)
        {
            interactPrompt.SetActive(false);
            levelUpMenu.SetActive(false);
            inMenu = true;
            print("BIGGGGGGGGGGGG CHECKPIOINT");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            cameraControls.enabled = false;
            playerAttack.enabled = false;

            levelUpMenu.SetActive(true);

        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            Reset();
        }
    }

    private void Reset()
    {
        inMenu = false;
        print("Leaving Checkpoint");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraControls.enabled = true;
        playerAttack.enabled = true;

        interactPrompt.SetActive(false);
        levelUpMenu.SetActive(false);

    }
    // Update is called once per frame
    void Update()
    {

    }
}