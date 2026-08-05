using TMPro;
using UnityEngine;

public class MoneyHUD : MonoBehaviour
{
    public static MoneyHUD instance;

    //Player money
    public float totalMoney;
    public TextMeshProUGUI moneyText;

    //Shop Money


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        moneyText = UIManager.instance.moneyText;
    }


    public void AddMoney(int value)
    {
        if (moneyText == null)
        {
            moneyText = UIManager.instance.moneyText;
        }

        totalMoney += value;
        moneyText.SetText("Money: {0}", totalMoney);

    }

}
