using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] public Button startMenuButton;
    [SerializeField] public Button startGameButton;           
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
        StartCoroutine(GameStateManager.instance.LoadGameplaySceneAsync());
    }
}
