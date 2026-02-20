using System.Collections;
using System.Collections.Generic;
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

    public GameState _gameState;
    public RoundNumber _currentRound;

    [HideInInspector] public bool waitingForPlayersToJoin = false;

    public float countdownTime = 3.0f;
    public float roundTime = 90.0f;
    public float intermissionTime = 4.0f;

    public int roundOneScoreRequirement = 50;
    public int roundTwoScoreRequirement = 100;
    public int roundThreeScoreRequirement = 150;

    [HideInInspector] public bool itemsSpawning;
    public float itemSpawnCooldown = 7.5f;
    public List<Transform> possibleItemSpawnLocations;     // first transform in the list will spawn at the start of the round
    public List<GameObject> possibleItemSpawnObjects;
    private Dictionary<Transform, GameObject> possibleItemSpawnDictionary = new Dictionary<Transform, GameObject>();

    [HideInInspector] public Transform player1GameplaySpawnPosition;
    [HideInInspector] public Transform player2GameplaySpawnPosition;

    private Coroutine itemSpawningCoroutine;

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

        float _roundTime = roundTime;

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

    private IEnumerator itemSpawning()
    {
        yield return null;

        GameObject firstItem = Instantiate(possibleItemSpawnObjects[Random.Range(0, possibleItemSpawnObjects.Count)], possibleItemSpawnLocations[0].position, Quaternion.identity);    // first transform will be the first spawn

        possibleItemSpawnDictionary[possibleItemSpawnLocations[0]] = firstItem;

        while (itemsSpawning)
        {
            yield return new WaitForSeconds(itemSpawnCooldown);

            while (true)
            {
                int newSpawnIndex = Random.Range(0, possibleItemSpawnLocations.Count);
                GameObject randomObject = possibleItemSpawnObjects[Random.Range(0, possibleItemSpawnObjects.Count)];

                if (possibleItemSpawnDictionary.ContainsKey(possibleItemSpawnLocations[newSpawnIndex]))
                {

                    if (possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]] != null && possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]].GetComponent<IItem>()._ItemState == IItem.ItemState.NotCollected)
                    {
                        yield return null;
                        continue;
                    }
                    possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]] = Instantiate(randomObject, 
                                                                                                         possibleItemSpawnLocations[newSpawnIndex].position,
                                                                                                         randomObject.transform.rotation);
                    break;
                }
                else
                {
                    possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]] = Instantiate(randomObject,
                                                                                                         possibleItemSpawnLocations[newSpawnIndex].position,
                                                                                                         randomObject.transform.rotation);
                    break;
                }
            }
        }
    }

    private IEnumerator StartPreRoundCountdown()
    {
        yield return null;

        float _countdownTime = countdownTime;

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
}
