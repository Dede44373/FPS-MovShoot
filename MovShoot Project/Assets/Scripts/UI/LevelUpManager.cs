using Unity.VisualScripting;
using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager instance;
    public CostManager costManager;
    public PlayerAttack pAttack;
    public PlayerHealth pHealth;
    public float levelUpCost;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            return;
        }
    }


    public void LevelHealth()
    {
        if (levelUpCost <= MoneyHUD.instance.totalMoney)
        {
            MoneyHUD.instance.RemoveMoney(levelUpCost);
            pHealth.IncreaseMaxHealth();
            IncreaseCosts();
            costManager.SetText();
        }
        else
        {
            NotEnoughCurrency();
        }
    }
    public void LevelDamage()
    {
        if (levelUpCost <= MoneyHUD.instance.totalMoney)
        {
            MoneyHUD.instance.RemoveMoney(levelUpCost);
            pAttack.IncreaseDamage();
            IncreaseCosts();
            costManager.SetText();
        }
        else
        {
            NotEnoughCurrency();
        }
    }

    public void LeveJuice()
    {
        if (levelUpCost <= MoneyHUD.instance.totalMoney)
        {
            MoneyHUD.instance.RemoveMoney(levelUpCost);
            pHealth.IncreaseMaxHealth();
            IncreaseCosts();
            costManager.SetText();
        }
        else
        {
            NotEnoughCurrency();
        }
    }

    void NotEnoughCurrency()
    {
        print("Insufficient currency");
        SoundManager.PlaySound(SoundType.Empty);
    }

    public void IncreaseCosts()
    {
        Mathf.RoundToInt(levelUpCost += 5f);
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        
    }
}
