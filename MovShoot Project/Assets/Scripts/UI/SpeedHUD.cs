using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SpeedHUD : MonoBehaviour
{
    public Text SpeedText;
    public PlayerMovement mov;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
       
    }

    public float SpeedNumber;
  
    // Update is called once per frame

    void Update()
    {
    }

    private void setText()
    {
        SpeedText.text = SpeedNumber.ToString();
        SpeedNumber = mov.desiredMoveSpeed;  

    }
    
}
