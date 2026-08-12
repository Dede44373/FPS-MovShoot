using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class SpeedHUD : MonoBehaviour
{
    public TextMeshProUGUI SpeedText;
    public PlayerMovement mov;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        mov = PlayerMovement.instance;
    }

    public float SpeedNumber;
  
    // Update is called once per frame

    void Update()
    {
        //Sets the textmeshpro object (only works with TextMeshProUGUI) to the SetText() function. the "" is for string and {0:2} is the numbers you want to change.
        //First being the Ints and the 2nd number controls how many decimal points you want. the final variable is the reference to what float you actually want to see. 
        SpeedText.SetText("Speed: {0:2}\nDMSpeed: {1:1}", mov.rb.linearVelocity.magnitude, mov.desiredMoveSpeed);
    }

    private void setText()
    {
        SpeedText.text = SpeedNumber.ToString();
        SpeedNumber = mov.desiredMoveSpeed;  

    }
    
}
