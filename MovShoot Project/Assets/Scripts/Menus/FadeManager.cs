using UnityEngine;

public class FadeManager : MonoBehaviour
{
    public GameObject fIn;
    public GameObject fOut;
    
    //Gets Lighter
    public void FadeOut()
    {
        fOut.SetActive(true);
    }
    
    //Gets Darker
    public void FadeIn()
    {
        fIn.gameObject.SetActive(true);
    }
}
