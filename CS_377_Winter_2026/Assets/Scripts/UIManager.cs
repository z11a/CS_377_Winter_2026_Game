using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using NUnit.Framework;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Start Menu")]
    public GameObject StartMenuUI;
    public Button startMenuButton;
    public Button startGameButton;
    public Button trainingAreaButton;
    public TextMeshProUGUI playerJoinTextPrefab;
    [HideInInspector] public List<TextMeshProUGUI> playerJoinTextList;

    [Header("Gameplay")]
    public GameObject GameplayUI;
    public TextMeshProUGUI roundWinText;
    public TextMeshProUGUI roundTimerText;
    public TextMeshProUGUI preRoundTimerText;

    [Header("Pause Menu")]
    public GameObject PauseMenuUI;

    [Header("Other")]
    public RawImage loadingScreen;
    public float loadingScreenFadeDuration = 1.0f;
    public Camera mainMenuCamera;
    
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
        InitialUISetup();
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

    public void InitialUISetup()
    {
        startGameButton.gameObject.SetActive(false);
        startMenuButton.gameObject.SetActive(true);
        loadingScreen.gameObject.SetActive(false);
        StartMenuUI.SetActive(true);
        GameplayUI.SetActive(false);
        PauseMenuUI.SetActive(false);
        roundWinText.gameObject.SetActive(false);

        // setup training area button to be faded and non-interactable until enabled
        trainingAreaButton.gameObject.SetActive(false);
        trainingAreaButton.interactable = false;
        Color transparent = trainingAreaButton.image.color;
        transparent.a = 0.3f;
        trainingAreaButton.image.color = transparent;
    }
    public void ActivateTrainingAreaButton()
    {
        trainingAreaButton.interactable = true;
        Color fullAlpha = trainingAreaButton.image.color;
        fullAlpha.a = 1.0f;
        trainingAreaButton.image.color = fullAlpha;
    }

    public void OnTrainingAreaButton()
    {
        StartCoroutine(LoadTrainingArea());
    }

    private IEnumerator LoadTrainingArea()
    {
        ActivateLoadingScreen();

        GameStateManager.instance._gameState = GameStateManager.GameState.isLoading;
        InputManager.instance.playerInputManager.DisableJoining();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TrainingArea", LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            Debug.Log("Progress: " + asyncLoad.progress * 100 + "%");

            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("Loaded! Switching scene in 2 seconds...");

                yield return new WaitForSeconds(2.0f);
                asyncLoad.allowSceneActivation = true;

                yield return null;

                PlayerInput playerOneInput = InputManager.instance.PlayerInputs[0];

                SceneManager.MoveGameObjectToScene(InputManager.instance.PlayerInputs[0].gameObject, SceneManager.GetSceneByName("TrainingArea"));

                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                LightProbes.Tetrahedralize();

                var refs = GameplaySceneReferences.instance;
                
                Transform playerSpawnPosition = refs.playerSpawnLocations[0];

                PlayerHandler playerOnePlayerHandler = playerOneInput.GetComponent<PlayerHandler>();
                playerOnePlayerHandler.rb.position = playerSpawnPosition.position;
                playerOnePlayerHandler.currentSpawnPosition = playerSpawnPosition;
                playerOnePlayerHandler.playerCanMove = true;
                playerOneInput.SwitchCurrentActionMap("Player");

                DeactivateLoadingScreen();
                GameStateManager.instance._gameState = GameStateManager.GameState.inGame;
            }
            
            yield return null;
        }
    }

    public void OnStartButton()
    {
        Debug.Log("Start Menu button pressed.");

        startMenuButton.gameObject.SetActive(false);
        trainingAreaButton.gameObject.SetActive(true);

        GameStateManager.instance.waitingForPlayersToJoin = true;
        GameStateManager.instance._gameState = GameStateManager.GameState.inCharacterCustomization;

        for (int i = 0; i < 2; i++)
        {
            Vector3 worldPos = InputManager.instance.playerStartSceneSpawnPositions[i].position;
            Vector3 screenPos = mainMenuCamera.WorldToScreenPoint(worldPos);

            TextMeshProUGUI newPlayerJoinText = Instantiate(playerJoinTextPrefab, screenPos, Quaternion.identity, StartMenuUI.transform);
            playerJoinTextList.Add(newPlayerJoinText);
        }

        InputManager.instance.playerInputManager.EnableJoining();
    }

    public void OnStartGameButton()
    {
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

    public void ActivatePauseScreen()
    {
        PauseMenuUI.SetActive(true);
    }

    public void DeactivatePauseScreen()
    {
        PauseMenuUI.SetActive(false);
    }

    

    public void OnQuitButton()
    {
        //InputManager.instance.StopAllCoroutines();

        //foreach (PlayerInput playerInput in InputManager.instance.PlayerInputs)
        //{
        //    Destroy(playerInput.gameObject);
        //}

        //InputManager.instance.PlayerInputs.Clear();
        //InputManager.instance.player1Joined = false;
        //InputManager.instance.player2Joined = false;

        //GameStateManager.instance.StopAllCoroutines();
        //GameStateManager.instance.waitingForPlayersToJoin = false;
        //GameStateManager.instance._gameState = GameStateManager.GameState.notInGame;
        //GameStateManager.instance.itemSpawnDictionary.Clear();

        foreach (PlayerInput playerInput in InputManager.instance.PlayerInputs)
        {
            Destroy(playerInput.gameObject);
        }
        Destroy(InputManager.instance.gameObject);
        Destroy(GameStateManager.instance.gameObject);
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("StartScene");
        Destroy(this.gameObject);
    }
}
