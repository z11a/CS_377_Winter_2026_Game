using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Start Menu")]
    public GameObject StartMenuUI;
    public Button startMenuButton;
    public Button startGameButton;

    [Header("Gameplay")]
    public GameObject GameplayUI;
    public TextMeshProUGUI roundWinText;
    public TextMeshProUGUI roundTimerText;

    [Header("Other")]
    public RawImage loadingScreen;
    public float loadingScreenFadeDuration = 1.0f;
    
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed extra UIManager");
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startGameButton.gameObject.SetActive(false);
        startMenuButton.gameObject.SetActive(true);
        loadingScreen.gameObject.SetActive(false);
        StartMenuUI.SetActive(true);
        GameplayUI.SetActive(false);
        roundWinText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateManager.instance._gameState == GameStateManager.GameState.inGame)
        {
            int minutes = (int)(GameStateManager.instance.currentRoundTime / 60);
            int seconds = (int)(GameStateManager.instance.currentRoundTime % 60);
            roundTimerText.text = $"{minutes}:{seconds:D2}";
        }
    }

    public void ActivatePreRoundTimerCoroutine()
    {

    }

    public void OnStartButton()
    {
        Debug.Log("Start Menu button pressed.");

        startMenuButton.gameObject.SetActive(false);
        GameStateManager.instance.waitingForPlayersToJoin = true;
        InputManager.instance.playerInputManager.EnableJoining();
    }

    public void OnStartGameButton()
    {
        Debug.Log("Start Game button pressed.");

        startGameButton.gameObject.SetActive(false);
        StartCoroutine(GameStateManager.instance.LoadGameplaySceneAsync(GameStateManager.RoundNumber.One));
    }
    public void ActivateLoadingScreen()
    {
        loadingScreen.gameObject.SetActive(true);
        Color noAlpha = loadingScreen.color;
        noAlpha.a = 0.0f;
        loadingScreen.color = noAlpha;
        StartCoroutine(RawImageFadeIn(loadingScreen, loadingScreenFadeDuration));
    }
    public void DeactivateLoadingScreen()
    {
        StartMenuUI.SetActive(false);
        loadingScreen.gameObject.SetActive(true);
        Color fullAlpha = loadingScreen.color;
        fullAlpha.a = 1.0f;
        loadingScreen.color = fullAlpha;
        StartCoroutine(RawImageFadeOut(loadingScreen, loadingScreenFadeDuration));
    }
    private IEnumerator RawImageFadeIn(RawImage rawImage, float duration)
    {
        float elapsedTime = 0.0f;
        Color col = rawImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            col.a = Mathf.Clamp01(elapsedTime / duration);
            rawImage.color = col;
            yield return null;
        }
        DeactivateRoundWinText();
        yield break;
    }

    private IEnumerator RawImageFadeOut(RawImage rawImage, float duration)
    {
        float elapsedTime = 0.0f;
        Color col = rawImage.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            col.a = 1.0f - Mathf.Clamp01(elapsedTime / duration);
            rawImage.color = col;
            yield return null;
        }
        yield break;
    }

    public void ActivateRoundWinText(PlayerHandler playerHandler)
    {
        string playerNumberString = "";
        switch (playerHandler.playerNumber)
        {
            case PlayerHandler.PlayerNumber.PlayerOne:
                playerNumberString = "Player One";
                break;
            case PlayerHandler.PlayerNumber.PlayerTwo:
                playerNumberString = "Player Two";
                break;
        }
        roundWinText.text = playerNumberString + " wins Round " + GameStateManager.instance._currentRound.ToString() + "!";
        roundWinText.gameObject.SetActive(true);
    }

    public void DeactivateRoundWinText()
    {
        roundWinText.gameObject.SetActive(false);
    }
}
