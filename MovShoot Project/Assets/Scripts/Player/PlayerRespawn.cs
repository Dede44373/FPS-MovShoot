using UnityEngine;
using UnityEngine.Events;

public class PlayerRespawn : MonoBehaviour
{
    public UnityEvent respawn;
    //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (respawn == null)
            respawn = new UnityEvent();

        respawn.AddListener(OnEventTriggered);
    }
    void OnEventTriggered()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
