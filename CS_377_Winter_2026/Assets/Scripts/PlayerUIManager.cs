using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerUIManager : MonoBehaviour
{
    public PlayerHandler playerHandler;

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
