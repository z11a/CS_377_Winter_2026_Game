using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] public Button startButton;
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

        startButton.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnStartButton()
    {
        Debug.Log("Start button pressed.");

        startButton.gameObject.SetActive(false);
        GameStateManager.waitingForPlayersToJoin = true;
        InputManager.playerInputManager.EnableJoining();
    }
}
