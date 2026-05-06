using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerUIHandler : MonoBehaviour
{
    [Header("Player Information")]
    [SerializeField] private PlayerHandler playerHandler;
    private GameObject PlayerUITemplate;

    [Header("UI Elements")]
    public RawImage PlayerAvatar;
    public TextMeshProUGUI RoundWinsText;
    public TextMeshProUGUI PointsText;
    public TextMeshProUGUI CheeseText;
    public Slider HealthBar;

    void Start()
    {
        playerHandler = GetComponent<PlayerHandler>();

        switch (playerHandler.playerNumber)
        {
            case PlayerHandler.PlayerNumber.PlayerOne:
                PlayerUITemplate = UIManager.instance.PlayerOneUI;
                break;
            case PlayerHandler.PlayerNumber.PlayerTwo:
                PlayerUITemplate = UIManager.instance.PlayerTwoUI;
                break;
        }

        PlayerUIReferences playerUIReferences = PlayerUITemplate.GetComponent<PlayerUIReferences>();
        PlayerAvatar = playerUIReferences.PlayerAvatar;
        RoundWinsText = playerUIReferences.RoundWins;
        RoundWinsText.text = "Round wins: 0 / 2";
        PointsText = playerUIReferences.PointsText;
        PointsText.text = "Points: 0 / " + GameStateManager.instance.roundOneScoreRequirement.ToString();
        CheeseText = playerUIReferences.CheeseText;
        CheeseText.text = "Cheese: 0";
        HealthBar = playerUIReferences.HealthBar;

    }

    public void RoundWinUpdate(int roundsWon)
    {
        RoundWinsText.text = "Round wins: " + roundsWon.ToString() + " / 2";
    }
    public void HealthUpdate(float health)
    {
        HealthBar.value = health;
    }

    public void PointsUpdate(int points)
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

        PointsText.text = "Points: " + points.ToString() + " / " + currentRoundScoreReq.ToString();
    }

    public void CheeseUpdate(int cheeseCount)
    {
        CheeseText.text = "Cheese: " + cheeseCount.ToString();
    }

    public void DeathUpdate()
    {
        Color transparentAlpha = PlayerAvatar.color;
        transparentAlpha.a = 0.7f;
        PlayerAvatar.color = transparentAlpha;
    }

    public void RespawnUpdate()
    {
        Color fullAlpha = PlayerAvatar.color;
        fullAlpha.a = 1.0f;
        PlayerAvatar.color = fullAlpha;
    }

    public void StaminaUpdate()
    {

    }
}
