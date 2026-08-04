using TMPro;
using UnityEngine;

public class MoneyHUD : MonoBehaviour
{
    public float totalMoney;
    public TextMeshProUGUI moneyText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {

    }


    // Update is called once per frame

    void Update()
    {
        //Sets the textmeshpro object (only works with TextMeshProUGUI) to the SetText() function. the "" is for string and {0:2} is the numbers you want to change.
        //First being the Ints and the 2nd number controls how many decimal points you want. the final variable is the reference to what float you actually want to see. 
        moneyText.SetText("Money: {0}", totalMoney);
    }

    private void setText()
    {
        moneyText.text = totalMoney.ToString();

    }

}
