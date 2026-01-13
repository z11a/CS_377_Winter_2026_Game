using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    public static bool inGameplay = false;
    public static bool waitingForPlayersToJoin = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance != null && instance == this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(loadGameplaySceneAsync());
        }
    }

    IEnumerator loadGameplaySceneAsync()
    {
        yield return null;

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
