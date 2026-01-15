using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public static bool player1Joined = false;
    public static bool player2Joined = false;

    public static PlayerInputManager playerInputManager;

    [SerializeField] public static Vector3 player1SpawnPosition = new Vector3(-3.0f, 0.0f, -0.25f);
    [SerializeField] public static Vector3 player2SpawnPosition = new Vector3(3.0f, 0.0f, -0.25f);
    [SerializeField] public GameObject player1;
    [SerializeField] public GameObject player2;
    public static PlayerInput player1Input;
    public static PlayerInput player2Input;

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

        playerInputManager = this.GetComponent<PlayerInputManager>();
        playerInputManager.DisableJoining();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (!player1Joined)
        {
            player1Joined = true;
            playerInput.transform.position = player1SpawnPosition;
            Debug.Log("Player 1 joined.");
        }
        else if (!player2Joined)
        {
            player2Joined = true;
            playerInput.transform.position = player2SpawnPosition;
            Debug.Log("Player 2 joined.");
        }
    }
}
