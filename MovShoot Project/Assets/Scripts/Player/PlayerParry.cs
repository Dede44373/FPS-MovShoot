using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

public class PlayerParry : PlayerThrow
{
    public float parryWindow;
    public float parryCooldown;
    private float parryTime;
    public PlayerHealth health;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
   //     health = GetComponent<PlayerHealth>();   
    }


    //public async void HandleAttackStart(InputAction.CallbackContext ctx)
    //{
    //    print("big money moves");
    //    while (parryWindow >= 0)
    //    {
    //        parryTime += Time.deltaTime;
    //        await Awaitable.NextFrameAsync(destroyCancellationToken);
    //    }
    //    // Update is called once per frame
    //}


        void Update()
        {

        }
}
