using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerUIManager : MonoBehaviour
{
    [Header("Player Information")]
    public PlayerHandler playerHandler;

    [Header("UI Elements")]
    public Slider HealthBar;
    public TextMeshProUGUI PointsText;
    public TextMeshProUGUI CheeseText;

    void Update()
    {
        if (playerHandler != null)
        {
            HealthBar.value = playerHandler.playerHealth;
            PointsText.text = "Points: " + playerHandler.playerCurrentRoundScore.ToString();
            CheeseText.text = "Cheese: " + playerHandler.playerCurrentHoldingCheeses.Count.ToString();
        }
    }
}
