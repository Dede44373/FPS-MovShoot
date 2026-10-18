using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerJuiceBlood : MonoBehaviour
{
    public int maxJuice = 5;
    public int currentJuice;
    public bool isThrowing;

    public int healingcost;
    public int healAmount;
    public int throwCost;

    public PlayerThrow pthrow;
    public PlayerHealth health;
    public UserInputs controls;
    public HealthBarUI juiceBar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentJuice = maxJuice;
    }

    private void OnEnable()
    {
        controls = UserInputManager.Instance.Controls;
        controls.Player.Healing.performed += HandleHealing;
        controls.Player.RangedAttack.performed += HandleThrow;
    }

    private void OnDisable()
    {
        controls.Player.Healing.performed -= HandleHealing;
        controls.Player.RangedAttack.performed -= HandleThrow;
    }
    private void HandleHealing(InputAction.CallbackContext ctx)
    {
        AttemptHealing(healingcost);
    }
    private void HandleThrow(InputAction.CallbackContext ctx)
    {
        AttemptThrowing(throwCost);
    }

    public void IncreaseCurrentJuice(int increase)
    {
        if (currentJuice < maxJuice)
        {
            currentJuice += increase;
            juiceBar.setHealth(currentJuice);
        }
        else
        {
            health.Heal(healAmount);
        }
    }
    public void AttemptThrowing(int usage)
    {
        if (usage <= currentJuice && pthrow.readyToThrow)
        {
            currentJuice -= usage;
            juiceBar.setHealth(currentJuice);
            pthrow.Throw();


        }
        else
        {
            InsufficientCurrency();
        }
    }

    public void AttemptHealing(int usage)
    {
        if (usage <= currentJuice)
        {
            currentJuice -= usage;
            health.Heal(healAmount);
            juiceBar.setHealth(currentJuice);
        }
        else
        {
            InsufficientCurrency();
        }
    }

    void InsufficientCurrency()
    {
        print("Insufficient currency");
        SoundManager.PlaySound(SoundType.Empty);
    }
    // Update is called once per frame
    void Update()
    {
        if (currentJuice >= 0)
        {
            
        }
    }
}
