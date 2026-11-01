using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    public void TurnOff()
    {
        gameObject.SetActive(false);
    }
    public void LoadEndScene()
    {
        SceneManager.LoadScene("End Screen");

    }
    public void LoadTutorial()
    {
        SceneManager.LoadScene("Tutorial");

    }
}
