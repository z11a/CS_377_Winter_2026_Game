using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class PlayerHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private Vector2 moveAmount;
    public float playerSpeed = 10.0f;
    private CharacterController controller;
    public float gravity = -2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        controller = GetComponent<CharacterController>();
    }   

    // Update is called once per frame

    void Update()
    {
        if (GameStateManager._gameState == GameStateManager.GameState.inGame)
        {
            Debug.Log("Waiting for movement input.");
            
            controller.Move(new Vector3(moveAmount.x, 0, moveAmount.y) * playerSpeed * Time.deltaTime);
        }
    }

    public void OnMove(InputValue value)
    {
        moveAmount = value.Get<Vector2>();
        Debug.Log("Moving...");
    }
}
