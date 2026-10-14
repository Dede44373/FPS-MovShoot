using UnityEngine;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager instance;
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


    public void LevelUp()
    {
        if (levelUpCost <= MoneyHUD.instance.totalMoney)
        {
            MoneyHUD.instance.totalMoney -= levelUpCost;
            pHealth.maxHealth ++;
            pHealth.currentHealth = pHealth.maxHealth;
            IncreaseCosts();
        }
        else
        {
            print("Insufficient currency");
        }
    }

    public void IncreaseCosts()
    {
        Mathf.RoundToInt(levelUpCost *= 1.5f);
        
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        
    }
}
