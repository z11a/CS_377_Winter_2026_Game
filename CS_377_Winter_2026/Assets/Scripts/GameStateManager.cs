using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    public enum GameState
    {
        //mainMenu,
        //pauseMenu,
        //loadingScreen,
        //intermission, // in between rounds / before the first round starts.
        notInGame,
        isLoading,
        inGame,
        isPaused,
        inCharacterCustomization
    }
    public enum RoundNumber
    {
        One,
        Two,
        Three
    }
    public enum RoundState
    {
        preRound,
        inRound,
        postRound
    }

    [Header("Game State Tracking")]
    public GameState _gameState;
    public RoundNumber _currentRound;
    public RoundState _currentRoundState;

    [Header("Timing")]
    public float countdownLength = 3.0f;
    public float roundLength = 120.0f;
    [HideInInspector] public float currentRoundTime = 120.0f;
    public float intermissionLength = 4.0f;
    private IEnumerator roundTimerCoroutine;

    [Header("Scoring")]
    public int roundOneScoreRequirement = 50;
    public int roundTwoScoreRequirement = 100;
    public int roundThreeScoreRequirement = 150;
    [SerializeField] private AudioClip winSFX;
    private IEnumerator activateIntermissionCoroutine;

    [Header("Item Spawning")]
    public float itemSpawnCooldown = 3.0f;
    public float uncommonItemSpawnChance = 40.0f;
    public float rareItemSpawnChance = 15.0f;
    public List<Transform> possibleItemSpawnLocations;     // first transform in the list will spawn at the start of the round, this list is set by the "GameplayReferences" gameObject in each round scene.
    public List<GameObject> commonItems;
    public List<GameObject> uncommonItems;
    public List<GameObject> rareItems;
    [HideInInspector] public Dictionary<Vector3, GameObject> itemSpawnDictionary = new Dictionary<Vector3, GameObject>();
    [HideInInspector] public bool itemsSpawning;
    private IEnumerator itemSpawningCoroutine;
    [HideInInspector] public int cheesePity = 0;
    [HideInInspector] public int uncommonPity = 0;

    [HideInInspector] public bool waitingForPlayersToJoin = false;
    [HideInInspector] public List<Transform> playerGameplaySpawnPositions;
    //[HideInInspector] public Transform player1GameplaySpawnPosition;
    //[HideInInspector] public Transform player2GameplaySpawnPosition;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed extra GameStateManager");
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
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("roundOne"))    // this is for when we don't start the game from the MainMenu scene.
        {
            _gameState = GameState.inGame;
            itemsSpawning = true;
            itemSpawningCoroutine = itemSpawning();
            StartCoroutine(itemSpawningCoroutine);
        }
        else
        {
            _gameState = GameState.notInGame;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void PlayerWonRound(PlayerHandler playerHandler)
    {
        if (winSFX != null && AudioManager.instance.audioSource != null)
        {
            AudioManager.instance.audioSource.PlayOneShot(winSFX);
        }

        if (itemSpawningCoroutine != null)
        {
            StopCoroutine(itemSpawningCoroutine);
        }
        if (roundTimerCoroutine != null)
        {
            StopCoroutine(roundTimerCoroutine);
        }

        playerHandler.playerTotalRoundScore++;
        UIManager.instance.ActivateRoundWinText(playerHandler);

        if (playerHandler.playerTotalRoundScore >= 2)
        {
            Debug.Log(playerHandler.playerNumber + " won the game!");
            UIManager.instance.roundWinText.text = $"{playerHandler.playerNumber} wins the game!";
            UIManager.instance.roundWinText.gameObject.SetActive(true);
            GameStateManager.instance._gameState = GameStateManager.GameState.notInGame;
            Time.timeScale = 0.0f;
        }
        else
        {
            activateIntermissionCoroutine = ActivateIntermission(GameStateManager.instance._currentRound + 1);
            StartCoroutine(activateIntermissionCoroutine);
        }

        return;
    }
    private IEnumerator ActivateIntermission(GameStateManager.RoundNumber nextRoundNumber)
    {
        //.instance._gameState = GameStateManager.GameState.intermission;

        yield return new WaitForSeconds(GameStateManager.instance.intermissionLength);

        StartCoroutine(GameStateManager.instance.LoadGameplaySceneAsync(nextRoundNumber));
    }

    private IEnumerator StartRoundTimer()
    {
        _gameState = GameState.inGame;
        _currentRoundState = RoundState.inRound;

        currentRoundTime = roundLength;

        int minutes = (int)(GameStateManager.instance.currentRoundTime / 60);
        int seconds = (int)(GameStateManager.instance.currentRoundTime % 60);
        UIManager.instance.roundTimerText.text = $"{minutes}:{seconds:D2}";

        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            currentRoundTime -= 1.0f;

            if (currentRoundTime <= 0.0f)
            {
                Debug.Log("Round over.");

                _currentRoundState = RoundState.postRound;

                // check for cheese tie here, if they have the same cheeses maybe we check who hit more attacks
                PlayerHandler playerOneHandler = InputManager.instance.PlayerInputs[0].GetComponent<PlayerHandler>();
                PlayerHandler playerTwoHandler = InputManager.instance.PlayerInputs[1].GetComponent<PlayerHandler>();

                if (playerOneHandler.playerCurrentRoundScore > playerTwoHandler.playerCurrentRoundScore)
                {
                    PlayerWonRound(playerOneHandler);
                }
                else if (playerTwoHandler.playerCurrentRoundScore > playerOneHandler.playerCurrentRoundScore)
                {
                    PlayerWonRound(playerTwoHandler);
                }
                else if (playerOneHandler.playerCurrentRoundScore == playerTwoHandler.playerCurrentRoundScore)
                {

                }
                yield break;
            }
        }
    }

    private IEnumerator StartPreRoundCountdown()
    {
        _currentRoundState = RoundState.preRound;

        float _countdownTime = countdownLength;

        Color fullAlpha = UIManager.instance.preRoundTimerText.color;
        fullAlpha.a = 1.0f;
        UIManager.instance.preRoundTimerText.color = fullAlpha;
        UIManager.instance.preRoundTimerText.gameObject.SetActive(true);

        Debug.Log("Countdown started.");

        while (true)
        {
            //yield return null;

            Debug.Log("Countdown: " + _countdownTime);

            UIManager.instance.preRoundTimerText.text = _countdownTime.ToString();

            yield return new WaitForSeconds(1.0f);

            _countdownTime -= 1.0f;

            if (_countdownTime <= 0.0f)
            {
                Debug.Log("Countdown ended.");

                foreach (PlayerInput playerInput in InputManager.instance.PlayerInputs)
                {
                    playerInput.SwitchCurrentActionMap("Player");
                    playerInput.GetComponent<PlayerHandler>().playerCanMove = true;
                }
                //InputManager.instance.PlayerInputs[0].SwitchCurrentActionMap("Player");
                //InputManager.instance.PlayerInputs[1].SwitchCurrentActionMap("Player");

                itemsSpawning = true;
                itemSpawningCoroutine = itemSpawning();
                StartCoroutine(itemSpawningCoroutine);

                roundTimerCoroutine = StartRoundTimer();
                StartCoroutine(roundTimerCoroutine);

                UIManager.instance.preRoundTimerText.text = "Play!";

                // fade out preRoundTimerText
                float elapsedTime = 0.0f;
                float duration = 2.0f;
                Color col = UIManager.instance.preRoundTimerText.color;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    col.a = 1.0f - Mathf.Clamp01(elapsedTime / duration);
                    UIManager.instance.preRoundTimerText.color = col;
                    yield return null;
                }
                UIManager.instance.preRoundTimerText.gameObject.SetActive(false);
                yield break;
            }
        }
    }

    private void GameplaySceneSetup()
    {
        for (int i = 0; i < InputManager.instance.PlayerInputs.Count; i++) 
        {
            PlayerInput playerInput = InputManager.instance.PlayerInputs[i];
            playerInput.GetComponent<Rigidbody>().position = playerGameplaySpawnPositions[i].position;
            playerInput.GetComponent<PlayerHandler>().currentSpawnPosition = playerGameplaySpawnPositions[i];
            playerInput.GetComponent<PlayerHandler>().playerCurrentHoldingCheeses = new List<GameObject>();
            playerInput.GetComponent<PlayerHandler>().playerCurrentRoundScore = 0;
            playerInput.GetComponent<PlayerHandler>().ResetPlayerValues();
        }
        //InputManager.instance.player1Input.GetComponent<Rigidbody>().position = player1GameplaySpawnPosition.position;
        //InputManager.instance.player1Input.GetComponent<PlayerHandler>().currentSpawnPosition = player1GameplaySpawnPosition;
        //InputManager.instance.player1Input.GetComponent<PlayerHandler>().playerCurrentHoldingCheeses = new List<GameObject>();
        //InputManager.instance.player1Input.GetComponent<PlayerHandler>().playerCurrentRoundScore = 0;
        //InputManager.instance.player1Input.GetComponent<PlayerHandler>().ResetPlayerValues();

        //InputManager.instance.player2Input.GetComponent<Rigidbody>().position = player2GameplaySpawnPosition.position;
        //InputManager.instance.player2Input.GetComponent<PlayerHandler>().currentSpawnPosition = player2GameplaySpawnPosition;
        //InputManager.instance.player2Input.GetComponent<PlayerHandler>().playerCurrentHoldingCheeses = new List<GameObject>();
        //InputManager.instance.player2Input.GetComponent<PlayerHandler>().playerCurrentRoundScore = 0;
        //InputManager.instance.player2Input.GetComponent<PlayerHandler>().ResetPlayerValues();

        currentRoundTime = roundLength;
        int minutes = (int)(GameStateManager.instance.currentRoundTime / 60);
        int seconds = (int)(GameStateManager.instance.currentRoundTime % 60);
        UIManager.instance.roundTimerText.text = $"{minutes}:{seconds:D2}";

        UIManager.instance.GameplayUI.SetActive(true);
        UIManager.instance.DeactivateLoadingScreen();
        StartCoroutine(StartPreRoundCountdown());
    }
    public IEnumerator LoadGameplaySceneAsync(RoundNumber roundNumber)
    {
        _gameState = GameState.isLoading;

        foreach (PlayerInput playerInput in InputManager.instance.PlayerInputs)
        {
            playerInput.StopAllCoroutines();
            //playerInput.SwitchCurrentActionMap("UI");
            playerInput.GetComponent<PlayerHandler>().playerCanMove = false;
        }

        //InputManager.instance.player1Input.StopAllCoroutines();
        //InputManager.instance.player2Input.StopAllCoroutines();
        //InputManager.instance.player1Input.SwitchCurrentActionMap("UI");
        //InputManager.instance.player2Input.SwitchCurrentActionMap("UI");

        UIManager.instance.ActivateLoadingScreen();
        yield return null;

        int sceneIndex = 1; 

        switch (roundNumber)
        {
            case RoundNumber.One:
                sceneIndex = 1;
                _currentRound = RoundNumber.One;
                break;
            case RoundNumber.Two:
                sceneIndex = 2;
                _currentRound = RoundNumber.Two;
                break;
            case RoundNumber.Three:
                sceneIndex = 3;
                _currentRound = RoundNumber.Three;
                break;
        }

        _gameState = GameState.notInGame;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            Debug.Log("Progress: " + asyncLoad.progress * 100 + "%");

            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("Loaded! Starting game in 2 seconds...");

                yield return new WaitForSeconds(2.0f);
                asyncLoad.allowSceneActivation = true;

                yield return null;

                foreach (PlayerInput playerInput in InputManager.instance.PlayerInputs)
                {
                    SceneManager.MoveGameObjectToScene(playerInput.gameObject, SceneManager.GetSceneByBuildIndex(sceneIndex));
                }

                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                LightProbes.Tetrahedralize();

                var refs = GameplaySceneReferences.instance;
                //player1GameplaySpawnPosition = refs.player1Spawn;
                //player2GameplaySpawnPosition = refs.player2Spawn;
                playerGameplaySpawnPositions = refs.playerSpawnLocations;
                possibleItemSpawnLocations = refs.itemSpawnLocations;

                GameplaySceneSetup();
            }

            yield return null;
        }
    }
    private IEnumerator itemSpawning()
    {
        // setup itemSpawnDictionary
        itemSpawnDictionary.Clear();
        for (int i = 0; i < possibleItemSpawnLocations.Count; i++)
        {
            itemSpawnDictionary[possibleItemSpawnLocations[i].position] = null;
        }

        // spawn random common item at first location
        GameObject firstItem = Instantiate(commonItems[Random.Range(0, commonItems.Count)], possibleItemSpawnLocations[0].position, Quaternion.identity);
        itemSpawnDictionary[possibleItemSpawnLocations[0].position] = firstItem;

        while (itemsSpawning)
        {
            yield return null;

            bool allLocationsFull = itemSpawnDictionary.Values.All(value => value != null);

            if (allLocationsFull)
            {
                yield return new WaitForSeconds(4.0f);
                continue;
            }
            else
            {
                yield return new WaitForSeconds(itemSpawnCooldown);
            }

            var emptyLocations = itemSpawnDictionary
                                .Where(kvp => kvp.Value == null)
                                .Select(kvp => kvp.Key)
                                .ToList();

            if (emptyLocations.Count > 0)
            {
                Vector3 newSpawnIndex = emptyLocations[Random.Range(0, emptyLocations.Count)];
                GameObject randomObject = ChooseRandomItem();

                itemSpawnDictionary[newSpawnIndex] = Instantiate(randomObject, newSpawnIndex, randomObject.transform.rotation);
            }

        }
    }

    private void printDictionary()
    {
        Debug.Log("itemSpawnDictionary:");
        foreach (KeyValuePair<Vector3, GameObject> entry in itemSpawnDictionary)
        {
            Debug.Log("Key: " + entry.Key + ", Value: " + entry.Value);
        }
    }

    private GameObject ChooseRandomItem()
    {
        float randomValue = Random.Range(0.0f, 100f);
        GameObject randomItem = null;
        //Debug.Log(uncommonPity);
        if (cheesePity >= 3)
        {
            randomItem = commonItems[Random.Range(0, commonItems.Count)];
            cheesePity = 0;
            return randomItem;
        }
        if (uncommonPity >= 3)
        {
            randomItem = uncommonItems[Random.Range(0, uncommonItems.Count)];
            uncommonPity = 0;
            return randomItem;
        }

        if (randomValue < rareItemSpawnChance)
        {
            randomItem = rareItems[Random.Range(0, rareItems.Count)];
            cheesePity++;
            uncommonPity = 0;
        }
        else if (randomValue < uncommonItemSpawnChance)
        {
            randomItem = uncommonItems[Random.Range(0, uncommonItems.Count)];
            cheesePity++;
            uncommonPity = 0;
        }
        else
        {
            randomItem = commonItems[Random.Range(0, commonItems.Count)];
            cheesePity = 0;
            uncommonPity++;
        }

        return randomItem;
    }
}
