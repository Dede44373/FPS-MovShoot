using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TextMeshProUGUI moneyText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
}
