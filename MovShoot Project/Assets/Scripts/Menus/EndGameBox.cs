using UnityEngine;

public class EndGameBox : MonoBehaviour
{
    public Animator anim;
    public GameObject fade;
    public PlayerMovement mov;
    public PlayerGrapple grap;
    public PlayerWallRun wall;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            print("End Game");
            mov.enabled = false;
            grap.enabled = false;
            wall.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            fade.gameObject.SetActive(true);

        }
    }
}
