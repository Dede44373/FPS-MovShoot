using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject FadeOut;
    public void PlayGame()
    {
        if (FadeOut != null)
        {
            FadeOut.SetActive(true);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        print("Game Quitted");
    }
}
