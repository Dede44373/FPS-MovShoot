using TMPro;
using UnityEngine;

public class CostManager : MonoBehaviour
{
    public MoneyHUD manager;
    public LevelUpManager levelManager;
    public TextMeshProUGUI costText;

    private void OnEnable()
    {
        costText.SetText("Cost: {0}", levelManager.levelUpCost);
    }

    public void SetText()
    {
        costText.SetText("Cost: {0}", levelManager.levelUpCost);
    }
}
