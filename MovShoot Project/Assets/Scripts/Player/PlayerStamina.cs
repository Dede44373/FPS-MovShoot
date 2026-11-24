using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 10;
    public float currentStamina;
    public float useStamina;

    public float grappleCost;
    public float jumpCost;
    public float dashCost;

    [SerializeField] private PlayerGrapple pg;
    [SerializeField] private PlayerMovement pm;


    private void OnEnable()
    {
        currentStamina = maxStamina;
    }

    public void calculateStamina()
    {

    }

    public void StaminaDrain()
    {
        if (currentStamina <= useStamina)
        {
            currentStamina -= useStamina;
        }
        else
        {
            return;
        }
    }
}
