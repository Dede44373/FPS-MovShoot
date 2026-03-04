using UnityEngine;

public class UserInputManager : MonoBehaviour
{
    public static UserInputManager Instance { get; private set; }
    public UserInputs Controls;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Controls = new UserInputs();
        Controls.Enable();
    }

    private void OnDisable()
    {
        if (Controls == null) return;
        Controls.Disable();
        Controls = null;
    }
}
