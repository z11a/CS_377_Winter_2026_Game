using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerUIManager : MonoBehaviour
{
    [Header("Player Information")]
    public PlayerHandler playerHandler;

    [Header("UI Elements")]
    public RawImage PlayerAvatar;
    public TextMeshProUGUI RoundWins;
    public TextMeshProUGUI PointsText;
    public TextMeshProUGUI CheeseText;
    public Slider HealthBar;
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
            RoundWins.text = "Round wins: " + playerHandler.playerTotalRoundScore.ToString() + " / 3";
            HealthBar.value = playerHandler.playerHealth;
            PointsText.text = "Points: " + playerHandler.playerCurrentRoundScore.ToString() + " / " + currentRoundScoreReq.ToString();
            CheeseText.text = "Cheese: " + playerHandler.playerCurrentHoldingCheeses.Count.ToString();

            if (playerHandler._playerState == PlayerHandler.PlayerState.Dead)
            {
                Color transparentAlpha = PlayerAvatar.color;
                transparentAlpha.a = 0.7f;
                PlayerAvatar.color = transparentAlpha;
            }
            else
            {
                Color fullAlpha = PlayerAvatar.color;
                fullAlpha.a = 1.0f;
                PlayerAvatar.color = fullAlpha;
            }
        }
    }
}
