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

    public static GameState _gameState;
    public static bool waitingForPlayersToJoin = false;

    public static float countdownTime = 3.0f;
    public static float roundTime = 90.0f;

    public bool itemsSpawning;
    public float itemSpawnCooldown = 7.5f;
    public List<Transform> possibleItemSpawnLocations;     // first transform will be the first spawn
    public List<GameObject> possibleItemSpawnObjects;
    private Dictionary<Transform, GameObject> possibleItemSpawnDictionary = new Dictionary<Transform, GameObject>();

    [SerializeField] private Transform player1GameplaySpawnPosition; 
    [SerializeField] private Transform player2GameplaySpawnPosition;

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

    private static IEnumerator StartRoundTimer()
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

                if (possibleItemSpawnDictionary.ContainsKey(possibleItemSpawnLocations[newSpawnIndex]))
                {
                    if (possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]] != null && possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]].GetComponent<Item>().State == Item.ItemState.NotCollected)
                    {
                        yield return null;
                        continue;
                    }
                    possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]] = Instantiate(possibleItemSpawnObjects[Random.Range(0, possibleItemSpawnObjects.Count)], 
                                                                                                         possibleItemSpawnLocations[newSpawnIndex].position, 
                                                                                                         Quaternion.identity);
                    break;
                }
                else
                {
                    possibleItemSpawnDictionary[possibleItemSpawnLocations[newSpawnIndex]] = Instantiate(possibleItemSpawnObjects[Random.Range(0, possibleItemSpawnObjects.Count)],
                                                                                                         possibleItemSpawnLocations[newSpawnIndex].position,
                                                                                                         Quaternion.identity);
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
    private void gameplaySceneStart()
    {
        InputManager.instance.player1Input.gameObject.transform.position = new Vector3(-6.21835f, 0.7f, -3.8f);
        InputManager.instance.player2Input.gameObject.transform.position = new Vector3(6.21835f, 0.7f, -3.8f);

        instance.StartCoroutine(StartPreRoundCountdown());
    }
    public IEnumerator LoadGameplaySceneAsync()
    {
        yield return null;

        _gameState = GameState.loadingScreen;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/GameplayScene", LoadSceneMode.Additive);
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

                SceneManager.MoveGameObjectToScene(InputManager.instance.player1Input.gameObject, SceneManager.GetSceneByName("GameplayScene"));
                SceneManager.MoveGameObjectToScene(InputManager.instance.player2Input.gameObject, SceneManager.GetSceneByName("GameplayScene"));
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

                var refs = GameplaySceneReferences.Instance;
                player1GameplaySpawnPosition = refs.player1Spawn;
                player1GameplaySpawnPosition = refs.player2Spawn;
                possibleItemSpawnLocations = refs.itemSpawnLocations;

                gameplaySceneStart();
            }

            yield return null;
        }
    } 
}
