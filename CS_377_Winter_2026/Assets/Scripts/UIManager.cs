using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Buttons")]
    public Button startMenuButton;
    public Button startGameButton;

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
    }

    // Update is called once per frame
    void Update()
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
        loadingScreen.gameObject.SetActive(true);
        Color fullAlpha = loadingScreen.color;
        fullAlpha.a = 0.0f;
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
}
