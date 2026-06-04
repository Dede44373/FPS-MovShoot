using TMPro;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public AudioSource taco;
    public GameObject patrick;
    private async void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("You Died");
            patrick.SetActive(true);
            taco.Play();
            await Awaitable.WaitForSecondsAsync(1, destroyCancellationToken);
            Application.Quit();
        }
          
    }
}
 