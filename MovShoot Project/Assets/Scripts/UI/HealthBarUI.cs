using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class HealthBarUI : MonoBehaviour
{
    //public float Health, MaxHealth, width, height;

    //[SerializeField]
    //private RectTransform healthBar;

    //public void SetMaxHealth(float maxHealth)
    //{
    //    MaxHealth = maxHealth;
    //}

    //public void setHealth(float health)
    //{
    //    Health = health;
    //    float newWidth = (Health / MaxHealth) * width;
    //    healthBar.sizeDelta = new Vector2(newWidth, height);
    //}

    public float Health;
    public float MaxHealth;
    public float duration;
    private Slider HealthBar;

    private void Awake()
    {
        HealthBar = GetComponent<Slider>();
    }

    public void setHealth(float health)
    {
        float OldHealth = HealthBar.value;
        HealthBar.DOValue(health / MaxHealth, duration);

    }
}
