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
            return;
        }

        Controls = new UserInputs();
        Controls.Enable();

        QualitySettings.vSyncCount = 1;
        print("user inputs initialized");
    }

    private void OnDisable()
    {
        if (Controls == null) return;

        Controls.Disable();
        Controls.Dispose();

        Controls = null;
        Instance = null;
        print("Instance set to null");
    }



}
