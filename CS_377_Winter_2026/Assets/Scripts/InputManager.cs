using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public static bool player1Joined = false;
    public static bool player2Joined = false;

    public static PlayerInputManager playerInputManager;

    [SerializeField] public Vector3 player1SpawnPosition = new Vector3(-3.0f, 0.0f, -0.25f);
    [SerializeField] public Vector3 player2SpawnPosition = new Vector3(3.0f, 0.0f, -0.25f);

    public static GameObject player1;
    public static GameObject player2;
    public static PlayerInput player1Input;
    public static PlayerInput player2Input;

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
        playerInputManager.DisableJoining();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        {
            player1Input.SwitchCurrentActionMap("Player");
            player2Input.SwitchCurrentActionMap("Player");
        }
        else
        {
            player1Input.SwitchCurrentActionMap("UI");
            player2Input.SwitchCurrentActionMap("UI");
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!player1Joined)
        {
            Debug.Log("Player 1 joined.");
            player1Joined = true;
            player1Input = playerInput;
            playerInput.transform.position = player1SpawnPosition;
        }
        else if (!player2Joined)
        {
            Debug.Log("Player 2 joined.");
            player2Joined = true;
            player2Input = playerInput;
            playerInput.transform.position = player2SpawnPosition;

            UIManager.startGameButton.gameObject.SetActive(true);
            EventSystem.current.SetSelectedGameObject(UIManager.startGameButton.gameObject);
            playerInputManager.DisableJoining();
        }
    }
}
