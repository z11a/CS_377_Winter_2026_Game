using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    public enum GameState
    {
        mainMenu,
        pauseMenu,
        loadingScreen,
        inGame,
        intermission // in between rounds / before the first round starts.
    }
    public enum RoundNumber
    {
        One,
        Two,
        Three
    }

    [Header("Game State Tracking")]
    public GameState _gameState;
    public RoundNumber _currentRound;

    [Header("Timing")]
    public float countdownLength = 3.0f;
    public float roundLength = 90.0f;
    public float intermissionLength = 4.0f;

    [Header("Scoring")]
    public int roundOneScoreRequirement = 50;
    public int roundTwoScoreRequirement = 100;
    public int roundThreeScoreRequirement = 150;

    [Header("Item Spawning")]
    public float itemSpawnCooldown = 7.5f;
    public float uncommonItemSpawnChance = 45.0f;
    public float rareItemSpawnChance = 15.0f;
    public List<Transform> possibleItemSpawnLocations;     // first transform in the list will spawn at the start of the round, this list is set by the "GameplayReferences" gameObject in each round scene.
    public List<GameObject> commonItems;
    public List<GameObject> uncommonItems;
    public List<GameObject> rareItems;
    [HideInInspector] public Dictionary<Vector3, GameObject> itemSpawnDictionary = new Dictionary<Vector3, GameObject>();
    [HideInInspector] public bool itemsSpawning;
    private Coroutine itemSpawningCoroutine;

    [HideInInspector] public bool waitingForPlayersToJoin = false;
    [HideInInspector] public Transform player1GameplaySpawnPosition;
    [HideInInspector] public Transform player2GameplaySpawnPosition;

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
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameplayScene"))    // this is for when we don't start the game from the MainMenu scene.
        {
            _gameState = GameState.inGame;
            itemsSpawning = true;
            itemSpawningCoroutine = StartCoroutine(itemSpawning());
            InputManager.instance.player1Input.SwitchCurrentActionMap("Player");
            InputManager.instance.player2Input.SwitchCurrentActionMap("Player");
        }
        else
        {
            _gameState = GameState.mainMenu;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (itemSpawningCoroutine != null && _gameState != GameState.inGame)
        {
            StopCoroutine(itemSpawningCoroutine);
        }
    }
    private IEnumerator StartRoundTimer()
    {
        yield return null;

        float _roundTime = roundLength;

        while (true)
        {
            yield return new WaitForSeconds(1.0f);

            _roundTime -= 1.0f;

            if (_roundTime <= 0.0f)
            {
                Debug.Log("Round over.");
                _gameState = GameState.intermission;
                break;
            }
        }
        yield return null;
    }

    private IEnumerator StartPreRoundCountdown()
    {
        yield return null;

        float _countdownTime = countdownLength;

        Debug.Log("Countdown started.");

        while (true)
        {
            Debug.Log("Countdown: " + _countdownTime);

            yield return new WaitForSeconds(1.0f);

            _countdownTime -= 1.0f;

            if (_countdownTime <= 0.0f)
            {
                Debug.Log("Countdown ended.");
                _gameState = GameState.inGame;
                InputManager.instance.player1Input.SwitchCurrentActionMap("Player");
                InputManager.instance.player2Input.SwitchCurrentActionMap("Player");
                itemsSpawning = true;
                StartCoroutine(itemSpawning());
                break;
            }
        }
        yield return null;
    }

    private void GameplaySceneSetup()
    {
        InputManager.instance.player1Input.GetComponent<Rigidbody>().position = player1GameplaySpawnPosition.position;
        InputManager.instance.player1Input.GetComponent<PlayerHandler>().currentSpawnPosition = player1GameplaySpawnPosition;
        InputManager.instance.player1Input.GetComponent<PlayerHandler>().playerCurrentHoldingCheeses = new List<GameObject>();
        InputManager.instance.player1Input.GetComponent<PlayerHandler>().playerCurrentRoundScore = 0;

        InputManager.instance.player2Input.GetComponent<Rigidbody>().position = player2GameplaySpawnPosition.position;
        InputManager.instance.player2Input.GetComponent<PlayerHandler>().currentSpawnPosition = player2GameplaySpawnPosition;
        InputManager.instance.player2Input.GetComponent<PlayerHandler>().playerCurrentHoldingCheeses = new List<GameObject>();
        InputManager.instance.player2Input.GetComponent<PlayerHandler>().playerCurrentRoundScore = 0;

        UIManager.instance.DeactivateLoadingScreen();
        UIManager.instance.GameplayUI.SetActive(true);
        instance.StartCoroutine(StartPreRoundCountdown());
    }
    public IEnumerator LoadGameplaySceneAsync(RoundNumber roundNumber)
    {
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

        _gameState = GameState.loadingScreen;
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

                SceneManager.MoveGameObjectToScene(InputManager.instance.player1Input.gameObject, SceneManager.GetSceneByBuildIndex(sceneIndex));
                SceneManager.MoveGameObjectToScene(InputManager.instance.player2Input.gameObject, SceneManager.GetSceneByBuildIndex(sceneIndex));
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                var refs = GameplaySceneReferences.instance;
                player1GameplaySpawnPosition = refs.player1Spawn;
                player2GameplaySpawnPosition = refs.player2Spawn;
                possibleItemSpawnLocations = refs.itemSpawnLocations;

                GameplaySceneSetup();
            }

            yield return null;
        }
    }
    private IEnumerator itemSpawning()
    {
        itemSpawnDictionary.Clear();
        // setup itemSpawnDictionary
        for (int i = 0; i < possibleItemSpawnLocations.Count; i++)
        {
            itemSpawnDictionary[possibleItemSpawnLocations[i].position] = null;
        }

        // spawn random common item at first location
        GameObject firstItem = Instantiate(commonItems[Random.Range(0, commonItems.Count)], possibleItemSpawnLocations[0].position, Quaternion.identity);
        itemSpawnDictionary[possibleItemSpawnLocations[0].position] = firstItem;

        while (itemsSpawning)
        {
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

            while (true)
            {
                int newSpawnIndex = Random.Range(0, possibleItemSpawnLocations.Count);
                GameObject randomObject = ChooseRandomItem();

                if (itemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex].position] == null)
                {
                    itemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex].position] = Instantiate(randomObject,
                                                                                                 possibleItemSpawnLocations[newSpawnIndex].position,
                                                                                                 randomObject.transform.rotation);
                    break;
                }
                yield return null;
            }
        }
    }

    private GameObject ChooseRandomItem()
    {
        float randomValue = Random.Range(0.0f, 100f);
        GameObject randomItem = null;

        if (randomValue < rareItemSpawnChance)
        {
            randomItem = rareItems[Random.Range(0, rareItems.Count)];
        }
        else if (randomValue < uncommonItemSpawnChance)
        {
            randomItem = uncommonItems[Random.Range(0, rareItems.Count)];
        }
        else
        {
            randomItem = commonItems[Random.Range(0, commonItems.Count)];
        }

        return randomItem;
    }
}
