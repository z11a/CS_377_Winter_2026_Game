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
        inGame
    }

    public static GameState _gameState;
    public static bool waitingForPlayersToJoin = false;

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

    }

    // Update is called once per frame
    void Update()
    {
        
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
            }

            yield return null;
        }
    } 
}
