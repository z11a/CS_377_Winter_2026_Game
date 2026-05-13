using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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
    public Slider StaminaBar;

    [HideInInspector] private IEnumerator staminaCoroutine;

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
        StaminaBar = playerUIReferences.StaminaBar;
        StaminaBar.gameObject.SetActive(false);
    }

    void Update()
    {
        Vector2 offset = new Vector2(0, -50);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(playerHandler.rb.position);
        Vector2 newScreenPosition = new Vector2(screenPosition.x, screenPosition.y) + offset;

        StaminaBar.GetComponent<RectTransform>().position = newScreenPosition;
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

    public void StartStaminaCooldown(float duration)
    {
        if (staminaCoroutine != null)
        {
            StopCoroutine(staminaCoroutine);
        }
        StaminaBar.gameObject.SetActive(true);
        staminaCoroutine = StaminaCooldown(duration);
        StartCoroutine(StaminaCooldown(duration));
    }

    private IEnumerator StaminaCooldown(float duration)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < duration)
        {
            yield return null;
            StaminaBar.value = elapsedTime / duration;
            elapsedTime += Time.deltaTime;
        }
        StaminaBar.value = 1.0f;
        StaminaBar.gameObject.SetActive(false);
        yield return null;
    }
}
