using System.Collections;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;


    [SerializeField] public Transform player1StartSceneSpawnPosition;
    [SerializeField] public Transform player2StartSceneSpawnPosition;
    [SerializeField] public GameObject mousePrefab;
    [SerializeField] public GameObject ratPrefab;
    [SerializeField] public PlayerUIManager playerOneUIManager;
    [SerializeField] public PlayerUIManager playerTwoUIManager;

    [HideInInspector] public PlayerInputManager playerInputManager;
    [HideInInspector] public PlayerInput player1Input;
    [HideInInspector] public PlayerInput player2Input;
    [HideInInspector] public bool player1Joined = false;
    [HideInInspector] public bool player2Joined = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed extra InputManager");
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
        playerInputManager = this.GetComponent<PlayerInputManager>();
        playerInputManager.playerPrefab = mousePrefab;
        EventSystem.current.SetSelectedGameObject(UIManager.instance.startMenuButton.gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!player1Joined)
        {
            Debug.Log("Player 1 joined.");
            player1Joined = true;
            player1Input = playerInput;
            player1Input.GetComponent<PlayerHandler>().playerNumber = PlayerHandler.PlayerNumber.Player1;
            player1Input.GetComponent<Rigidbody>().position = player1StartSceneSpawnPosition.position;
            player1Input.SwitchCurrentActionMap("UI");
            playerOneUIManager.playerHandler = player1Input.GetComponent<PlayerHandler>();

            playerInputManager.playerPrefab = ratPrefab;

        }
        else if (!player2Joined)
        {
            Debug.Log("Player 2 joined.");
            player2Joined = true;
            player2Input = playerInput;
            player2Input.GetComponent<PlayerHandler>().playerNumber = PlayerHandler.PlayerNumber.Player2;
            player2Input.GetComponent<Rigidbody>().position = player2StartSceneSpawnPosition.position;
            player2Input.SwitchCurrentActionMap("UI");
            playerTwoUIManager.playerHandler = player2Input.GetComponent<PlayerHandler>();

            StartCoroutine(enableStartButton());
        }
    }

    private IEnumerator enableStartButton()     // This is in a coroutine because we need to pause one frame before enabling the start button. This is because clicking Submit on my Xbox controller to join the game also instantly presses the start button. 
    {
        yield return null;

        UIManager.instance.startGameButton.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(UIManager.instance.startGameButton.gameObject);
        playerInputManager.DisableJoining();
    }
}
