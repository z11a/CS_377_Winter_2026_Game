using System.Collections;
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

    private static float countdownTime = 3.0f;
    private static float roundTime = 90.0f;

    [SerializeField] private Transform player1GameplaySpawnPosition; 
    [SerializeField] private Transform player2GameplaySpawnPosition;

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
        }
        else
        {
            _gameState = GameState.mainMenu;
        }
    }

    // Update is called once per frame
    void Update()
    {

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

    public static IEnumerator StartPreRoundCountdown()
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
                break;
            }
        }
        yield return null;
    }
    private static void gameplaySceneStart()
    {
        Instantiate(InputManager.player1Input.gameObject, new Vector3(-5.0f, 3.2f, -7.75f), Quaternion.identity);
        Instantiate(InputManager.player1Input.gameObject, new Vector3(8.5f, 3.0f, -6.75f), Quaternion.identity);

        instance.StartCoroutine(StartPreRoundCountdown());
    }
    public static IEnumerator LoadGameplaySceneAsync()
    {
        yield return null;

        _gameState = GameState.loadingScreen;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scenes/GameplayScene");
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            Debug.Log("Progress: " + asyncLoad.progress * 100 + "%");

            if (asyncLoad.progress >= 0.9f)
            {
                Debug.Log("Loaded! Starting game in 2 seconds...");
                yield return new WaitForSeconds(2.0f);
                asyncLoad.allowSceneActivation = true;
                gameplaySceneStart();
            }

            yield return null;
        }
    } 
}
