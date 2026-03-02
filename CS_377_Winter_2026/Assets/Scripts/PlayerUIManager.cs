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
        int currentRoundScoreReq = 0;
        switch (GameStateManager.instance._currentRound) 
        {
            case GameStateManager.RoundNumber.One:
                currentRoundScoreReq = GameStateManager.instance.roundOneScoreRequirement;
                break;
            case GameStateManager.RoundNumber.Two:
                currentRoundScoreReq = GameStateManager.instance.roundTwoScoreRequirement;
                break;
            case GameStateManager.RoundNumber.Three:
                currentRoundScoreReq = GameStateManager.instance.roundThreeScoreRequirement;
                break;
        }
        if (playerHandler != null)
        {
            HealthBar.value = playerHandler.playerHealth;
            PointsText.text = "Points: " + playerHandler.playerCurrentRoundScore.ToString() + " / " + currentRoundScoreReq.ToString();
            CheeseText.text = "Cheese: " + playerHandler.playerCurrentHoldingCheeses.Count.ToString();
        }
    }
}
